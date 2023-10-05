using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro.Examples;
using UnityEngine;

public class Hunter : Agent
{
    [SerializeField]
    private float distThreshold = 0.2f;

    // Start is called before the first frame update
    protected override void Start()
    {
        state = KeyState.Normal;
        base.Start();
        direction = new Vector2(-1, 0);
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

        //if(Input.GetKeyDown(KeyCode.P))
        //{
        //    InsertKey((KeyState)(((int)state + 1) % 4)); ;
        //    Debug.Log(state);
        //    //InsertKey((KeyState)Mathf.FloorToInt(Random.Range(1, 3)));
        //}
    }
    
    public void AttachKey(KeyState key)
    {

    }

    private void EdgeDetectMovement(bool detectFloorEdges, bool detectWalls)
    {
        int tempDir = EdgeDetect(collidingObjs, detectFloorEdges, detectWalls);
        direction.x = tempDir != 0 ? tempDir : direction.x;
    }

    protected override void BehaviorTree(float walkSpeed, bool fast)
    {
        float sqrDist = Mathf.Pow(PlayerPosition.x - direction.x, 2) + Mathf.Pow(PlayerPosition.y - direction.y, 2);

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
            EdgeDetectMovement(true, true);
        }
        else if (sqrDist > distThreshold * distThreshold)
        {
            // chase player
            direction.x = (PlayerPosition - transform.position).x;
            direction = direction.normalized;

            // Chase and player at height, move
            // Chase and blocked by wall, see if it can be jumped over, if it can then do it
            // If the player falls, follow them downward
        }
        else
        {
            // stop moving, attack player
        }

        base.BehaviorTree(walkSpeed, fast);
    }
}
