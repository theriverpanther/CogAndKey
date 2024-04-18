using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.Timeline;

public class Hunter : Agent
{
    private float distThreshold = 0.01f;
    private bool wallDetected;
    private GameObject player;
    [SerializeField] Material signifier_mat_idle;
    [SerializeField] Material signifier_mat_attack;
    [SerializeField] GameObject signifier;

    string currentStateMat = "";

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        direction = new Vector2(-1, 0);
        wallDetected = false;
        player = GameObject.Find("Player");

        //huntSignifier = gameObject.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
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
                if (animationTag != null && !processingTurn && !processingStop)
                {
                    animationTag.GetComponent<Animator>().SetBool("Walking", true);
                }
                //rb.velocity = new Vector2(movementSpeed * direction.x, rb.velocity.y);
                break;
            case KeyState.Reverse:
                // Change direction
                // Might try to cache old movement for full reversal
                // For now, just use the opposite of the direction
                BehaviorTree(movementSpeed, false);
                if (animationTag != null && !processingTurn && !processingStop)
                {
                    animationTag.GetComponent<Animator>().SetBool("Walking", true);
                }
                //rb.velocity = new Vector2(movementSpeed * direction.x, rb.velocity.y);
                break;
            case KeyState.Lock:
                // Stop in place
                // Lock until removed
                // Will have logic in future iterations
                rb.velocity = new Vector2(0, rb.velocity.y);
                if (animationTag != null)
                {
                    animationTag.GetComponent<Animator>().SetBool("Walking", false);
                }
                break;
            case KeyState.Fast:
                // Same movement, scale the speed by a fast value, do not edge detect ground
                // Lose control of seeking, just zoom in direction
                BehaviorTree(movementSpeed * fastScalar, true);
                if (animationTag != null && !processingTurn && !processingStop)
                {
                    animationTag.GetComponent<Animator>().SetBool("Walking", true);
                }
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

    protected override void BehaviorTree(float walkSpeed, bool fast)
    {
        AllocateContacts();
        bool playerSensed = false;
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
            // for now just deal with edge detection
            if (jumpState == JumpState.Aerial) IsGrounded();
            if (jumpState == JumpState.Grounded) EdgeDetectMovement(!fast, true);

            if (!isLost && pathTarget != null)
            {
                
                Vector2 dir = (pathTarget.transform.position - this.transform.position).normalized;
                if (Mathf.Sign(dir.x) != Mathf.Sign(direction.x) && ledgeSize > 2) StartCoroutine(TurnDelay());
            }
            if (currentStateMat != "idle")
            {
                Agent.MatSwap(signifier, signifier_mat_idle);
            }
            currentStateMat = "idle";
        }
        else if (sqrDist > distThreshold * distThreshold)
        {
            //EdgeDetectMovement(!fast, true);
            if (currentStateMat != "attack")
            {
                Agent.MatSwap(signifier, signifier_mat_attack);
            }
            currentStateMat = "attack";

            // try to chase the player
            float tempX = (playerPosition - transform.position).x;
            wallDetected = EdgeDetect(false, true) != 0;
            if (Mathf.Sign(tempX) != Mathf.Sign(direction.x) && !processingTurn) 
            {
                StartCoroutine(TurnDelay());
            }
            else if (wallDetected)
            {
                Jump();
                //Debug.Log("Wall D");
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
            
            // If there's a wall in front and the player is above it, try to jump
            // Player needs to be able to jump over enemy
            // instead of jumping to meet, turn around
            
            if(playerPosition.y > transform.position.y + halfHeight * 2)
            {
                if(playerSensed && wallDetected) Jump();
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

            //EdgeDetectMovement(!fast, true);
        }
        else
        {
            // stop moving, attack player
            base.BehaviorTree(0, fast);
            return;
        }

        base.BehaviorTree(walkSpeed, fast);
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.red;
        if(playerPosition!=Vector3.zero) Gizmos.DrawWireSphere(playerPosition, 1);
    }
}
