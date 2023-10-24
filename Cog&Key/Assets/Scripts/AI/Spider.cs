using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Spider : Agent
{
    private enum SpiderState
    {
        Inactive,Fast,Slow
    };

    private SpiderState state;
    private float AOERad;
    private float attackDamageSecond;
    private float attackPredictSpeed;
    private float clockAnimSpeed;

    // Start is called before the first frame update
    protected override void Start()
    {
        state = SpiderState.Inactive;
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }
}
