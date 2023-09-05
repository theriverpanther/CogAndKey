using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toy : Agent
{
    public enum ToyState 
    {
        Inactive, Active, Recovering
    }

    [SerializeField]
    private List<Vector3> endpoints = new List<Vector3>();
    [SerializeField]
    private int endpointSeekIndex = 0;
    [SerializeField]
    private ToyState state;
    private float recoverTime = 5f;
    private float recoverTimer = 0f;
    private bool keyInserted = true;
    // Start is called before the first frame update
    protected override void Start()
    {
        state = ToyState.Inactive;
        base.Start();
        direction = (endpoints[endpointSeekIndex] - transform.position).normalized;
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (!keyInserted)
        {
            // Remove this toy from any of the managers / other objects tracking it
            Destroy(this.gameObject);
        }
        if (state == ToyState.Recovering)
        {
            if(recoverTimer <= recoverTime)
            {
                recoverTimer += Time.deltaTime;
            }
            else
            {
                recoverTimer = 0f;
                state = ToyState.Active;
            }
        }
        if (state == ToyState.Inactive)
        {
            if(Input.GetKeyDown(KeyCode.Q))
            {
                state = ToyState.Active;
            }
        }
        if(state == ToyState.Active)
        {
            base.Update();
            // Testing to ensure functionality
            if (Input.GetKeyDown(KeyCode.E))
            {
                keyInserted = false;
                transform.GetChild(0).SetParent(transform.parent);
            }
            if(health <= 0)
            {
                state = ToyState.Recovering;
            }
            if(Vector3.Distance(transform.position, endpoints[endpointSeekIndex]) <= .05)
            {
                endpointSeekIndex = endpointSeekIndex + 1 >= endpoints.Count ? 0 : endpointSeekIndex + 1;
                direction = (endpoints[endpointSeekIndex] - transform.position).normalized;
            }
                
            transform.position += direction * Time.deltaTime * movementSpeed;



            gameObject.GetComponent<SpriteRenderer>().flipX = direction.x > 0;
            Vector3 keyPos = transform.GetChild(0).localPosition;
            // Currently magic number, will change in future iterations
            keyPos.x = direction.x > 0 ? -0.636f : 0.636f;
            transform.GetChild(0).localPosition = keyPos;

        }
    }
}
