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

    [SerializeField] private float coyoteTime = 0.4f;
    [SerializeField] private float fallGravity = 0.25f;
    private bool isFalling = false;
    
    private enum Ground
    {
        Bottom, Left, Top, Right
    }

    [SerializeField] private Ground groundState;

    private bool rotating = false;
    private float rotateTimer = 0;

    protected override void Start()
    {
        base.Start();
        rb.MoveRotation(180);
    }

    protected override void Update()
    {
        //switch(transform.rotation.z)
        //{
        //    case 0:
        //        groundState = Ground.Bottom;
        //        break;
        //    case 360:
        //        groundState = Ground.Bottom;
        //        break;

        //    case 90:
        //        groundState = Ground.Right;
        //        break;
        //    case -270:
        //        groundState = Ground.Right;
        //        break;

        //    case 180:
        //        groundState = Ground.Top;
        //        break;
        //    case -180:
        //        groundState = Ground.Top;
        //        break;

        //    case 270:
        //        groundState = Ground.Left;
        //        break;
        //    case -90:
        //        groundState = Ground.Left;
        //        break;
        //}
        if(Mathf.Abs(transform.rotation.z) > 360)
        {
            transform.eulerAngles = new Vector3(0, 0, transform.rotation.z % 360);
        }

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


        if (jumpState == JumpState.Aerial)
        {
            //transform.rotation = Quaternion.Lerp(transform.rotation, groundState = ? Quaternion.LookRotation(Vector3.zero) : Quaternion.LookRotation(Vector2.right * direction.x), rotateTimer);
            rotateTimer += Time.deltaTime;
        }
        else
        {
            rotateTimer = 0;
            if (Mathf.Abs(transform.rotation.z) > 45)
                transform.eulerAngles = Vector2.right * Mathf.Sign(transform.rotation.z);
            else
                transform.eulerAngles = Vector3.zero;

        }

        if(!isFalling)
        {
            if (groundState != Ground.Bottom && jumpState == JumpState.Grounded)
            {
                if (groundState == Ground.Top) rb.gravityScale = -GROUND_GRAVITY;
                else rb.gravityScale = 0;
            }
            else rb.gravityScale = GROUND_GRAVITY;
        }
        

        if (floorPts.Count > 0 && groundState == Ground.Bottom && floorPts[0].point.y > transform.position.y)
        {
            Fall();
        }


        if(Input.GetKeyDown(KeyCode.V))
        {
            groundState = Ground.Bottom;
            Fall();
        }

        base.Update();
    }

    protected override void BehaviorTree(float walkSpeed, bool fast)
    {
        charging = false;

        bool playerSensed = false;
        // Check only sense[0] if in normal orientation
        // Check both [0] and [1] if on a wall or ceiling
        if (senses[0].collidedPlayer)
        {
            playerSensed = true;
            playerPosition = player.transform.position;
        }
        if (transform.rotation.z != 0)
        {
            if (senses[1].collidedPlayer)
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
                if (Mathf.Sign(dir.x) != Mathf.Sign(direction.x) && ledgeSize > minLedgeSize && (groundState == Ground.Bottom || groundState == Ground.Top)) StartCoroutine(TurnDelay());
            }
        }

        if (transform.rotation.z != 0)
        {
            if (playerSensed)
            {
                if ((transform.position.x - playerPosition.x) < distThreshold)
                {
                    Jump();
                }
            }
        }

        else if (sqrDist > distThreshold * distThreshold)
        {
            // try to chase the player
            float tempX = (playerPosition - transform.position).x;
            if (Mathf.Sign(tempX) != Mathf.Sign(direction.x) && !processingTurn)
            {
                if (groundState == Ground.Bottom || groundState == Ground.Top) StartCoroutine(TurnDelay());
            }
            wallDetected = EdgeDetect(false, true) != 0;
            // If there's a wall in front and the player is above it, try to jump
            // Player needs to be able to jump over enemy
            // instead of jumping to meet, turn around
            if (wallDetected && playerSensed)
            {
                //Jump();
            }
            if (playerPosition.y > transform.position.y + halfHeight * 5)
            {
                if (playerSensed || wallDetected) Debug.Log("Jump"); //Jump();
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


        if (!processingTurn && !processingStop)
        {
            if(groundState == Ground.Bottom || groundState == Ground.Top)
            {
                rb.velocity = new Vector2(walkSpeed * direction.x, rb.velocity.y);
            }
            else
            {
                rb.velocity = new Vector2(rb.velocity.x, walkSpeed * direction.y);
            }
        }
    }

    private void Fall()
    {
        if(!isFalling) StartCoroutine(CoyoteFall());
    }

    IEnumerator CoyoteFall()
    {
        isFalling = true;
        rb.gravityScale = fallGravity;
        yield return new WaitForSeconds(coyoteTime);
        rb.gravityScale = GROUND_GRAVITY;
        //yield return new WaitUntil(() => floorPts.Count == 0);
        isFalling = false;
        yield return null;
    }
}
