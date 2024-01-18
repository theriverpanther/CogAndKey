using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEditor;
using UnityEngine.U2D;
using Unity.XR.OpenVR;

public class PathNode : Node
{
    [SerializeField] List<PathNode> nextNodes = new List<PathNode>();
    int nodeIndex = 0;

    [SerializeField] private bool stopAt = false;
    public bool StopAt { get { return stopAt; } }

    public PathNode ContinuePath()
    {
        PathNode node = nextNodes[nodeIndex];
        //nodeIndex++;
        //if (nodeIndex == nextNodes.Count) nodeIndex = 0;
        return node;
    }

    public void Update()
    {
        
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        Agent agent = collision.GetComponent<Agent>();
        if (agent != null) 
        {
            if(agent.pathTarget == this)
            {                
                // Send the value of the next node to the agent
                agent.pathTarget = nextNodes[nodeIndex];
                nodeIndex++;
                if (nodeIndex >= nextNodes.Count) nodeIndex = 0;
                if(stopAt)
                {
                    agent.Stop();
                }
            }
        }
    }

    protected void OnDrawGizmos()
    {
        //Gizmos.color = Color.white;
        //Gizmos.DrawLine(transform.position, transform.position + (nextNodes[0].transform.position - transform.position).normalized);
        Gizmos.color = Color.blue;

        float radius;
        Vector3 midpoint;
        Vector3[] bezierPoints = new Vector3[3];
        bezierPoints[0] = transform.position;
        foreach (PathNode node in nextNodes)
        {
            midpoint = (transform.position + node.transform.position) / 2;
            radius = (node.transform.position - transform.position).magnitude / 2;
            //Gizmos.DrawWireSphere(midpoint, radius);

            // PLEASE COMMENT OUT BEFORE BUILD
#if DEBUG
            //bezierPoints = Handles.MakeBezierPoints(transform.position, node.transform.position, transform.up, -transform.up, 5);

            
            Handles.color = Color.black;
            bezierPoints[1] = new Vector3(midpoint.x, midpoint.y + 0.5f * radius, midpoint.z);
            bezierPoints[2] = node.transform.position;
            Handles.DrawAAPolyLine(bezierPoints);
            //Handles.DrawWireArc(midpoint, transform.forward, transform.position, 180, radius);
            #endif
        }

        //Gizmos.DrawLine(transform.position, transform.position + transform.up);
        //Gizmos.DrawWireSphere()

        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, 0.125f);
    }
}
