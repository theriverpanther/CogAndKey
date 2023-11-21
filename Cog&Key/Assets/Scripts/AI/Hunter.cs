using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.Timeline;

public class Hunter : Agent
{
    private float distThreshold = 0.2f;
    private bool wallDetected;
    [SerializeField] private Color idleColor;
    [SerializeField] private Color huntColor;
    private GameObject player;

    private float maxHuntTime = 2f;
    private float huntTimer = 0f;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        direction = new Vector2(-1, 0);
        wallDetected = false;
        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    protected override void Update()
    {
        switch(InsertedKeyType)
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

        base.Update();
        //if(Input.GetKeyDown(KeyCode.P))
        //{
        //    InsertKey((KeyState)(((int)state + 1) % 4)); ;
        //    Debug.Log(state);
        //    //InsertKey((KeyState)Mathf.FloorToInt(Random.Range(1, 3)));
        //}
    }

    private void EdgeDetectMovement(bool detectFloorEdges, bool detectWalls)
    {
        int tempDir = EdgeDetect(detectFloorEdges, detectWalls);
        if (tempDir != direction.x && tempDir != 0)
        {
            StartCoroutine(TurnDelay());
        }
    }

    protected override void BehaviorTree(float walkSpeed, bool fast)
    {
        bool playerSensed = false;
        foreach(Sense s in senses)
        {
            if (s.collidedPlayer)
            {
                playerSensed = true;
                playerPosition = player.transform.position;
            }  
        }
        float sqrDist = Mathf.Pow(playerPosition.x - direction.x, 2) + Mathf.Pow(playerPosition.y - direction.y, 2);

        if (sqrDist <= distThreshold * distThreshold && !playerSensed)
        {
            PlayerPosition = Vector3.zero;
        }

        if (PlayerPosition.Equals(Vector3.zero))
        {
            // patrol
            // for now just deal with edge detection
            //EdgeDetectMovement(!fast, true);
            Vector2 dir = (pathTarget.transform.position - this.transform.position).normalized;
            if (Mathf.Sign(dir.x) != Mathf.Sign(direction.x)) StartCoroutine(TurnDelay());
            gameObject.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().color = idleColor;
        }
        else if (sqrDist > distThreshold * distThreshold)
        {
            gameObject.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().color = huntColor;
            // try to chase the player
            float tempX = (playerPosition - transform.position).x;
            if(Mathf.Sign(tempX) != Mathf.Sign(direction.x) && !processingTurn) 
            {
                StartCoroutine(TurnDelay());
            }
            wallDetected = EdgeDetect(false, true) != 0;
            // If there's a wall in front and the player is above it, try to jump
            // Player needs to be able to jump over enemy
            // instead of jumping to meet, turn around
            if(wallDetected && playerSensed)
            {
                Jump();
            }
            if(playerPosition.y > transform.position.y)
            {
                if(playerSensed || wallDetected) Jump();
            }
            //List<Vector2> jumps = new List<Vector2>();
            //jumps = ValidJumps();
            
            //if (IsPointOnJump(playerPosition.x, playerPosition.y, mistakeThreshold))
            //{
            //    Jump();
            //}
            //else if(jumps.Count > 0)
            //{
            //    float closestDist = float.MaxValue;
            //    Vector2 node = Vector2.zero;
            //    float jumpSqrDist = 0f;
            //    foreach (Vector2 v in jumps)
            //    {
            //        jumpSqrDist = Mathf.Pow(v.x - transform.position.x, 2) + Mathf.Pow(v.y - transform.position.y, 2);
            //        if (jumpSqrDist < closestDist)
            //        {
            //            closestDist = jumpSqrDist;
            //            node = v;
            //        }
            //    }
            //    if (closestDist != float.MaxValue)
            //    {
            //        Vector3 directionToNode = ((Vector3)node - transform.position).normalized;
            //        if (directionToNode.x != direction.x) 
            //        {
            //            StartCoroutine(TurnDelay());
            //            Jump();
            //        }

            //    }
            //}
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
            gameObject.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().color = huntColor;
            // stop moving, attack player
            base.BehaviorTree(0, fast);
            return;
        }

        base.BehaviorTree(walkSpeed, fast);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(playerPosition, 1);
    }
}
