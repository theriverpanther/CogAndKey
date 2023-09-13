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
        AgainstWallMidair
    }

    private const float FALL_GRAVITY = 5.0f;
    private const float JUMP_GRAVITY = 2.4f;
    private const float JUMP_VELOCITY = 13.0f;

    private const float WALK_SPEED = 7.0f; // per second
    private const float WALK_ACCEL = 100.0f; // per second^2

    private Rigidbody2D physicsBody;
    private State currentState;

    private InputAction jumpAction;
    private InputAction rightAction;
    private InputAction leftAction;

    void Start()
    {
        physicsBody = GetComponent<Rigidbody2D>();
        physicsBody.gravityScale = FALL_GRAVITY;
        currentState = State.Aerial;

        jumpAction = new InputAction((keyboard) => keyboard.upArrowKey, (gamepad) => gamepad.aButton);
        rightAction = new InputAction((keyboard) => keyboard.rightArrowKey, (gamepad) => gamepad.leftStick.right);
        leftAction = new InputAction((keyboard) => keyboard.leftArrowKey, (gamepad) => gamepad.leftStick.left);
    }

    void Update()
    {
        Vector2 velocity = physicsBody.velocity;
        float friction = 0f; // per second^2

        // vertical movement
        switch(currentState)
        {
            case State.Aerial:
                friction = 5f;

                // extend jump height while jump is still held
                if(physicsBody.gravityScale == JUMP_GRAVITY && (physicsBody.velocity.y <= 0 || !jumpAction.IsPressed()) ) {
                    physicsBody.gravityScale = FALL_GRAVITY;
                }
                break;

            case State.Grounded:
                friction = 30f;

                if(jumpAction.JustPressed()) {
                    // jump
                    velocity.y = JUMP_VELOCITY;
                    physicsBody.gravityScale = JUMP_GRAVITY;
                    currentState = State.Aerial;
                }
                else if(velocity.y < 0) {
                    // fall off platform
                    currentState = State.Aerial;
                }
                break;

            case State.AgainstWallMidair:
                break;
        }

        // horizontal movement
        float walkAccel = WALK_ACCEL * Time.deltaTime;
        bool moveRight = rightAction.IsPressed() && velocity.x < WALK_SPEED;
        bool moveLeft = leftAction.IsPressed() && velocity.x > -WALK_SPEED;
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
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Wall") {
            if(physicsBody.velocity.y == 0) {
                currentState = State.Grounded;
            }
        }
    }
}
