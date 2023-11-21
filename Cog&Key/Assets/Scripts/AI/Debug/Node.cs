using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    [SerializeField] protected Color gizmoColor = Color.yellow;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.GetComponent<Agent>()!= null)
        {
            other.gameObject.GetComponent<Agent>().PassTriggerValue(gameObject, false);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<Agent>() != null)
        {
            other.gameObject.GetComponent<Agent>().PassTriggerValue(gameObject, true);
        }
    }

    protected void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, 0.25f);
    }
}
