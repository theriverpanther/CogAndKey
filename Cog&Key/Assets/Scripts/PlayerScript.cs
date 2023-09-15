using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{
    private enum State
    {
        Grounded,
        Aerial,
    }

    private const float FALL_GRAVITY = 5.0f;
    private const float JUMP_GRAVITY = 2.4f;
    private const float JUMP_VELOCITY = 13.0f;
    private const float CLING_VELOCITY = -1.5f; // the maximum downward speed when pressed against a wall

    private const float WALK_SPEED = 7.0f; // per second
    private const float WALK_ACCEL = 100.0f; // per second^2

    private Rigidbody2D physicsBody;
    private State currentState;
    private PlayerInput input;

    private float coyoteTime;
    private List<GameObject> currentWalls; // the wall the player is currently up against, can be multiple at once

    void Start()
    {
        physicsBody = GetComponent<Rigidbody2D>();
        physicsBody.gravityScale = FALL_GRAVITY;
        currentState = State.Aerial;
        input = new PlayerInput();
        currentWalls = new List<GameObject>();
    }

    void Update()
    {
        input.Update();
        Vector2 velocity = physicsBody.velocity;
        float friction = 0f; // per second^2

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
                if(physicsBody.gravityScale == JUMP_GRAVITY && 
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
        bool moveRight = input.IsPressed(PlayerInput.Action.Right) && velocity.x < WALK_SPEED;
        bool moveLeft = input.IsPressed(PlayerInput.Action.Left) && velocity.x > -WALK_SPEED;
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
        }
        else if(moveLeft) {
            velocity.x -= walkAccel;
            if(velocity.x < -WALK_SPEED) {
                velocity.x = -WALK_SPEED;
            }
        }

        physicsBody.velocity = velocity;
        //Debug.Log(currentState);
    }

    private void Jump(ref Vector2 newVelocity) {
        newVelocity.y = JUMP_VELOCITY;
        physicsBody.gravityScale = JUMP_GRAVITY;
        currentState = State.Aerial;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Wall") {
            if(Mathf.Abs(physicsBody.velocity.y) <= 0.01f) {
                // land on ground, from aerial or wall state
                currentState = State.Grounded;
            }
            if(Mathf.Abs(physicsBody.velocity.x) <= 0.01f) {
                // against a wall
                currentWalls.Add(collision.gameObject);
            }
        }
    }

    // determines if the player is up against the input wall on the left or right side
    private bool IsAgainstWall(GameObject wall) {
        if(transform.position.y + transform.localScale.y / 2 <= wall.transform.position.y - wall.transform.localScale.y / 2
            || transform.position.y - transform.localScale.y / 2 >= wall.transform.position.y + wall.transform.localScale.y / 2
        ) {
            // above or below the wall
            return false;
        }

        return Math.Abs(wall.transform.position.x - transform.position.x) - (wall.transform.localScale.x + transform.localScale.x) / 2 < 0.1f;
    }
}
