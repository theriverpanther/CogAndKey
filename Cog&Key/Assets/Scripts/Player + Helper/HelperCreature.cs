using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class HelperCreature : MonoBehaviour
{
    // Start is called before the first frame update

    private Vector2 directionToplayer;
    private float moveSpeed;
    private GameObject player;
    private Rigidbody2D rb;
    private float dis = 0f;
    //public AnimationCurve myCurve;
    float distanceAwayAllowed = 2f;
    private Vector3 goPoint;
    string dir = "left";

    [SerializeField]
    public bool followPlayer;
    [SerializeField]
    private GameObject helperVisual;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player");
        moveSpeed = 2f;
        //Physics2D.IgnoreCollision(player.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        followPlayer = true;

        if (LevelData.Instance != null && LevelData.Instance.RespawnPoint.HasValue)
        {
            transform.position = LevelData.Instance.RespawnPoint.Value;
            transform.position = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z);
        }
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
        if (followPlayer)
        {
            goPoint = player.transform.position;

        } 
        directionToplayer = (goPoint - transform.position).normalized;
        dis = Vector2.Distance(goPoint, transform.position);

        // following player speed info
        if (dis > distanceAwayAllowed && followPlayer)
        {
            rb.velocity = new Vector2(directionToplayer.x, directionToplayer.y) * moveSpeed;
        } else if (!followPlayer && dis > 0.2f)
        {
            rb.velocity = new Vector2(directionToplayer.x, directionToplayer.y) * (moveSpeed * 3f);
        }
        else {
            rb.velocity = Vector3.zero;
        }

        FlipSprite();
        ChangeSpeedBasedOnDistance(dis);
    }

    /// <summary>
    /// Changes speed based on how far the creature is from the player
    /// </summary>
    /// <param name="distance"></param>
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

    /// <summary>
    /// Force respawn point
    /// </summary>
    /// <param name="position"></param>
    public void SetSpawnpoint(Vector3 position)
    {
        transform.position = position;
    }

    private void FlipSprite()
    {
        //Debug.Log(transform.right);debug
        if(directionToplayer.x > 0 && dir == "right")
        {
            helperVisual.transform.localScale = new Vector3(-Mathf.Abs(helperVisual.transform.localScale.x), helperVisual.transform.localScale.y, helperVisual.transform.localScale.z);
            dir = "left";
        } else if (dir == "left" && directionToplayer.x < 0)
        {
            helperVisual.transform.localScale = new Vector3(Mathf.Abs(helperVisual.transform.localScale.x), helperVisual.transform.localScale.y, helperVisual.transform.localScale.z);
            dir = "right";
        }
    }

    public void SetGoPoint(Vector3 position)
    {
        goPoint = position;
    }

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if(!followPlayer)
    //    {
    //        Physics2D.IgnoreCollision(collision.collider, GetComponent<Collider2D>() , true);
    //    }

    //    if (collision.gameObject.tag == "Wall" || collision.gameObject.tag == "Agent")
    //    {
    //        Physics2D.IgnoreCollision(collision.collider, GetComponent<Collider2D>(), true);
    //    }
    //}

    //private void OnCollisionStay2D(Collision2D collision)
    //{
    //    if (collision.gameObject.tag == "Wall")
    //    {
    //        Physics2D.IgnoreCollision(collision.collider, GetComponent<Collider2D>(), true);
    //    }
    //}

    //private void OnCollisionExit2D(Collision2D collision)
    //{
    //    if (!followPlayer)
    //    {
    //        Physics2D.IgnoreCollision(collision.collider, GetComponent<Collider2D>(), false);
    //    }
    //}



    //void FloatInPlace()
    //{
    //    if (!stopped)
    //    {
    //        stopped = true;
    //        stopX = transform.position.x;
    //        stopY = transform.position.y;
    //    } else
    //    {
    //        float yTo = myCurve.Evaluate((Time.time % myCurve.length));
    //        // Mathf.Lerp(transform.position.y, transform.position.y + yTo
    //        transform.position = new Vector3(stopX, Mathf.Lerp(transform.position.y, stopY + yTo, Time.time), transform.position.z);
    //    }

    //}

    //IEnumerator floatPlace()
    //{

    //    if (!stopped)
    //    {
    //        stopped = true;
    //        midFrame = true;
    //        stopX = transform.position.x;
    //        stopY = transform.position.y;
    //    }

    //    startedCorountine = true;
    //    rb.velocity = Vector3.zero;

    //    int loopTime = 0;


    //    float yTo = myCurve.Evaluate((Time.time % myCurve.length));
    //    // Mathf.Lerp(transform.position.y, transform.position.y + yTo
    //    while(currentFrame != myCurve.length)
    //    {
    //        transform.position = new Vector3(stopX, Mathf.Lerp(transform.position.y, stopY + yTo, Time.time), transform.position.z);
    //        currentFrame++;
    //    }

    //    Debug.Log("hit here");
    //    midFrame = false;
    //    startedCorountine = false;
    //    currentFrame = 0;
    //    yield return null;
    //}

}
