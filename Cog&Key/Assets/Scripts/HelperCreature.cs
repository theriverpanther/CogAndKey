using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class HelperCreature : MonoBehaviour
{
    // Start is called before the first frame update

    private Vector2 directionToplayer;
    private float moveSpeed;
    [SerializeField]
    public GameObject player;
    private Rigidbody2D rb;

    bool stop = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        moveSpeed = 2f;

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        MoveTowardsPlayer();
    }

    void MoveTowardsPlayer()
    {
        directionToplayer = (player.transform.position - transform.position).normalized;
        float dis = Vector2.Distance(player.transform.position, transform.position);
        Debug.Log("Distance: " + dis);

        if(!stop)
        {
            rb.velocity = new Vector2(directionToplayer.x, directionToplayer.y) * moveSpeed;
        }

        ChangeSpeedBasedOnDistance(dis);
    }

    void ChangeSpeedBasedOnDistance(float distance)
    {
        //slow down
        if (distance < 1.5f)
        {
            if (moveSpeed != 0)
            {
                moveSpeed -= 0.2f;
            }

            if (moveSpeed < 0f)
            {
                moveSpeed = 0f;
                stop = true;
            }
        }

        //speed up
        if (distance > 4)
        {
            stop = false;
            moveSpeed += 0.2f;
        } else
        {
            if(moveSpeed != 2f) {
                moveSpeed -= 0.2f;
            }

            if(moveSpeed < 2f)
            {
                moveSpeed = 2f;
            }

        }
    }

}
