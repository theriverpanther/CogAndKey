using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.Timeline;

public class Hunter : Agent
{
    [SerializeField] private float distThreshold = 0.2f;
    [SerializeField] private bool wallDetected;

    private float maxHuntTime = 10f;
    private float huntTimer = 0f;

    // Start is called before the first frame update
    protected override void Start()
    {
        state = KeyState.Normal;
        base.Start();
        direction = new Vector2(-1, 0);
        wallDetected = false;
    }

    // Update is called once per frame
    protected override void Update()
    {
        switch(state)
        {
            case KeyState.Normal:
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

        transform.localScale = new Vector3(direction.x > 0 ? -scaleVal.x : scaleVal.x, scaleVal.y, scaleVal.z);
        base.Update();
        //if(Input.GetKeyDown(KeyCode.P))
        //{
        //    InsertKey((KeyState)(((int)state + 1) % 4)); ;
        //    Debug.Log(state);
        //    //InsertKey((KeyState)Mathf.FloorToInt(Random.Range(1, 3)));
        //}
    }
    
    //public void AttachKey(KeyState key)
    //{

    //}

    private void EdgeDetectMovement(bool detectFloorEdges, bool detectWalls)
    {
        int tempDir = EdgeDetect(detectFloorEdges, detectWalls);
        direction.x = tempDir != 0 ? tempDir : direction.x;
    }

    protected override void BehaviorTree(float walkSpeed, bool fast)
    {
        float sqrDist = Mathf.Pow(playerPosition.x - direction.x, 2) + Mathf.Pow(playerPosition.y - direction.y, 2);

        bool playerSensed = false;
        foreach(Sense s in senses)
        {
            if (s.collidedPlayer)
                playerSensed = true;
        }

        if(sqrDist <= distThreshold * distThreshold && !playerSensed)
        {
            PlayerPosition = Vector3.zero;
        }

        if (PlayerPosition.Equals(Vector3.zero))
        {
            // patrol
            // for now just deal with edge detection
            EdgeDetectMovement(!fast, true);
        }
        else if (sqrDist > distThreshold * distThreshold)
        {
            // try to chase the player
            float tempX = (playerPosition - transform.position).x;
            if(Mathf.Sign(tempX) != Mathf.Sign(direction.x)) 
            {
                if (turnTimer <= 0)
                {
                    direction.x = tempX;
                    direction = direction.normalized;
                    turnTimer = turnDelay;
                }
                else
                {
                    turnTimer -= Time.deltaTime;
                }
            }
            // If there's a wall in front and the player is above it, try to jump
            if(wallDetected && playerSensed && playerPosition.y > transform.position.y)
            {
                if(jumpState == JumpState.Grounded) Jump();
            }
            if(!playerSensed)
            {
                huntTimer += Time.deltaTime;
                if(huntTimer >= maxHuntTime)
                {
                    huntTimer = 0f;
                    playerPosition = Vector2.zero;
                }
            }
            else
            {
                huntTimer = 0f;
            }

            // TODO
            // NEED TO ACCOUNT FOR LOWER BOUNDS AKA FALLING OFF INTO DEATH
        }
        else
        {
            // stop moving, attack player
            base.BehaviorTree(0, fast);
            return;
        }

        base.BehaviorTree(walkSpeed, fast);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        wallDetected = other.tag == "Wall";
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Wall")
        {
            wallDetected = false;
        }
    }
}