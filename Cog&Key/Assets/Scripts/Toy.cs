using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toy : Agent
{
    // Start is called before the first frame update
    protected override void Start()
    {
        state = KeyState.Normal;
        base.Start();
        direction = new Vector3(1, 0, 0);
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
                break;
            case KeyState.Reverse:
                break;
            case KeyState.Lock:
                // Stop in place
                // Lock until removed
                break;
            case KeyState.Fast:
                // Same movement, scale the speed by a fast value, do not edge detect
                break;
            default:
                break;
        }
    }
    
    public void AttachKey(KeyState key)
    {

    }
}
