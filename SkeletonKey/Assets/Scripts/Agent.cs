using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    protected enum State
    {
        Inactive,
        Active
    };

    protected State agentState;
    // Start is called before the first frame update
    void Start()
    {
        agentState = State.Inactive;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
