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

    private const float FALL_GRAVITY = 5.0f;
    private const float JUMP_GRAVITY = 2.4f;
    private const float JUMP_VELOCITY = 13.0f;
    private const float CLING_VELOCITY = -1.0f; // the maximum downward speed when pressed against a wall

    private const float WALK_SPEED = 7.0f; // per second
    private const float WALK_ACCEL = 100.0f; // per second^2

    private Rigidbody2D physicsBody;
    private State currentState;
    private PlayerInput input;
    private KeyScript activeKey;

    private bool jumpHeld;
    private float coyoteTime;
    private float keyCooldown;
    private bool? moveLockedRight = null; // prevents the player from moving in this direction. false is left, null is neither

    public Rect CollisionArea {  get {
            Vector2 size = GetComponent<BoxCollider2D>().bounds.size;
            return new Rect((Vector2)transform.position - size / 2, size);
    } }

    void Start()
    {
        physicsBody = GetComponent<Rigidbody2D>();
        physicsBody.gravityScale = FALL_GRAVITY;
        currentState = State.Aerial;
        input = new PlayerInput();

        if(LevelData.Instance != null && LevelData.Instance.RespawnPoint.HasValue) {
            transform.position = LevelData.Instance.RespawnPoint.Value;
            CameraScript.Instance.SetInitialPosition();
        }
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
                if(Mathf.Abs(velocity.y) <= 0.05f) {
                    // check to make sure this isn't the player hitting a ceiling
                    Rect collisionArea = CollisionArea;
                    RaycastHit2D leftRaycast = Physics2D.Raycast(new Vector3(collisionArea.xMin, collisionArea.yMin - 0.1f, 0), Vector2.down);
                    RaycastHit2D rightRaycast = Physics2D.Raycast(new Vector3(collisionArea.xMax, collisionArea.yMin - 0.1f, 0), Vector2.down);

                    // land on the ground
                    if(leftRaycast.collider != null && leftRaycast.distance < 0.2f || rightRaycast.collider != null && rightRaycast.distance < 0.2f) {
                        currentState = State.Grounded;
                        break;
                    }
                }

                friction = 5f;

                // extend jump height while jump is held
                if(physicsBody.velocity.y < 0 || !input.IsPressed(PlayerInput.Action.Jump)) {
                    jumpHeld = false;
                }

                // determine gravity
                if(jumpHeld) {
                    physicsBody.gravityScale = JUMP_GRAVITY;
                    if(physicsBody.velocity.y > JUMP_VELOCITY) {
                        physicsBody.gravityScale = (JUMP_GRAVITY + FALL_GRAVITY) / 2;
                    }
                } else {
                    physicsBody.gravityScale = FALL_GRAVITY;
                }

                if(physicsBody.gravityScale != FALL_GRAVITY && 
                    (physicsBody.velocity.y < 0 || !input.IsPressed(PlayerInput.Action.Jump))
                ) {
                    physicsBody.gravityScale = FALL_GRAVITY;
                }
                
                Direction adjWallDir = GetAdjacentWallDireciton();

                // cling to walls
                if(velocity.y < CLING_VELOCITY && 
                    (adjWallDir == Direction.Left && input.IsPressed(PlayerInput.Action.Left) || adjWallDir == Direction.Right && input.IsPressed(PlayerInput.Action.Right))
                ) {
                    velocity.y = CLING_VELOCITY;
                }

                // wall jump
                if(adjWallDir != Direction.None && input.JustPressed(PlayerInput.Action.Jump)) {
                    int jumpDirection = (adjWallDir == Direction.Left ? 1 : -1);
                    velocity.y = 11.0f;
                    velocity.x = jumpDirection * 6.0f;
                    moveLockedRight = (jumpDirection == -1);
                    jumpHeld = true;
                }

                // allow jump during coyote time
                if(coyoteTime > 0) {
                    if(input.JustPressed(PlayerInput.Action.Jump)) {
                        Jump(ref velocity);
                        coyoteTime = 0;
                    } else {
                        coyoteTime -= Time.deltaTime;
                    }
                }
                break;

            case State.Grounded:
                friction = 30f;
                if(input.JumpBuffered) { // jump buffer allows a jump when pressed slightly before landing
                    Jump(ref velocity);
                }
                else if(velocity.y < -0.01f) {
                    // fall off platform
                    currentState = State.Aerial;
                    coyoteTime = 0.05f;
                }
                break;
        }

        // horizontal movement
        float walkAccel = WALK_ACCEL * Time.deltaTime;
        bool moveRight = input.IsPressed(PlayerInput.Action.Right) && velocity.x < WALK_SPEED && moveLockedRight != true;
        bool moveLeft = input.IsPressed(PlayerInput.Action.Left) && velocity.x > -WALK_SPEED && moveLockedRight != false;
        if(moveRight == moveLeft) { // both pressed is same as neither pressed
            // apply friction
            if(velocity != Vector2.zero) {
                float reduction = friction * Time.deltaTime;
                if(Mathf.Abs(velocity.x) <= reduction) {
                    // prevent passing 0
                    velocity.x = 0;
                } else {
                    velocity.x += (velocity.x > 0 ? -1 : 1) * friction * Time.deltaTime;
                }
            }
        }
        else if(moveRight) {
            velocity.x += walkAccel;
            if(velocity.x > WALK_SPEED) {
                velocity.x = WALK_SPEED;
            }

            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if(moveLeft) {
            velocity.x -= walkAccel;
            if(velocity.x < -WALK_SPEED) {
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
                Vector2 attackDirection = Vector2.zero;
                if(input.IsPressed(PlayerInput.Action.Up)) {
                    attackDirection = Vector2.up;
                }
                if(input.IsPressed(PlayerInput.Action.Down)) {
                    attackDirection = Vector2.down;
                }
                if(attackDirection == Vector2.zero) {
                    attackDirection = (transform.localScale.x > 0 ? Vector2.right : Vector2.left);
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
                keyCooldown = 0.5f;
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
        currentState = State.Aerial;
        jumpHeld = true;
    }

    // checks if the player's left and right sides are against any surfaces. Returns Direction.None for no wall, and left or right if there is a wall
    private Direction GetAdjacentWallDireciton() {
        Rect collisionArea = CollisionArea;

        const float BUFFER = 0.05f;

        RaycastHit2D leftTop = Physics2D.Raycast(new Vector3(collisionArea.xMin - BUFFER, collisionArea.yMax, 0), Vector2.left);
        RaycastHit2D leftMid = Physics2D.Raycast(new Vector3(collisionArea.xMin - BUFFER, collisionArea.center.y, 0), Vector2.left);
        RaycastHit2D leftBot = Physics2D.Raycast(new Vector3(collisionArea.xMin - BUFFER, collisionArea.yMin, 0), Vector2.left);

        RaycastHit2D rightTop = Physics2D.Raycast(new Vector3(collisionArea.xMax + BUFFER, collisionArea.yMax, 0), Vector2.right);
        RaycastHit2D rightMid = Physics2D.Raycast(new Vector3(collisionArea.xMax + BUFFER, collisionArea.center.y, 0), Vector2.right);
        RaycastHit2D rightBot = Physics2D.Raycast(new Vector3(collisionArea.xMax + BUFFER, collisionArea.yMin, 0), Vector2.right);

        if(leftTop.collider != null && leftTop.distance < 2 * BUFFER || leftMid.collider != null && leftMid.distance < 2 * BUFFER || leftBot.collider != null && leftBot.distance < 2 * BUFFER) {
            return Direction.Left;
        }
        if(rightTop.collider != null && rightTop.distance < 2 * BUFFER || rightMid.collider != null && rightMid.distance < 2 * BUFFER || rightBot.collider != null && rightBot.distance < 2 * BUFFER) {
            return Direction.Right;
        }

        return Direction.None;
    }
}
