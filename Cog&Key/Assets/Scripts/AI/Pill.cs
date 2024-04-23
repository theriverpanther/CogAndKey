using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using static Unity.Burst.Intrinsics.X86;

public class Pill : Agent
{
    private GameObject player;
    [SerializeField] private float distThreshold = 0.1f;
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
        player = GameObject.Find("Player");
        base.Start();
        direction = Vector2.right;
        stepSize = 0.05f;
    }

    protected override void Update()
    {
        //rb.SetRotation(1 + rb.rotation);
        if (Mathf.Abs(transform.rotation.z) > 360)
        {
            rb.rotation %= 360;
        }

        AllocateContacts();

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
            IsGrounded();
            if (orientationState != Orientation.Up && jumpState == JumpState.Grounded)
            {
                if (InsertedKeyType == KeyState.Lock) rb.gravityScale = GROUND_GRAVITY;
                else if (orientationState == Orientation.Down) rb.gravityScale = -GROUND_GRAVITY;
                else
                {
                    rb.gravityScale = 0;
                    if (ledgeSize > minLedgeSize) rb.velocity = new Vector2(0, rb.velocity.y);
                }
            }
            else
            {
                if (!rotating)
                {
                    rb.gravityScale = GROUND_GRAVITY;
                    if (orientationState == Orientation.Down && jumpState == JumpState.Aerial) Fall();
                }
            }
        }
        

        if (floorPts.Count > 0 && orientationState == Orientation.Down && floorPts[0].point.y > transform.position.y)
        {
            //Fall();
        }


        if(Input.GetKeyDown(KeyCode.V))
        {
            Fall();
        }

        if (testVal) RotateUp();

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
            if(jumpState == JumpState.Grounded) EdgeDetectMovement(!fast, true);
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
                if (Mathf.Abs(transform.position.x - playerPosition.x) < distThreshold && orientationState == Orientation.Down)
                {
                    Fall();
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
            //if(transform.position.y > playerPosition.y && playerPosition != Vector3.zero) Fall();
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

    protected override void StepUp()
    {
        switch(orientationState)
        {
            case Orientation.Up:
                rb.position = new Vector2(rb.position.x, rb.position.y + stepSize);
                break;
            case Orientation.Down:
                rb.position = new Vector2(rb.position.x, rb.position.y - stepSize);
                break;
            case Orientation.Left:
                rb.position = new Vector2(rb.position.x - stepSize, rb.position.y);
                break;
            case Orientation.Right:
                rb.position = new Vector2(rb.position.x + stepSize, rb.position.y);
                break;
        } 
    }

    protected override int EdgeDetect(bool detectFloorEdges, bool detectWalls)
    {
        if (rotating) return 0;
        int returnVal = 0;
        if (contacts != null)
        {
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

            if (detectFloorEdges)
            {
                if (ledgeSize <= minLedgeSize)
                {
                    //bool leftRayCheck = RayCheck(transform.position, 0.1f, -halfWidth, halfHeight, 10);
                    //bool rightRayCheck = RayCheck(transform.position, -0.1f, halfWidth, halfHeight, 10);
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
                                    if (pathTarget.transform.position.y > transform.position.y) RotateUp();
                                    returnVal = 0;
                                    lostTimer = 0;
                                    isLost = false;
                                }
                                else RotateDown();
                            }
                            else
                            {
                                if (sqrDistToTarget <= 64f)
                                {
                                    if (transform.position.y < pathTarget.transform.position.y) RotateUp();
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
                                    RotateDown();
                                }
                            }
                        }


                        else if (PlayerPosition != Vector3.zero)
                        {
                            if (Mathf.Abs(rb.velocity.x) > 0) RotateUp();
                            returnVal = 0;
                        }

                        RotateDown();

                    }
                    else RotateDown();
                }
                else
                {
                    if(orientationState == Orientation.Up || orientationState == Orientation.Down)
                    {
                        float maxY = float.MinValue;
                        float minY = float.MaxValue;
                        foreach (ContactPoint2D contact in floorPts)
                        {
                            if (contact.point.y < minY) minY = contact.point.y;
                            if (contact.point.y > maxY) maxY = contact.point.y;
                        }
                        if (Mathf.Abs(maxY - minY) > stepSize) StepUp();
                    }
                    else
                    {
                        float maxX = float.MinValue;
                        float minX = float.MaxValue;
                        foreach (ContactPoint2D contact in floorPts)
                        {
                            if (contact.point.x < minX) minX = contact.point.x;
                            if (contact.point.x > maxX) maxX = contact.point.x;
                        }
                        if (Mathf.Abs(maxX - minX) > stepSize) StepUp();
                    }
                    
                }
            }
            if (detectWalls)
            {
                if (wallPts.Count >= 2)
                {
                    if (orientationState == Orientation.Up || orientationState == Orientation.Down)
                    {
                        float maxY = float.MinValue;
                        float minY = float.MaxValue;
                        float avgX = 0;
                        foreach (ContactPoint2D contact in wallPts)
                        {
                            if (contact.point.y < minY) minY = contact.point.y;
                            if (contact.point.y > maxY) maxY = contact.point.y;
                            avgX += contact.point.x;
                        }
                        avgX /= wallPts.Count;
                        // If the wall is detected at the direction of travel
                        if(Mathf.Sign(avgX - transform.position.x) == Mathf.Sign(direction.x))
                        {
                            if (Mathf.Abs(maxY - minY) > stepSize) RotateUp();
                        }
                    }
                    else
                    {
                        float maxX = float.MinValue;
                        float minX = float.MaxValue;
                        float avgY = 0;
                        foreach (ContactPoint2D contact in wallPts)
                        {
                            if (contact.point.y < minX) minX = contact.point.x;
                            if (contact.point.y > maxX) maxX = contact.point.x;
                            avgY += contact.point.y;
                        }
                        avgY /= wallPts.Count;
                        // If the wall is detected at the direction of travel
                        if (Mathf.Sign(avgY - transform.position.y) == Mathf.Sign(direction.y))
                        {
                            if (Mathf.Abs(maxX - minX) > stepSize) RotateUp();
                        }

                    }
                }
            }
        }
        return returnVal;
    }

    protected override void AllocateContacts()
    {
        rb.GetContacts(contacts);
        //Debug.Log(contacts.Count);

        floorPts.Clear();
        wallPts.Clear();

        foreach (ContactPoint2D contact in contacts)
        {
            bool floorCheck = false;
            bool wallCheck = false;
            switch (orientationState)
            {
                case Orientation.Up:
                    floorCheck = transform.position.y - contact.point.y <= 0.1f && transform.position.y - contact.point.y > 0;
                    wallCheck = Mathf.Abs(transform.position.x - contact.point.x) <= halfWidth + 0.1f;
                    break;
                case Orientation.Right:
                    floorCheck = transform.position.x - contact.point.x <= halfHeight && transform.position.x - contact.point.x > 0;
                    wallCheck = Mathf.Abs(transform.position.y - contact.point.y) <= halfWidth + 0.1f;
                    break;
                case Orientation.Down:
                    floorCheck = contact.point.y - transform.position.y <= 0.1f && contact.point.y - transform.position.y > 0;
                    wallCheck = Mathf.Abs(transform.position.x - contact.point.x) <= halfWidth + 0.1f;
                    break;
                case Orientation.Left:
                    floorCheck = contact.point.x - transform.position.x <= halfHeight && contact.point.x - transform.position.x > 0;
                    wallCheck = Mathf.Abs(transform.position.y - contact.point.y) <= halfWidth + 0.1f;
                    break;

            }
            if (contact.collider.tag == "Agent" || contact.collider.tag == "Player") continue;
            if (floorCheck)
            {
                floorPts.Add(contact);
            }
            if (wallCheck)
            {
                wallPts.Add(contact);
            }
            else
            {
                Debug.Log(Mathf.Abs(transform.position.x - contact.point.x));
            }

        }


        // Custom Sort based on agent -> pill based on orientation
        if(orientationState == Orientation.Up || orientationState == Orientation.Down)
        {
            floorPts.Sort((i, j) => { return i.point.x < j.point.x ? -1 : 1; });
            wallPts.Sort((i, j) => { return i.point.y < j.point.y ? 1 : -1; });
        }
        else
        {
            floorPts.Sort((i, j) => { return i.point.y < j.point.y ? -1 : 1; });
            wallPts.Sort((i, j) => { return i.point.x < j.point.x ? 1 : -1; });
        }
        CalcLedgeSize();
    }
    private void CalcLedgeSize()
    {
        ledgeSize = 0f;
        if (floorPts.Count > 1)
        {
            float min = float.MaxValue;
            float max = float.MinValue;

            if (orientationState == Orientation.Up || orientationState == Orientation.Down)
            {
                foreach (ContactPoint2D contact in floorPts)
                {
                    if (contact.point.x < min) min = contact.point.x;
                    if (contact.point.x > max) max = contact.point.x;
                }
            }
            else
            {
                foreach (ContactPoint2D contact in floorPts)
                {
                    if (contact.point.y < min) min = contact.point.y;
                    if (contact.point.y > max) max = contact.point.y;
                }
            }
            ledgeSize = Mathf.Abs(max - min);
        }
    }

    private void Fall()
    {
        if(!isFalling) StartCoroutine(CoyoteFall());
    }

    private void RotateDown()
    {
        if (orientationState == Orientation.Up)
        {
            StartCoroutine(Rotate(direction.x * -1));
        }
        else if (orientationState == Orientation.Down)
        {
            StartCoroutine(Rotate(direction.x));
        }
        else if (orientationState == Orientation.Right)
        {
            StartCoroutine(Rotate(direction.y));
        }
        else
        {
            StartCoroutine(Rotate(direction.y * -1));
        }
    }

    protected void RotateUp()
    {
        if (orientationState == Orientation.Up)
        {
            StartCoroutine(Rotate(direction.x));
        }
        else if (orientationState == Orientation.Down)
        {
            StartCoroutine(Rotate(direction.x * -1));
        }
        else if (orientationState == Orientation.Right)
        {
            StartCoroutine(Rotate(direction.y * -1));
        }
        else
        {
            StartCoroutine(Rotate(direction.y));
        }
    }


    IEnumerator Rotate(float direction)
    {
        IsGrounded();
        //&& jumpState == JumpState.Grounded
        if (!isRotating && jumpState == JumpState.Grounded)
        {
            isRotating = true;
            rb.gravityScale = 0;

            // Update Orientation
            orientationState -= (int)direction;
            if (orientationState > Orientation.Left) orientationState = 0;
            if (orientationState < 0) orientationState = Orientation.Left;

            float value = rb.rotation + direction * 90;
            rb.freezeRotation = false;
            rb.velocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
            
            rb.MoveRotation(value);
            float modTheta = value % 360;
            bool xCondition = modTheta == -90 || modTheta == 270 || modTheta == 0;
            bool yCondition = modTheta == -270 || modTheta == 90 || modTheta == 0;
            Vector3 newPos = transform.position + new Vector3((xCondition ? -1 : 1) * (Mathf.Abs(halfWidth - halfHeight)),
                                                                (yCondition ? -1 : 1) * (Mathf.Abs(halfHeight - halfWidth)), 0f);
            if (orientationState == Orientation.Down) newPos.y += .25f;
            //Debug.DrawLine(transform.position, newPos, Color.white, 2f);
            transform.position = newPos;
            yield return new WaitForSeconds(0.2f);
            rb.freezeRotation = true;
            rb.bodyType = RigidbodyType2D.Dynamic;

            if (this.direction.x != 0)
            {
                this.direction.y = this.direction.x * direction;
                this.direction.x = 0;
            }
            else if(this.direction.y != 0)
            {
                this.direction.x = this.direction.y * -1 * direction;
                this.direction.y = 0;
            }

            switch(orientationState)
            {
                case Orientation.Left:
                    transform.localScale = new Vector3(-this.direction.y, 1, 1);
                    rb.velocity = Vector2.right * 0.75f;
                    break;
                case Orientation.Down:
                    transform.localScale = new Vector3(this.direction.x, 1, 1);
                    rb.velocity = Vector2.up * 5f;
                    break;
                case Orientation.Right:
                    transform.localScale = new Vector3(this.direction.y, 1, 1);
                    rb.velocity = Vector2.left * 0.75f;
                    break;
                case Orientation.Up:
                    transform.localScale = new Vector3(-this.direction.x, 1, 1);
                    rb.velocity = Vector2.down * 0.75f;
                    break;
            }
            rb.gravityScale = 0;
            yield return new WaitUntil(() => ledgeSize > minLedgeSize);
            rb.velocity = Vector2.zero;
            jumpState = JumpState.Grounded;

            //float timer = 0;
            //float rotateTime = 1f;
            //while (timer < rotateTime)
            //{
            //    rb.SetRotation(Mathf.Lerp(rb.rotation, rb.rotation + 90 * direction, timer));
            //    timer += Time.deltaTime;
            //    yield return null;
            //}
            isRotating = false;
            testVal = false;
        }
        yield return null;
    }

    IEnumerator CoyoteFall()
    {
        if(!isRotating)
        {
            isFalling = true;
            rb.gravityScale = fallGravity;
            yield return new WaitForSeconds(coyoteTime);
            orientationState = Orientation.Up;
            transform.localScale = new Vector3(-direction.x, 1, 1);
            rb.rotation = 0;
            rb.gravityScale = GROUND_GRAVITY;
            //yield return new WaitUntil(() => floorPts.Count == 0);
            isFalling = false;
        }
        yield return null;
    }

    protected override bool RayCheck(Vector3 position, float buffer, float halfWidth, float halfHeight, float distance)
    {
        Vector3 origin = RotatePoint(new Vector3(halfWidth, -halfHeight, 0), rb.rotation);
        Vector2 direction = Vector2.down;
        switch(orientationState)
        {
            case Orientation.Up:
                direction = Vector2.down;
                break;
            case Orientation.Right:
                direction = Vector2.left;
                break;
            case Orientation.Down:
                direction = Vector2.up;
                break;
            case Orientation.Left:
                direction = Vector2.right;
                break;
        }

        RaycastHit2D ray = Physics2D.Raycast(position + origin, direction, distance, LayerMask.GetMask("Ground"));
        if (ray.collider != null && distance != 10)
        {
            Debug.DrawRay(origin + position, direction, Color.green, 1f);
        }
        return ray.collider != null && ray.distance < halfHeight + distance;
    }

    protected override void IsGrounded()
    {
        const float BUFFER = 0.2f;

        jumpState = ((RayCheck(transform.position, BUFFER, -halfWidth, 0, 5) || RayCheck(transform.position, -BUFFER, halfWidth, 0, 5)) && ledgeSize >= minLedgeSize - 0.1f ? JumpState.Grounded : JumpState.Aerial);
        //if(floorPts!=null)
        //{
        //    jumpState = floorPts.Count >= 2 && ledgeSize >= minLedgeSize ? JumpState.Grounded : JumpState.Aerial;
        //}
    }
}
