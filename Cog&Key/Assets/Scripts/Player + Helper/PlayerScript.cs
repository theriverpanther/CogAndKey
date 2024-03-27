using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerScript : MonoBehaviour
{
    public enum State
    {
        Grounded,
        Aerial,
    }

    public const float FALL_GRAVITY = 5.0f;
    public const float JUMP_GRAVITY = 2.4f;
    private const float GROUND_GRAVITY = 10.0f; // a higher gravity makes the player move smoothly over the tops of slopes
    private const float JUMP_VELOCITY = 13.0f;
    private const float CLING_VELOCITY = -1.0f; // the maximum downward speed when pressed against a wall

    private const float WALK_SPEED = 7.0f; // per second
    private const float WALK_ACCEL = 100.0f; // per second^2

    private Rigidbody2D physicsBody;
    private Vector2 colliderHalfSize;
    private PlayerInput input;

    private float coyoteTime;
    private bool? moveLockedRight = null; // prevents the player from moving in this direction. false is left, null is neither
    public Vector2? CoyoteMomentum { get; set; } // allows mechanics like vertical platforms to give momentum buffers

    private GameObject helper;
    private HelperCreature helperScript;

    [SerializeField]
    public Animator playerAnimation;

    public State CurrentState { get; private set; }
    public bool HasWallSlid { get; private set; }
    public Vector2 Velocity { get { return physicsBody.velocity; } }
    public Dictionary<KeyState, bool> EquippedKeys { get; private set; }
    public static PlayerScript CurrentPlayer { get; private set; }

    private void Awake()
    {
        CurrentPlayer = this;
        EquippedKeys = new Dictionary<KeyState, bool>() {
            { KeyState.Lock, false },
            { KeyState.Fast, false },
            { KeyState.Reverse, false }
        };
    }

    void Start()
    {
        physicsBody = GetComponent<Rigidbody2D>();
        colliderHalfSize = GetComponent<BoxCollider2D>().size / 2f;
        physicsBody.gravityScale = FALL_GRAVITY;
        CurrentState = State.Aerial;
        input = PlayerInput.Instance;
        input.Player = this;

        helper = GameObject.FindGameObjectWithTag("Helper");

        if (LevelData.Instance != null && LevelData.Instance.RespawnPoint.HasValue) {
            transform.position = LevelData.Instance.RespawnPoint.Value;
            CameraController.Instance?.SetInitialPosition();
        }

        helperScript = helper?.GetComponent<HelperCreature>();
    }

    void FixedUpdate()
    {
        input.Update();
        Vector2 velocity = physicsBody.velocity;
        playerAnimation.SetFloat("velocity", physicsBody.velocity.y);

        if(physicsBody.velocity.y <= 0.5f) {
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
        GameObject floorObject = null;
        float groundDistance;
        bool onFloor = IsOnFloor(out floorNorm, out floorObject, out groundDistance);
        if(onFloor) {
            CoyoteMomentum = null;
        }
        
        switch(CurrentState) {
            case State.Aerial:
                if(physicsBody.gravityScale != FALL_GRAVITY) {
                    physicsBody.gravityScale = JUMP_GRAVITY;
                    if(velocity.y >  1.5f * JUMP_VELOCITY) {
                        physicsBody.gravityScale = (JUMP_GRAVITY + FALL_GRAVITY) / 2f; // reduce jump boost when there is a lot of upward momentum
                    }

                    if(velocity.y < 0 || !input.IsPressed(PlayerInput.Action.Jump)) {
                        physicsBody.gravityScale = FALL_GRAVITY; // extend jump height while jump is held
                    }
                }
                
                bool topAgainstWall = false;
                Direction adjWallDir = GetAdjacentWallDireciton(out topAgainstWall);

                if (velocity.y < 0 && groundDistance > 1.0f)
                {
                    SetAnimation("Falling");
                }

                // cling to walls
                if (velocity.y < CLING_VELOCITY && topAgainstWall &&
                    (adjWallDir == Direction.Left && input.IsPressed(PlayerInput.Action.Left) || adjWallDir == Direction.Right && input.IsPressed(PlayerInput.Action.Right))
                ) {
                    velocity.y = CLING_VELOCITY;
                    HasWallSlid = true;
                    SetAnimation("Wallslide");
                }


                // wall jump
                if(adjWallDir != Direction.None && input.JustPressed(PlayerInput.Action.Jump)) {
                    HasWallSlid = true;
                    coyoteTime = 0;
                    physicsBody.gravityScale = JUMP_GRAVITY;
                    const float WALL_JUMP_SPEED = 11f;
                    if(velocity.y < WALL_JUMP_SPEED) {
                        velocity.y = WALL_JUMP_SPEED;
                    } else {
                        physicsBody.gravityScale = FALL_GRAVITY; // no extendable jump height when launching off of a vertical conveyor belt
                    }

                    int jumpDirection = (adjWallDir == Direction.Left ? 1 : -1);
                    if(Mathf.Sign(velocity.x) != jumpDirection) {
                        velocity.x = 0;
                    }
                    velocity.x += jumpDirection * 6.0f;
                    moveLockedRight = (jumpDirection == -1);
                    SetAnimation("Jumping");
                }

                // allow jump during coyote time
                if(coyoteTime > 0 && input.JustPressed(PlayerInput.Action.Jump)) {
                    Jump(ref velocity, true);
                    SetAnimation("Jumping");
                    coyoteTime = 0;
                }

                // land on the ground
                if(onFloor) {
                    CurrentState = State.Grounded;
                    physicsBody.gravityScale = GROUND_GRAVITY;
                    SetAnimation(null);
                }
                break;

            case State.Grounded:
                HasWallSlid = false;
                playerAnimation.SetBool("Falling", false);
                playerAnimation.SetBool("Wallslide", false);

                // check if walking into a slope
                bool movingRight = input.IsPressed(PlayerInput.Action.Right);
                bool movingLeft = input.IsPressed(PlayerInput.Action.Left);
                if(floorNorm.y > .98f && movingLeft != movingRight) {
                    float side = transform.position.x + (movingRight ? 1 : -1) * colliderHalfSize.x;
                    Vector2 direction = movingRight ? Vector2.right : Vector2.left;
                    RaycastHit2D floorCast = Physics2D.Raycast(new Vector3(side, transform.position.y - colliderHalfSize.y + 0.05f, 0), direction, 0.15f, LayerMask.NameToLayer("Player"));
                    RaycastHit2D hipCast = Physics2D.Raycast(new Vector3(side, transform.position.y, 0), direction, 0.15f, LayerMask.NameToLayer("Player"));
                    if(hipCast.collider == null && floorCast.collider != null) {
                        floorNorm = floorCast.normal;
                    }
                }

                if(floorNorm.y < 0.9f) {
                    // apply a force to stay still on slopes
                    Vector2 gravity = Physics2D.gravity * physicsBody.gravityScale;
                    Vector2 normalForce = -Vector3.Project(gravity, -floorNorm);
                    Vector2 downSlope = gravity + normalForce;
                    physicsBody.AddForce(-downSlope);
                }

                if(input.JumpBuffered) { // jump buffer allows a jump when pressed slightly before landing
                    Jump(ref velocity, floorObject != null && floorObject.GetComponent<MovingWallScript>() != null);
                }
                else if(!onFloor) {
                    // fall off platform
                    CurrentState = State.Aerial;
                    coyoteTime = 0.125f;
                    physicsBody.gravityScale = FALL_GRAVITY;
                }
                break;
        }

        // horizontal movement
        float friction = (CurrentState == State.Grounded ? 30f : 5f);
        Vector2 slopeLeft = Vector2.left;
        if(CurrentState == State.Grounded) {
            slopeLeft = Vector2.Perpendicular(floorNorm);
        }
        Vector2 slopeRight = -slopeLeft;

        bool moveRight = input.IsPressed(PlayerInput.Action.Right) && moveLockedRight != true && (Vector2.Dot(velocity, slopeRight) <= 0 || Vector3.Project(velocity, slopeRight).sqrMagnitude <= WALK_SPEED * WALK_SPEED + Mathf.Epsilon);
        bool moveLeft = input.IsPressed(PlayerInput.Action.Left) && moveLockedRight != false && (Vector2.Dot(velocity, slopeLeft) <= 0 || Vector3.Project(velocity, slopeLeft).sqrMagnitude <= WALK_SPEED * WALK_SPEED + Mathf.Epsilon);
        if(moveRight == moveLeft && velocity.x != 0) { // both pressed is same as neither pressed

            // apply friction
            Vector2 fricDir = velocity.x > 0 ? slopeLeft : slopeRight;
            if(Mathf.Abs(velocity.x) >= 0.1f) {
                velocity += friction * Time.deltaTime * fricDir;
            }

            // check if slowed to a stop
            if(onFloor && velocity.sqrMagnitude < 0.1f) {
                velocity = Vector2.zero;
                playerAnimation.SetBool("Running", false);
            }
            else if(!onFloor && Vector2.Dot(velocity, fricDir) > 0) {
                velocity.x = 0;
                playerAnimation.SetBool("Running", false);
            }
        }
        else if(moveRight || moveLeft) {
            // walk (or midair strafe)
            transform.localScale = new Vector3((moveRight ? 1 : -1) * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            transform.GetChild(1).localScale = new Vector3((moveRight ? 1 : -1) * Mathf.Abs(transform.GetChild(1).localScale.x), transform.GetChild(1).localScale.y, transform.GetChild(1).localScale.z);

            if(CurrentState == State.Grounded) {
                playerAnimation.SetBool("Running", true);
            }

            Vector2 moveDir = (moveRight ? slopeRight : slopeLeft);

            velocity += WALK_ACCEL * Time.deltaTime * moveDir;

            // cap walk speed
            if(Vector2.Dot(velocity, moveDir) > 0 && Vector3.Project(velocity, moveDir).sqrMagnitude > WALK_SPEED * WALK_SPEED) {
                velocity = (Vector2)Vector3.Project(velocity, (onFloor ? floorNorm : Vector2.up)) + WALK_SPEED * moveDir;
            }
        }

        physicsBody.velocity = velocity;
        playerAnimation.SetFloat("velocity", physicsBody.velocity.y);

        if (coyoteTime > 0) {
            coyoteTime -= Time.deltaTime;
            if(coyoteTime <= 0) {
                CoyoteMomentum = null;
            }
        }
    }

    // restarts the level from the most recent checkpoint
    public void Die() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void Jump(ref Vector2 newVelocity, bool applyMomentum) {
        if(applyMomentum) {
            if(CoyoteMomentum.HasValue) {
                newVelocity = CoyoteMomentum.Value;
            }
            else if(newVelocity.y < 0) {
                newVelocity.y = 0;
            }
            newVelocity.y += JUMP_VELOCITY;
        } else {
            newVelocity.y = JUMP_VELOCITY;
        }
        physicsBody.gravityScale = JUMP_GRAVITY;
        SetAnimation("Jumping");
        CurrentState = State.Aerial;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        Vector2 floorNormal;
        GameObject hitSurface;
        float groundDist;
        if(collision.gameObject.tag == "Wall" && physicsBody.velocity.y < 0 && IsOnFloor(out floorNormal, out hitSurface, out groundDist) && floorNormal != Vector2.zero && floorNormal != Vector2.up) {
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
    private bool IsOnFloor(out Vector2 normal, out GameObject hitSurface, out float groundDistance) {
        groundDistance = 0;
        const float BUFFER = 0.2f;
        RaycastHit2D left = Physics2D.Raycast(new Vector3(transform.position.x - colliderHalfSize.x, transform.position.y - colliderHalfSize.y, 0), Vector2.down, 10, LayerMask.NameToLayer("Player"));
        RaycastHit2D right = Physics2D.Raycast(new Vector3(transform.position.x + colliderHalfSize.x, transform.position.y - colliderHalfSize.y, 0), Vector2.down, 10, LayerMask.NameToLayer("Player"));

        groundDistance = Mathf.Max(right.distance, left.distance);

        bool leftOnSurface = left.collider != null && left.distance < BUFFER;
        bool rightOnSurface = right.collider != null && right.distance < BUFFER;

        if(leftOnSurface && rightOnSurface) {
            // if split between two surfaces, favor the direction the player is trying to move
            bool movingRight = input.IsPressed(PlayerInput.Action.Right);
            bool movingLeft = input.IsPressed(PlayerInput.Action.Left);
            if(movingRight && !movingLeft) {
                leftOnSurface = false;
            }
            if(movingLeft && !movingRight) {
                rightOnSurface = false;
            }
        }

        if(leftOnSurface && rightOnSurface) {
            normal = (left.normal + right.normal) / 2f;
            hitSurface = right.collider.gameObject;
        }
        else if(leftOnSurface) {
            normal = left.normal;
            hitSurface = left.collider.gameObject;
        }
        else if(rightOnSurface) {
            normal = right.normal;
            hitSurface = right.collider.gameObject;
        }
        else {
            normal = Vector2.zero;
            hitSurface = null;
        }

        return leftOnSurface || rightOnSurface;
    }

    // checks if the player's left and right sides are against any surfaces. Returns Direction.None for no wall, and left or right if there is a wall
    private Direction GetAdjacentWallDireciton(out bool topAdjacent) {
        float left = transform.position.x - colliderHalfSize.x;
        float right = transform.position.x + colliderHalfSize.x;
        float top = transform.position.y + colliderHalfSize.y;
        float mid = transform.position.y;
        float bottom = transform.position.y - colliderHalfSize.y;

        const float BUFFER = 0.2f;

        RaycastHit2D leftTop = Physics2D.Raycast(new Vector3(left, top, 0), Vector2.left, 10, LayerMask.NameToLayer("Player"));
        RaycastHit2D rightTop = Physics2D.Raycast(new Vector3(right, top, 0), Vector2.right, 10, LayerMask.NameToLayer("Player"));

        RaycastHit2D leftBot = Physics2D.Raycast(new Vector3(left, bottom, 0), Vector2.left, 10, LayerMask.NameToLayer("Player"));
        RaycastHit2D rightBot = Physics2D.Raycast(new Vector3(right, bottom, 0), Vector2.right, 10, LayerMask.NameToLayer("Player"));
        RaycastHit2D leftMid = Physics2D.Raycast(new Vector3(left, mid, 0), Vector2.left, 10, LayerMask.NameToLayer("Player"));
        RaycastHit2D rightMid = Physics2D.Raycast(new Vector3(right, mid, 0), Vector2.right, 10, LayerMask.NameToLayer("Player"));

        topAdjacent = true;
        if(leftTop.collider != null && leftTop.distance < BUFFER) {
            return Direction.Left;
        }

        if(rightTop.collider != null && rightTop.distance < BUFFER) {
            return Direction.Right;
        }

        topAdjacent = false;
        if(leftBot.collider != null && leftBot.distance < BUFFER || leftMid.collider != null && leftMid.distance < BUFFER) {
            return Direction.Left;
        }

        if(rightBot.collider != null && rightBot.distance < BUFFER || rightMid.collider != null && rightMid.distance < BUFFER) {
            return Direction.Right;
        }

        return Direction.None;
    }
}
