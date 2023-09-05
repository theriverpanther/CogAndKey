using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toy : Agent
{
    public enum ToyState 
    {
        Inactive, Active, Recovering
    }

    private ToyState state;
    private float recoverTime = 1f;
    private bool keyInserted = true;
    // Start is called before the first frame update
    protected override void Start()
    {
        state = ToyState.Inactive;
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        if(!keyInserted)
        {
            // Remove this toy from any of the managers / other objects tracking it
            Destroy(this.gameObject);
        }
        base.Update();
        // Testing to ensure functionality
        if(Input.GetKeyDown(KeyCode.W))
        {
            keyInserted = false;
            transform.GetChild(0).SetParent(transform.parent);
        }
    }
}
