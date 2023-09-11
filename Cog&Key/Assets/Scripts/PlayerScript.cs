using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{
    private const float FALL_GRAVITY = 4.0f;
    private const float JUMP_GRAVITY = 2.0f;
    private const float JUMP_VELOCITY = 12.0f;

    private const float WALK_SPEED = 6.0f; // per second
    private const float WALK_ACCEL = 100.0f; // per second^2

    private Rigidbody2D physicsBody;

    private InputAction jumpAction;
    private InputAction rightAction;
    private InputAction leftAction;

    void Start()
    {
        physicsBody = GetComponent<Rigidbody2D>();
        physicsBody.gravityScale = FALL_GRAVITY;

        jumpAction = new InputAction((keyboard) => keyboard.upArrowKey, (gamepad) => gamepad.aButton);
        rightAction = new InputAction((keyboard) => keyboard.rightArrowKey, (gamepad) => gamepad.leftStick.right);
        leftAction = new InputAction((keyboard) => keyboard.leftArrowKey, (gamepad) => gamepad.leftStick.left);
    }

    void Update()
    {
        // jump and fall
        if(jumpAction.JustPressed()) {
            physicsBody.velocity = new Vector2(physicsBody.velocity.x, JUMP_VELOCITY);
            physicsBody.gravityScale = JUMP_GRAVITY;
        }

        if(physicsBody.velocity.y <= 0 || !jumpAction.IsPressed())
        {
            physicsBody.gravityScale = FALL_GRAVITY;
        }

        // horizontal movement
        float walkAccel = WALK_ACCEL * Time.deltaTime;
        if(rightAction.IsPressed()) {
            physicsBody.velocity += new Vector2(walkAccel, 0);
        }
        if(leftAction.IsPressed()) {
            physicsBody.velocity -= new Vector2(walkAccel, 0);
        }

        if(Mathf.Abs(physicsBody.velocity.x) > WALK_SPEED) {
            physicsBody.velocity = new Vector2(WALK_SPEED * (physicsBody.velocity.x > 0 ? 1 : -1), physicsBody.velocity.y);
        }
    }
}
