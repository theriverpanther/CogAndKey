using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class Pill : Agent
{
    private GameObject player;
    private float distThreshold;
    private bool wallDetected = false;

    private bool charging = false;
    private float chargeSpeed = 5f;
    private float stunDuration = 2;
    private float stunTimer = 0;

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
        charging = false;

        bool playerSensed = false;
        // Check only sense[0] if in normal orientation
        // Check both [0] and [1] if on a wall or ceiling
        foreach (Sense s in senses)
        {
            if (s.collidedPlayer)
            {
                playerSensed = true;
                playerPosition = player.transform.position;
            }
        }

        float sqrDist = SquareDistance(playerPosition, direction);

        if (sqrDist <= distThreshold * distThreshold && !playerSensed)
        {
            PlayerPosition = Vector3.zero;
        }

        if (PlayerPosition.Equals(Vector3.zero))
        {
            // patrol
            EdgeDetectMovement(!fast, true);
            if (!isLost && pathTarget != null)
            {
                Vector2 dir = (pathTarget.transform.position - this.transform.position).normalized;
                if (Mathf.Sign(dir.x) != Mathf.Sign(direction.x) && ledgeSize > 2) StartCoroutine(TurnDelay());
            }
        }
        else if (sqrDist > distThreshold * distThreshold)
        {
            // try to chase the player
            float tempX = (playerPosition - transform.position).x;
            if (Mathf.Sign(tempX) != Mathf.Sign(direction.x) && !processingTurn)
            {
                StartCoroutine(TurnDelay());
            }
            wallDetected = EdgeDetect(false, true) != 0;
            // If there's a wall in front and the player is above it, try to jump
            // Player needs to be able to jump over enemy
            // instead of jumping to meet, turn around
            if (wallDetected && playerSensed)
            {
                Jump();
            }
            if (playerPosition.y > transform.position.y + halfHeight * 5)
            {
                if (playerSensed || wallDetected) Jump();
            }
            if (!playerSensed)
            {
                huntTimer += Time.deltaTime;
                if (huntTimer >= maxHuntTime)
                {
                    huntTimer = 0f;
                    playerPosition = Vector2.zero;
                }
            }
            else
            {
                huntTimer = 0f;
            }
        }
        else
        {
            // Assault the player
            charging = true;
            // If above the player, fall
            Fall();
            // If parallel to the player, charge them

            base.BehaviorTree(chargeSpeed, true);
            return;
        }

        base.BehaviorTree(walkSpeed, fast);
    }

    private void Fall()
    {

    }
}
