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
    private const float JUMP_VELOCITY = 13.0f;
    private const float CLING_VELOCITY = -1.0f; // the maximum downward speed when pressed against a wall

    private const float WALK_SPEED = 7.0f; // per second
    private const float WALK_ACCEL = 100.0f; // per second^2

    private Rigidbody2D physicsBody;
    private Vector2 colliderSize;
    private State currentState;
    private PlayerInput input;
    private KeyScript activeKey;

    private bool jumpHeld;
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
        currentState = State.Grounded;
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
        float friction = 0f; // per second^2

        if(physicsBody.velocity.y <= 1.5f) {
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
        switch(currentState)
        {
            case State.Aerial:
                friction = 5f;
                playerAnimation.SetBool("Running", false);
                // extend jump height while jump is held
                if(physicsBody.velocity.y < 0 || !input.IsPressed(PlayerInput.Action.Jump)) {
                    jumpHeld = false;
                }

                // determine gravity
                if(jumpHeld) {
                    physicsBody.gravityScale = JUMP_GRAVITY;
                    playerAnimation.SetBool("Running", false);
                    playerAnimation.SetBool("Falling", false);
                    playerAnimation.SetBool("Jumping", true);

                    if (physicsBody.velocity.y > JUMP_VELOCITY) {
                        physicsBody.gravityScale = (JUMP_GRAVITY + FALL_GRAVITY) / 2;
                    }
                } else {
                    physicsBody.gravityScale = FALL_GRAVITY;
                    playerAnimation.SetBool("Jumping", false);
                    playerAnimation.SetBool("Falling", true);
                    playerAnimation.SetBool("Running", false);
                    playerAnimation.SetBool("Wallslide", false);
                }

                if(physicsBody.gravityScale != FALL_GRAVITY && 
                    (physicsBody.velocity.y < 0 || !input.IsPressed(PlayerInput.Action.Jump))
                ) {
                    physicsBody.gravityScale = FALL_GRAVITY;
                    playerAnimation.SetBool("Jumping", false);
                    playerAnimation.SetBool("Running", false);
                    playerAnimation.SetBool("Falling", true);
                    playerAnimation.SetBool("Wallslide", false);
                }
                
                Direction adjWallDir = GetAdjacentWallDireciton();

                // cling to walls
                if(velocity.y < CLING_VELOCITY && 
                    (adjWallDir == Direction.Left && input.IsPressed(PlayerInput.Action.Left) || adjWallDir == Direction.Right && input.IsPressed(PlayerInput.Action.Right))
                ) {
                    velocity.y = CLING_VELOCITY;
                    playerAnimation.SetBool("Wallslide", true);
                    playerAnimation.SetBool("Running", false);
                    playerAnimation.SetBool("Jumping", false);
                    playerAnimation.SetBool("Falling", false);
                }

                // wall jump
                if(adjWallDir != Direction.None && input.JustPressed(PlayerInput.Action.Jump)) {
                    int jumpDirection = (adjWallDir == Direction.Left ? 1 : -1);
                    velocity.y = 11.0f;
                    velocity.x = jumpDirection * 6.0f;
                    moveLockedRight = (jumpDirection == -1);
                    jumpHeld = true;
                    playerAnimation.SetBool("Falling", false);
                    playerAnimation.SetBool("Running", false);
                    playerAnimation.SetBool("Jumping", true);
                    playerAnimation.SetBool("Wallslide", false);
                }

                // allow jump during coyote time
                if(coyoteTime > 0) {
                    if(input.JustPressed(PlayerInput.Action.Jump)) {
                        Jump(ref velocity);
                        playerAnimation.SetBool("Falling", false);
                        playerAnimation.SetBool("Running", false);
                        playerAnimation.SetBool("Wallslide", false);
                        playerAnimation.SetBool("Jumping", true);
                        coyoteTime = 0;
                    } else {
                        coyoteTime -= Time.deltaTime;
                    }
                }

                // land on the ground
                Vector2 floorAngle;
                if(IsOnFloor(out floorAngle)) {
                    currentState = State.Grounded;
                    physicsBody.gravityScale = FALL_GRAVITY;
                    playerAnimation.SetBool("Jumping", false);
                    playerAnimation.SetBool("Falling", false);
                    playerAnimation.SetBool("Running", false);
                }
                break;

            case State.Grounded:
                friction = 30f;
                playerAnimation.SetBool("Wallslide", false);
                playerAnimation.SetBool("Running", false);
                Vector2 floorNorm;
                bool onFloor = IsOnFloor(out floorNorm);
                if(input.JumpBuffered) { // jump buffer allows a jump when pressed slightly before landing
                    Jump(ref velocity);
                    playerAnimation.SetBool("Running", false);
                    playerAnimation.SetBool("Wallslide", false);
                    playerAnimation.SetBool("Jumping", true);
                }
                else if(!onFloor) {
                    // fall off platform
                    playerAnimation.SetBool("Running", false);
                    playerAnimation.SetBool("Wallslide", false);
                    playerAnimation.SetBool("Falling", true);
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

                    playerAnimation.SetBool("Running", false);
                    playerAnimation.SetBool("Wallslide", false);
                    playerAnimation.SetBool("Falling", false);
                    playerAnimation.SetBool("Wallslide", false);
                }
                break;
        }

        // horizontal movement
        float walkAccel = WALK_ACCEL * Time.deltaTime;
        bool moveRight = input.IsPressed(PlayerInput.Action.Right) && velocity.x <= WALK_SPEED + Mathf.Epsilon && moveLockedRight != true;
        bool moveLeft = input.IsPressed(PlayerInput.Action.Left) && velocity.x >= -WALK_SPEED - Mathf.Epsilon && moveLockedRight != false;
        if(moveRight == moveLeft) { // both pressed is same as neither pressed
            // apply friction
            if (velocity != Vector2.zero) {
                float reduction = friction * Time.deltaTime;
                if (Mathf.Abs(velocity.x) <= reduction) {
                    // prevent passing 0
                    velocity.x = 0;
                } else {
                    velocity.x += (velocity.x > 0 ? -1 : 1) * friction * Time.deltaTime;
                }
            } else
            {
                playerAnimation.SetBool("Running", false);
            }
        }
        else if(moveRight) {
            velocity.x += walkAccel;
            if (playerAnimation.GetBool("Wallslide") != true && (playerAnimation.GetBool("Falling") != true))
            {
                playerAnimation.SetBool("Running", true);
            }
            if (velocity.x > WALK_SPEED) {
                velocity.x = WALK_SPEED;
            }

            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if(moveLeft) {
            velocity.x -= walkAccel;
            if(playerAnimation.GetBool("Wallslide") != true && (playerAnimation.GetBool("Falling") != true))
            {
                playerAnimation.SetBool("Running", true);
            }
            
            if (velocity.x < -WALK_SPEED) {
                velocity.x = -WALK_SPEED;
            }
                
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        physicsBody.velocity = velocity;

        // manage key ability
        if(keyCooldown <= 0) {
            KeyState usedKey = KeyState.Normal;
            if(FastKey != null && input.JustPressed(PlayerInput.Action.FastKey)) {
                usedKey = KeyState.Fast;
            }
            else if(LockKey != null && input.JustPressed(PlayerInput.Action.LockKey)) {
                usedKey = KeyState.Lock;
            }
            else if(ReverseKey != null && input.JustPressed(PlayerInput.Action.ReverseKey)) {
                usedKey = KeyState.Reverse;
            }

            if(usedKey != KeyState.Normal) {
                // send key attack
                if(activeKey != null && usedKey == activeKey.Type) {
                    // remove active key
                    activeKey.Detach();
                    activeKey = null;
                }

                // determine attack direction
                Vector2 attackDirection = (transform.localScale.x > 0 ? Vector2.right : Vector2.left);
                if(input.MouseClicked()) {
                    Debug.Log("hello i have clicked mouse");
                    // use mouse position to determine the direction
                    Vector3 mouseDir = input.GetMouseWorldPosition() - transform.position;
                    if(Mathf.Abs(mouseDir.x) > Mathf.Abs(mouseDir.y)) {
                        mouseDir.y = 0;
                    } else {
                        mouseDir.x = 0;
                    }
                    attackDirection = mouseDir.normalized;
                    Debug.Log(input.GetMouseWorldPosition());
                }
                else if(!input.IsPressed(PlayerInput.Action.Right) && !input.IsPressed(PlayerInput.Action.Left)) {
                    if(input.IsPressed(PlayerInput.Action.Up)) {
                        attackDirection = Vector2.up;
                    }
                    if(input.IsPressed(PlayerInput.Action.Down)) {
                        attackDirection = Vector2.down;
                    }
                }

                
                if(!input.IsPressed(PlayerInput.Action.Right) && !input.IsPressed(PlayerInput.Action.Left)) {
                    if(input.IsPressed(PlayerInput.Action.Up)) {
                        attackDirection = Vector2.up;
                    }
                    if(input.IsPressed(PlayerInput.Action.Down)) {
                        attackDirection = Vector2.down;
                    }
                }

                switch(usedKey) {
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

                activeKey.Attack(attackDirection);
                keyCooldown = 0.1f;
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
        playerAnimation.SetBool("Running", false);
        playerAnimation.SetBool("Jumping", true);
        playerAnimation.SetBool("Wallslide", false);
        currentState = State.Aerial;
        jumpHeld = true;
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
        float top = transform.position.y + colliderSize.y / 2f;
        float mid = transform.position.y;
        float bottom = transform.position.y - colliderSize.y / 2f;

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
