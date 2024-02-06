using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pill : Agent
{
    

    protected override void Start()
    {
        base.Start();
        width = GetComponent<BoxCollider2D>().size.x;
    }

    protected override void BehaviorTree(float walkSpeed, bool fast)
    {
        base.BehaviorTree(walkSpeed, fast);
        EdgeDetectMovement(true, true);
    }
}
