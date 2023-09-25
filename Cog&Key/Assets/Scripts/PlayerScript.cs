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

    public bool HasFastKey;
    public bool HasLockKey;
    public bool HasReverseKey;

    private const float FALL_GRAVITY = 5.0f;
    private const float JUMP_GRAVITY = 2.4f;
    private const float JUMP_VELOCITY = 13.0f;
    private const float CLING_VELOCITY = -1.5f; // the maximum downward speed when pressed against a wall

    private const float WALK_SPEED = 7.0f; // per second
    private const float WALK_ACCEL = 100.0f; // per second^2

    private Rigidbody2D physicsBody;
    private KeyAttack keyAttack;
    private State currentState;
    private PlayerInput input;
    private float minX;
    private float maxX;
    private IKeyWindable keyTarget;
    private KeyState activeKey;

    private bool jumpHeld;
    private float coyoteTime;
    private float keyCooldown;
    private bool? moveLockedRight = null; // prevents the player from moving in this direction. false is left, null is neither
    private List<GameObject> currentWalls; // the wall the player is currently up against, can be multiple at once

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
        currentWalls = new List<GameObject>();
        keyAttack = transform.GetChild(0).GetComponent<KeyAttack>();

        if(LevelData.Instance.RespawnPoint.HasValue) {
            transform.position = LevelData.Instance.RespawnPoint.Value;
        }


        minX = LevelData.Instance.LevelAreas[0].xMin;
        maxX = LevelData.Instance.LevelAreas[0].xMax;
        foreach(Rect area in LevelData.Instance.LevelAreas) { 
            minX = Mathf.Min(minX, area.xMin);
            maxX = Mathf.Max(maxX, area.xMax);
        }
    }

    void Update()
    {
        input.Update();
        Vector2 velocity = physicsBody.velocity;
        float friction = 0f; // per second^2

        if(physicsBody.velocity.y <= 1.5f) {
            moveLockedRight = null;
        }

        // check if player left the boundaries of the level
        if(transform.position.x < minX) {
            transform.position = new Vector3(minX, transform.position.y, 0);
        }
        else if(transform.position.x > maxX) {
            transform.position = new Vector3(maxX, transform.position.y, 0);
        }

        bool withinBounds = false;
        Rect collision = CollisionArea;
        foreach(Rect area in LevelData.Instance.LevelAreas) { 
            if(area.Overlaps(collision)) {
                withinBounds = true;
                break;
            }
        }

        if(!withinBounds) {
            Die();
            return;
        }

        // if against a wall, check if still next to it
        for(int i = 0; i < currentWalls.Count; i++) {
            if(!IsAgainstWall(currentWalls[i])) {
                currentWalls.RemoveAt(i);
                i--;
            }
        }

        // vertical movement
        switch(currentState)
        {
            case State.Aerial:
                friction = 5f;

                // extend jump height while jump is held
                if(physicsBody.velocity.y < 0 || !input.IsPressed(PlayerInput.Action.Jump)) {
                    jumpHeld = false;
                }

                // determine gravity
                if(jumpHeld && physicsBody.velocity.y <= JUMP_VELOCITY) {
                    physicsBody.gravityScale = JUMP_GRAVITY;
                } else {
                    physicsBody.gravityScale = FALL_GRAVITY;
                }

                if(physicsBody.gravityScale != FALL_GRAVITY && 
                    (physicsBody.velocity.y < 0 || !input.IsPressed(PlayerInput.Action.Jump))
                ) {
                    physicsBody.gravityScale = FALL_GRAVITY;
                }

                // cling to walls
                if(velocity.y < CLING_VELOCITY) {
                    foreach(GameObject wall in currentWalls) {
                        if(velocity.x > 0 && wall.transform.position.x > transform.position.x
                            || velocity.x < 0 && wall.transform.position.x < transform.position.x
                        ) {
                            velocity.y = CLING_VELOCITY;
                            break;
                        }
                    }
                }

                // wall jump
                if(currentWalls.Count > 0 && input.JustPressed(PlayerInput.Action.Jump)) {
                    int jumpDirection = (transform.position.x > currentWalls[0].transform.position.x ? 1 : -1);
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
                else if(velocity.y < 0) {
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
            if(HasFastKey && input.JustPressed(PlayerInput.Action.FastKey)) {
                usedKey = KeyState.Fast;
            }
            else if(HasLockKey && input.JustPressed(PlayerInput.Action.LockKey)) {
                usedKey = KeyState.Lock;
            }
            else if(HasReverseKey && input.JustPressed(PlayerInput.Action.ReverseKey)) {
                usedKey = KeyState.Reverse;
            }

            if(usedKey != KeyState.Normal) {
                // send key attack
                if(usedKey == activeKey) {
                    // remove active key
                    keyTarget.InsertKey(KeyState.Normal);
                    activeKey = KeyState.Normal;
                    keyTarget = null;
                }

                // determine attack direction
                Vector2 attackDirection = Vector2.right;
                if(input.IsPressed(PlayerInput.Action.Up)) {
                    attackDirection = Vector2.up;
                }
                if(input.IsPressed(PlayerInput.Action.Down)) {
                    attackDirection = Vector2.down;
                }

                keyAttack.SendKey(usedKey, attackDirection);
                keyCooldown = 0.5f;
            }
        } else {
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Wall") {
            moveLockedRight = null;
            if(Mathf.Abs(physicsBody.velocity.y) <= 0.01f) {
                // land on ground, from aerial or wall state
                currentState = State.Grounded;
            }
            if(IsAgainstWall(collision.gameObject)) {
                // against a wall
                currentWalls.Add(collision.gameObject);
            }
        }
    }

    // trigger off of key collision
    private void OnTriggerEnter2D(Collider2D collision)
    {
        IKeyWindable keyWindable = collision.gameObject.GetComponent<IKeyWindable>();
        if(keyWindable != null) {
            if(keyTarget != null) {
                // remove last key
                keyTarget.InsertKey(KeyState.Normal);
            }

            keyTarget = keyWindable;
            activeKey = keyAttack.keyType;
            keyTarget.InsertKey(activeKey);
            keyAttack.gameObject.SetActive(false);
        }
    }

    // determines if the player is up against the input wall on the left or right side
    private bool IsAgainstWall(GameObject wall) {
        float halfHeight = GetComponent<BoxCollider2D>().bounds.extents.y;
        if(transform.position.y + halfHeight <= wall.transform.position.y - wall.transform.localScale.y / 2
            || transform.position.y - halfHeight >= wall.transform.position.y + wall.transform.localScale.y / 2
        ) {
            // above or below the wall
            return false;
        }

        return Math.Abs(wall.transform.position.x - transform.position.x) - (wall.transform.localScale.x + GetComponent<BoxCollider2D>().bounds.size.x) / 2 < 0.1f;
    }
}
