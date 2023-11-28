using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerScript : MonoBehaviour
{
    private enum State
    {
        Grounded,
        Aerial,
    }

    public KeyScript FastKey { get; set; }
    public KeyScript LockKey { get; set; }
    public KeyScript ReverseKey { get; set; }

    public const float FALL_GRAVITY = 5.0f;
    private const float JUMP_GRAVITY = 2.4f;
    private const float GROUND_GRAVITY = 10.0f; // a higher gravity makes the player move smoothly over the tops of slopes
    private const float JUMP_VELOCITY = 13.0f;
    private const float CLING_VELOCITY = -1.0f; // the maximum downward speed when pressed against a wall

    private const float WALK_SPEED = 7.0f; // per second
    private const float WALK_ACCEL = 100.0f; // per second^2

    private Rigidbody2D physicsBody;
    private Vector2 colliderSize;
    private State currentState;
    private PlayerInput input;
    private KeyScript activeKey;
    private KeyState selectedKey = KeyState.Fast;

    private float coyoteTime;
    private float keyCooldown;
    private bool? moveLockedRight = null; // prevents the player from moving in this direction. false is left, null is neither

    [SerializeField]
    public GameObject helper;
    private HelperCreature helperScript;

    [SerializeField]
    private Animator playerAnimation;

    public PlayerInput Input {  get { return input; } }

    void Start()
    {
        physicsBody = GetComponent<Rigidbody2D>();
        colliderSize = GetComponent<CapsuleCollider2D>().size;
        physicsBody.gravityScale = FALL_GRAVITY;
        currentState = State.Aerial;
        input = new PlayerInput();

        if(LevelData.Instance != null && LevelData.Instance.RespawnPoint.HasValue) {
            transform.position = LevelData.Instance.RespawnPoint.Value;
            CameraScript.Instance.SetInitialPosition();
        }

        helperScript = helper.GetComponent<HelperCreature>();
    }

    void FixedUpdate()
    {
        input.Update();
        Vector2 velocity = physicsBody.velocity;

        if(physicsBody.velocity.y <= 1.0f) {
            moveLockedRight = null;
        }

        // check if player left the boundaries of the level
        if(transform.position.x < LevelData.Instance.XMin) {
            transform.position = new Vector3(LevelData.Instance.XMin, transform.position.y, 0);
        }
        else if(transform.position.x  > LevelData.Instance.XMax) {
            int currentLevel = SceneManager.GetActiveScene().buildIndex;
            currentLevel++;
            if(currentLevel >= SceneManager.sceneCountInBuildSettings) {
                SceneManager.LoadScene("Titlescreen");
            } else {
                SceneManager.LoadScene(currentLevel);
            }
            //transform.position = new Vector3(maxX, transform.position.y, 0);
        }
        else if(transform.position.y + transform.localScale.y/2 < LevelData.Instance.YMin) {
            Die();
        }

        // vertical movement
        Vector2 floorNorm;
        bool onFloor = IsOnFloor(out floorNorm);

        switch(currentState) {
            case State.Aerial:
                if(velocity.y > JUMP_VELOCITY) {
                    // decrease the benefit from holding jump when launched upward
                    //physicsBody.gravityScale = (JUMP_GRAVITY + FALL_GRAVITY) / 2;
                }

                // extend jump height while jump is held
                if(physicsBody.gravityScale != FALL_GRAVITY && 
                    (velocity.y < 0 || velocity.y > JUMP_VELOCITY || !input.IsPressed(PlayerInput.Action.Jump))
                ) {
                    physicsBody.gravityScale = FALL_GRAVITY;
                }
                
                Direction adjWallDir = GetAdjacentWallDireciton();
                if(adjWallDir != Direction.None) {
                    SetAnimation("Wallslide");
                }
                else if(velocity.y < 0) {
                    SetAnimation("Falling");
                }

                // cling to walls
                if(velocity.y < CLING_VELOCITY && 
                    (adjWallDir == Direction.Left && input.IsPressed(PlayerInput.Action.Left) || adjWallDir == Direction.Right && input.IsPressed(PlayerInput.Action.Right))
                ) {
                    velocity.y = CLING_VELOCITY;
                }

                // wall jump
                if(adjWallDir != Direction.None && input.JustPressed(PlayerInput.Action.Jump)) {
                    physicsBody.gravityScale = JUMP_GRAVITY;
                    if(velocity.y < 0) {
                        velocity.y = 0;
                    }

                    bool boosted = false;
                    const float WALL_JUMP_SPEED = 11f;
                    if(velocity.y > WALL_JUMP_SPEED) {
                        // less boost if there is already upward momentum
                        velocity.y += WALL_JUMP_SPEED / 4f;
                        boosted = true;
                    }
                    else {
                        velocity.y += WALL_JUMP_SPEED;
                    }

                    int jumpDirection = (adjWallDir == Direction.Left ? 1 : -1);
                    if(Mathf.Sign(velocity.x) != jumpDirection) {
                        velocity.x = 0;
                    }
                    velocity.x += jumpDirection * (boosted ? 10f : 6.0f);
                    moveLockedRight = (jumpDirection == -1);
                    SetAnimation("Jumping");
                }

                // allow jump during coyote time
                if(coyoteTime > 0) {
                    if(input.JustPressed(PlayerInput.Action.Jump)) {
                        Jump(ref velocity);
                        SetAnimation("Jumping");
                        coyoteTime = 0;
                    } else {
                        coyoteTime -= Time.deltaTime;
                    }
                }

                // land on the ground
                if(onFloor) {
                    currentState = State.Grounded;
                    physicsBody.gravityScale = GROUND_GRAVITY;
                    SetAnimation(null);
                }
                break;

            case State.Grounded:
                if(input.JumpBuffered) { // jump buffer allows a jump when pressed slightly before landing
                    Jump(ref velocity);
                }
                else if(!onFloor) {
                    // fall off platform
                    SetAnimation("Falling");
                    currentState = State.Aerial;
                    coyoteTime = 0.08f;
                    physicsBody.gravityScale = FALL_GRAVITY;
                }
                
                if(floorNorm.y < 0.9f) {
                    // apply a force to stay still on slopes
                    Vector2 gravity = Physics2D.gravity * physicsBody.gravityScale;
                    Vector2 normalForce = -Vector3.Project(gravity, -floorNorm);
                    Vector2 downSlope = gravity + normalForce;
                    physicsBody.AddForce(-downSlope);
                }
                break;
        }

        // horizontal movement
        float friction = (currentState == State.Grounded ? 30f : 5f);
        Vector2 slopeLeft = Vector2.left;
        if(onFloor) {
            slopeLeft = Vector2.Perpendicular(floorNorm);
        }
        Vector2 slopeRight = -slopeLeft;

        bool moveRight = input.IsPressed(PlayerInput.Action.Right) && moveLockedRight != true && (Vector2.Dot(velocity, slopeRight) <= 0 || Vector3.Project(velocity, slopeRight).sqrMagnitude <= WALK_SPEED * WALK_SPEED + Mathf.Epsilon);
        bool moveLeft = input.IsPressed(PlayerInput.Action.Left) && moveLockedRight != false && (Vector2.Dot(velocity, slopeLeft) <= 0 || Vector3.Project(velocity, slopeLeft).sqrMagnitude <= WALK_SPEED * WALK_SPEED + Mathf.Epsilon);
        if(moveRight == moveLeft && velocity.x != 0) { // both pressed is same as neither pressed
            if(currentState == State.Grounded) {
                SetAnimation(null);
            }

            // apply friction
            Vector2 fricDir = velocity.x > 0 ? slopeLeft : slopeRight;
            if(Mathf.Abs(velocity.x) >= 0.1f) {
                velocity += friction * Time.deltaTime * fricDir;
            }

            // check if slowed to a stop
            if(onFloor && velocity.sqrMagnitude < 0.1f) {
                velocity = Vector2.zero;
            }
            else if(!onFloor && Vector2.Dot(velocity, fricDir) > 0) {
                velocity.x = 0;
            }
        }
        else if(moveRight || moveLeft) {
            // walk (or midair strafe)
            transform.localScale = new Vector3((moveRight ? 1 : -1) * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            if(currentState == State.Grounded) {
                SetAnimation("Running");
            }

            Vector2 moveDir = (moveRight ? slopeRight : slopeLeft);
            velocity += WALK_ACCEL * Time.deltaTime * moveDir;

            // cap walk speed
            if(Vector2.Dot(velocity, moveDir) > 0 && Vector3.Project(velocity, moveDir).sqrMagnitude > WALK_SPEED * WALK_SPEED) {
                velocity = (Vector2)Vector3.Project(velocity, (onFloor ? floorNorm : Vector2.up)) + WALK_SPEED * moveDir;
            }
        }

        physicsBody.velocity = velocity;

        // manage key ability
        if(FastKey != null && input.JustPressed(PlayerInput.Action.FastKey)) {
            selectedKey = KeyState.Fast;
            if(activeKey == FastKey) {
                activeKey.Detach();
                activeKey = null;
            }
        }
        else if(LockKey != null && input.JustPressed(PlayerInput.Action.LockKey)) {
            selectedKey = KeyState.Lock;
            if(activeKey == LockKey) {
                activeKey.Detach();
                activeKey = null;
            }
        }
        else if(ReverseKey != null && input.JustPressed(PlayerInput.Action.ReverseKey)) {
            selectedKey = KeyState.Reverse;
            if(activeKey == ReverseKey) {
                activeKey.Detach();
                activeKey = null;
            }
        }

        if(keyCooldown <= 0) {
            Vector2 keyDirection = Vector2.zero;
            if(input.JustPressed(PlayerInput.Action.ThrowUp)) {
                keyDirection = Vector2.up;
            }
            else if(input.JustPressed(PlayerInput.Action.ThrowDown)) {
                keyDirection = Vector2.down;
            }
            else if(input.JustPressed(PlayerInput.Action.ThrowLeft)) {
                keyDirection = Vector2.left;
            }
            else if(input.JustPressed(PlayerInput.Action.ThrowRight)) {
                keyDirection = Vector2.right;
            }

            // send key attack
            if(keyDirection != Vector2.zero) {
                // determine attack direction
                if(input.MouseClicked()) {
                    // use mouse position to determine the direction
                    Vector3 mouseDir = input.GetMouseWorldPosition() - transform.position;
                    if(Mathf.Abs(mouseDir.x) > Mathf.Abs(mouseDir.y)) {
                        mouseDir.y = 0;
                    } else {
                        mouseDir.x = 0;
                    }
                    keyDirection = mouseDir.normalized;
                }

                switch(selectedKey) {
                    case KeyState.Fast:
                        activeKey = FastKey;
                        break;
                    case KeyState.Lock:
                        activeKey = LockKey;
                        break;
                    case KeyState.Reverse:
                        activeKey = ReverseKey;
                        break;
                }

                if(activeKey != null) {
                    activeKey.Attack(keyDirection);
                    keyCooldown = 0.1f;
                }
            }
        }
        else {
            keyCooldown -= Time.deltaTime;
        }
    }

    // restarts the level from the most recent checkpoint
    public void Die() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void Jump(ref Vector2 newVelocity) {
        newVelocity.y = JUMP_VELOCITY;
        physicsBody.gravityScale = JUMP_GRAVITY;
        SetAnimation("Jumping");
        currentState = State.Aerial;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        Vector2 floorNormal;
        if(collision.gameObject.tag == "Wall" && physicsBody.velocity.y < 0 && IsOnFloor(out floorNormal) && floorNormal != Vector2.zero && floorNormal != Vector2.up) {
            physicsBody.velocity *= 0.5f; // prevent sliding down slopes
        }
    }

    // null sets no animation
    private void SetAnimation(String animationState) {
        playerAnimation.SetBool("Falling", false);
        playerAnimation.SetBool("Running", false);
        playerAnimation.SetBool("Jumping", false);
        playerAnimation.SetBool("Wallslide", false);

        if(animationState != null) {
            playerAnimation.SetBool(animationState, true);
        }
    }

    // uses raycasts to determine if the player is standing on a surface
    private bool IsOnFloor(out Vector2 normal) {
        const float BUFFER = 0.2f;
        float halfRadius = colliderSize.x / 2f;
        RaycastHit2D left = Physics2D.Raycast(new Vector3(transform.position.x - colliderSize.x / 2f, transform.position.y - colliderSize.y / 2f + halfRadius, 0), Vector2.down, 10, LayerMask.NameToLayer("Player"));
        RaycastHit2D mid = Physics2D.Raycast(new Vector3(transform.position.x, transform.position.y - colliderSize.y / 2f, 0), Vector2.down, 10, LayerMask.NameToLayer("Player"));
        RaycastHit2D right = Physics2D.Raycast(new Vector3(transform.position.x + colliderSize.x / 2f, transform.position.y - colliderSize.y / 2f + halfRadius, 0), Vector2.down, 10, LayerMask.NameToLayer("Player"));

        normal = (mid.collider == null ? Vector2.zero : mid.normal);

        return mid.collider != null && mid.distance < BUFFER || left.collider != null && left.distance < halfRadius + BUFFER || right.collider != null && right.distance < halfRadius + BUFFER;
    }

    // checks if the player's left and right sides are against any surfaces. Returns Direction.None for no wall, and left or right if there is a wall
    private Direction GetAdjacentWallDireciton() {
        float left = transform.position.x - colliderSize.x / 2f;
        float right = transform.position.x + colliderSize.x / 2f;
        float top = transform.position.y + colliderSize.y / 2f - colliderSize.x / 2f;
        float mid = transform.position.y;
        float bottom = transform.position.y - colliderSize.y / 2f + colliderSize.x / 2f;

        const float BUFFER = 0.2f;

        RaycastHit2D leftTop = Physics2D.Raycast(new Vector3(left, top, 0), Vector2.left, 10, LayerMask.NameToLayer("Player"));
        RaycastHit2D leftMid = Physics2D.Raycast(new Vector3(left, mid, 0), Vector2.left, 10, LayerMask.NameToLayer("Player"));
        RaycastHit2D leftBot = Physics2D.Raycast(new Vector3(left, bottom, 0), Vector2.left, 10, LayerMask.NameToLayer("Player"));

        RaycastHit2D rightTop = Physics2D.Raycast(new Vector3(right, top, 0), Vector2.right, 10, LayerMask.NameToLayer("Player"));
        RaycastHit2D rightMid = Physics2D.Raycast(new Vector3(right, mid, 0), Vector2.right, 10, LayerMask.NameToLayer("Player"));
        RaycastHit2D rightBot = Physics2D.Raycast(new Vector3(right, bottom, 0), Vector2.right, 10, LayerMask.NameToLayer("Player"));

        if(leftTop.collider != null && leftTop.distance < BUFFER || leftMid.collider != null && leftMid.distance < BUFFER || leftBot.collider != null && leftBot.distance < BUFFER) {
            return Direction.Left;
        }
        if(rightTop.collider != null && rightTop.distance < BUFFER || rightMid.collider != null && rightMid.distance < BUFFER || rightBot.collider != null && rightBot.distance < BUFFER) {
            return Direction.Right;
        }

        return Direction.None;
    }
}
