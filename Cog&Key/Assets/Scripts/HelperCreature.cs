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

    private CircleCollider2D playerTrigger;
    private Rigidbody2D rb;
    private bool stopped = false;
    public bool inRange = false;

    public AnimationCurve myCurve;
    private float stopX;
    private float stopY;
    float distanceAwayAllowed = 2f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        moveSpeed = 2f;
        stopX = 0f;
        stopY = 0f;
        Physics2D.IgnoreCollision(player.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        playerTrigger = player.GetComponent<CircleCollider2D>();
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

        //Debug.Log(dis);

        if(dis > distanceAwayAllowed && !inRange)
        {
            if (stopped)
            {
                stopped = false;
            }
            rb.velocity = new Vector2(directionToplayer.x, directionToplayer.y) * moveSpeed;
        } else
        {
            if(inRange)
            {
                FloatInPlace();
            }
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
        }

        //speed up
        if (distance > 4)
        {
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

    void FloatInPlace()
    {
        if (!stopped)
        {
            stopped = true;
            stopX = transform.position.x;
            stopY = transform.position.y;
        } else
        {
            float yTo = myCurve.Evaluate((Time.time % myCurve.length));
            // Mathf.Lerp(transform.position.y, transform.position.y + yTo
            transform.position = new Vector3(stopX, Mathf.Lerp(transform.position.y, stopY + yTo, Time.time), transform.position.z);
        }

    }
}
