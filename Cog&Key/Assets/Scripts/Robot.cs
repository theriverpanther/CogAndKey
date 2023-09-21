using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Robot : Agent
{
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
        //if (!keyInserted)
        //{
        //    // Remove this toy from any of the managers / other objects tracking it
        //    Destroy(this.gameObject);
        //}
        switch(state)
        {
            case KeyState.Normal:
                // Move forward until an edge is hit, turn around on the edge
                // Hits edge = either collision on side or edge of platform
                EdgeDetectMovement();
                rb.velocity = new Vector2(movementSpeed * direction.x, rb.velocity.y);
                break;
            case KeyState.Reverse:
                // Change direction
                // Might try to cache old movement for full reversal
                // For now, just use the opposite of the direction
                EdgeDetectMovement();
                rb.velocity = new Vector2(movementSpeed * direction.x, rb.velocity.y);
                break;
            case KeyState.Lock:
                // Stop in place
                // Lock until removed
                // Will have logic in future iterations
                rb.velocity = new Vector2(0, rb.velocity.y);
                break;
            case KeyState.Fast:
                // Same movement, scale the speed by a fast value, do not edge detect
                rb.velocity = new Vector2(movementSpeed * direction.x * fastScalar, rb.velocity.y);
                break;
            default:
                break;
        }

        if(Input.GetKeyDown(KeyCode.P))
        {
            InsertKey((KeyState)(((int)state + 1) % 4)); ;
            Debug.Log(state);
            //InsertKey((KeyState)Mathf.FloorToInt(Random.Range(1, 3)));
        }
    }
    
    public void AttachKey(KeyState key)
    {

    }

    private void EdgeDetectMovement()
    {
        int tempDir = EdgeDetect(collidingObjs);
        direction.x = tempDir != 0 ? tempDir : direction.x;
    }
}
