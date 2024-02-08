using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pill : Agent
{
    

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        switch (InsertedKeyType)
        {
            case KeyState.None:
                // Move forward until an edge is hit, turn around on the edge
                // Hits edge = either collision on side or edge of platform
                BehaviorTree(movementSpeed, false);
                //rb.velocity = new Vector2(movementSpeed * direction.x, rb.velocity.y);
                break;
            case KeyState.Reverse:
                // Change direction
                // Might try to cache old movement for full reversal
                // For now, just use the opposite of the direction
                BehaviorTree(movementSpeed, false);
                //rb.velocity = new Vector2(movementSpeed * direction.x, rb.velocity.y);
                break;
            case KeyState.Lock:
                // Stop in place
                // Lock until removed
                // Will have logic in future iterations
                rb.velocity = new Vector2(0, rb.velocity.y);
                break;
            case KeyState.Fast:
                // Same movement, scale the speed by a fast value, do not edge detect ground
                // Lose control of seeking, just zoom in direction
                BehaviorTree(movementSpeed * fastScalar, true);
                //rb.velocity = new Vector2(movementSpeed * direction.x * fastScalar, rb.velocity.y);
                break;
            default:
                break;
        }
        if (wallPts.Count > 0 && floorPts.Count <= 2) rb.gravityScale = 0;
        else rb.gravityScale = GROUND_GRAVITY;
        base.Update();
    }

    protected override void BehaviorTree(float walkSpeed, bool fast)
    {
        base.BehaviorTree(walkSpeed, fast);
        EdgeDetectMovement(!fast, true);
    }
}
