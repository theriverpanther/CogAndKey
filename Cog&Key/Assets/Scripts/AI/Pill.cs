using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;
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

    private float coyoteTime = 0.4f;
    private float fallGravity = 0.25f;
    [SerializeField] private bool isFalling = false;
    [SerializeField] private bool isRotating = false;

    public bool testVal = false;
    
    private enum Orientation
    {
        Up, Right, Down, Left
    }

    [SerializeField] private Orientation orientationState;

    private bool rotating = false;
    private float rotateTimer = 0;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        //rb.SetRotation(1 + rb.rotation);
        if (Mathf.Abs(transform.rotation.z) > 360)
        {
            //transform.eulerAngles = new Vector3(0, 0, transform.rotation.z % 360);
        }

        switch (InsertedKeyType)
        {
            case KeyState.None:
                // Move forward until an edge is hit, turn around on the edge
                // Hits edge = either collision on side or edge of platform
                BehaviorTree(movementSpeed, false);
                break;
            case KeyState.Reverse:
                // Change direction
                // Might try to cache old movement for full reversal
                // For now, just use the opposite of the direction
                BehaviorTree(movementSpeed, false);
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
                break;
            default:
                break;
        }


        if (jumpState == JumpState.Aerial)
        {
            //transform.rotation = Quaternion.Lerp(transform.rotation, groundState = ? Quaternion.LookRotation(Vector3.zero) : Quaternion.LookRotation(Vector2.right * direction.x), rotateTimer);
            //rotateTimer += Time.deltaTime;
        }
        else
        {
            rotateTimer = 0;
            //if (Mathf.Abs(transform.rotation.z) > 45)
                //transform.eulerAngles = Vector2.right * Mathf.Sign(transform.rotation.z);
            //else
                //transform.eulerAngles = Vector3.zero;

        }

        if(!isFalling)
        {
            if (orientationState != Orientation.Up && jumpState == JumpState.Grounded)
            {
                if (orientationState == Orientation.Down) rb.gravityScale = -GROUND_GRAVITY;
                else rb.gravityScale = 0;
            }
            else rb.gravityScale = GROUND_GRAVITY;
        }
        

        if (floorPts.Count > 0 && orientationState != Orientation.Up && floorPts[0].point.y > transform.position.y)
        {
            Fall();
        }


        if(Input.GetKeyDown(KeyCode.V))
        {
            orientationState = Orientation.Up;
            Fall();
        }

        if (testVal) Jump();

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
                if (Mathf.Sign(dir.x) != Mathf.Sign(direction.x) && ledgeSize > minLedgeSize && (orientationState == Orientation.Up || orientationState == Orientation.Down)) StartCoroutine(TurnDelay());
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
                if (orientationState == Orientation.Up || orientationState != Orientation.Down) StartCoroutine(TurnDelay());
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
            if(orientationState == Orientation.Up || orientationState == Orientation.Down)
            {
                rb.velocity = new Vector2(walkSpeed * direction.x, rb.velocity.y);
            }
            else
            {
                rb.velocity = new Vector2(rb.velocity.x, walkSpeed * direction.y);
            }
        }
    }

    protected override int EdgeDetect(bool detectFloorEdges, bool detectWalls)
    {
        int returnVal = 0;
        if (contacts != null)
        {
            BoxCollider2D collider = GetComponent<BoxCollider2D>();
            collider.GetContacts(contacts);

            // Floor Edges -
            // Find the contact points at the base of the agent
            // If the distance between the poles of these points is less than a proportion of the size of the hunter, turn (need to determine proportion)

            // Wall Edges -
            // Find contact points that are at any x extremity
            // Clean the list, only check ones that are in the direction of traversal
            // If the y position is at max y
            // If the player is past the wall, check the jump height
            // If a jump is possible, try it
            // Turn if fail
            // If the y position is below max y, jump
            floorPts.Clear();
            wallPts.Clear();
            foreach (ContactPoint2D contact in contacts)
            {
                if (contact.collider.tag == "Agent") continue;
                if (contact.point.y - transform.position.y <= halfHeight)
                {
                    floorPts.Add(contact);
                }
                if (Mathf.Abs(contact.point.x - transform.position.x) <= halfWidth)
                {
                    wallPts.Add(contact);
                    Debug.Log(wallPts.Count);
                }
                //DebugDisplay.Instance.DrawDot(contact.point);
            }

            // Custom Sort based on agent -> pill based on orientation
            floorPts.Sort((i, j) => { return i.point.x < j.point.x ? -1 : 1; });
            wallPts.Sort((i, j) => { return i.point.y < j.point.y ? 1 : -1; });
            // Change ray checks based on aligned axis

            float sqrDist = 0f;
            ledgeSize = 0f;
            if (floorPts.Count > 0)
            {
                sqrDist = SquareDistance(floorPts[0].point, floorPts[floorPts.Count - 1].point);

                ledgeSize = sqrDist;
            }

            if (detectFloorEdges)
            {
                if (sqrDist <= minLedgeSize)
                {
                    bool leftRayCheck = RayCheck(transform.position, 0.1f, -halfWidth, halfHeight, 10);
                    bool rightRayCheck = RayCheck(transform.position, -0.1f, halfWidth, halfHeight, 10);
                    if (pathTarget != null)
                    {
                        float xDistToTarget = Mathf.Abs(transform.position.x - pathTarget.transform.position.x);
                        float sqrDistToTarget = Vector3.SqrMagnitude(transform.position - pathTarget.transform.position);
                        if (xDistToTarget < 20f)
                        {
                            RaycastHit2D results;
                            results = Physics2D.Raycast(transform.position, (pathTarget.transform.position - transform.position).normalized, 5f, LayerMask.GetMask("Ground", "Player"));
                            if (results.collider != null)
                            {
                                Vector3 point = results.collider.transform.position;
                                if (Mathf.Abs(pathTarget.transform.position.y - transform.position.y) < 2f)
                                {
                                    if (pathTarget.transform.position.y > transform.position.y) Jump();
                                    returnVal = 0;
                                    lostTimer = 0;
                                    isLost = false;
                                }
                                else Drop();
                            }
                            else
                            {
                                if (sqrDistToTarget <= 64f)
                                {
                                    if (transform.position.y < pathTarget.transform.position.y) Jump();
                                    returnVal = 0;
                                    lostTimer = 0;
                                    isLost = false;
                                }
                                else
                                {
                                    lostTimer += Time.deltaTime;
                                    if (lostTimer >= confusionTime)
                                    {
                                        isLost = true;
                                    }
                                    Drop();
                                }
                            }
                        }


                        else if (PlayerPosition != Vector3.zero)
                        {
                            if (Mathf.Abs(rb.velocity.x) > 0) Jump();
                            returnVal = 0;
                        }

                        Drop();

                    }
                    else Drop();
                }
                else
                {
                    float maxY = float.MinValue;
                    float minY = float.MaxValue;
                    foreach (ContactPoint2D contact in floorPts)
                    {
                        if (contact.point.y < minY) minY = contact.point.y;
                        if (contact.point.y > maxY) maxY = contact.point.y;
                    }
                    if (maxY - minY > stepSize) StepUp();
                }
            }
            if (detectWalls)
            {
                if (wallPts.Count >= 2)
                {
                    float maxY = float.MinValue;
                    float minY = float.MaxValue;

                    foreach (ContactPoint2D contact in wallPts)
                    {
                        if (contact.point.y < minY) minY = contact.point.y;
                        if (contact.point.y > maxY) maxY = contact.point.y;
                    }

                    Jump();
                }
            }
        }
        return returnVal;
    }

    private void Fall()
    {
        if(!isFalling) StartCoroutine(CoyoteFall());
    }

    private void Drop()
    {        
        //StartCoroutine(Rotate(-1));
    }

    protected override void Jump()
    {
        StartCoroutine(Rotate(1));
    }


    IEnumerator Rotate(int direction)
    {
        if(!isRotating)
        {
            rb.freezeRotation = false;
            isRotating = true;

            orientationState -= direction;
            if (orientationState > Orientation.Left) orientationState = 0;
            if (orientationState < 0) orientationState = Orientation.Left;


            float timer = 0;
            float rotateTime = 1f;
            while (timer < rotateTime)
            {
                rb.SetRotation(Mathf.Lerp(rb.rotation, rb.rotation + 90 * direction, timer));
                timer += Time.deltaTime;
                yield return null;
            }
            isRotating = false;
            rb.freezeRotation = true;
            testVal = false;
        }
        yield return null;
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
