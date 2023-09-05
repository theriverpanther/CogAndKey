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
    private float recoverTime;
    private bool keyInserted = true;
    // Start is called before the first frame update
    protected virtual void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if(!keyInserted)
        {
            // Remove this toy from any of the managers / other objects tracking it
            Destroy(this.gameObject);
        }
        base.Update();
    }
}
