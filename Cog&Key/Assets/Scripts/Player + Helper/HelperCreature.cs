using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class HelperCreature : MonoBehaviour
{
    // Start is called before the first frame update

    private Vector2 directionToplayer;
    private float moveSpeed;
    private GameObject player;
    private Rigidbody2D rb;
    private float dis = 0f;
    private float speed = 2f;

    float distanceAwayAllowed = 2f;
    private Vector3 goPoint;
    string dir = "left";
    float progress = 0f;

    [SerializeField]
    public bool followPlayer;
    public bool connectedToPlayer = false;
    [SerializeField]
    private GameObject helperVisual;

    [SerializeField]
    private CapsuleCollider2D capsuleContainer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player");
        capsuleContainer = player.transform.GetChild(2).GetComponent<CapsuleCollider2D>();
        moveSpeed = 2f;
        followPlayer = true;

        if (LevelData.Instance != null && LevelData.Instance.RespawnPoint.HasValue)
        {
            transform.position = LevelData.Instance.RespawnPoint.Value;
            transform.position = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z);
        }
    }

    private void FixedUpdate()
    {
        //MoveTowardsPlayer();
        FollowPlayer();
    }

    private void FollowPlayer()
    {
        if(followPlayer && !connectedToPlayer)
        {
            directionToplayer = capsuleContainer.transform.position - transform.position;

            rb.velocity = directionToplayer * speed;
        }
        FlipSprite();
    }

    //void MoveTowardsPlayer()
    //{
    //    if (followPlayer)
    //    {
    //        goPoint = player.transform.position;

    //    } 
    //    directionToplayer = (goPoint - transform.position).normalized;
    //    dis = Vector2.Distance(goPoint, transform.position);

    //    // following player speed info
    //    if (dis > distanceAwayAllowed && followPlayer)
    //    {
    //        rb.velocity = new Vector2(directionToplayer.x, directionToplayer.y) * moveSpeed;
    //    } else if (!followPlayer && dis > 0.2f)
    //    {
    //        rb.velocity = new Vector2(directionToplayer.x, directionToplayer.y) * (moveSpeed * 3f);
    //    }
    //    else {
    //        rb.velocity = Vector3.zero;
    //    }

    //    FlipSprite();
    //    ChangeSpeedBasedOnDistance(dis);
    //}

    /// <summary>
    /// Changes speed based on how far the creature is from the player
    /// </summary>
    /// <param name="distance"></param>
    //void ChangeSpeedBasedOnDistance(float distance)
    //{
    //    //slow down
    //    if (distance < 1.5f)
    //    {
    //        if (moveSpeed != 0)
    //        {
    //            moveSpeed -= 0.2f;
    //        }
    //    }

    //    //speed up
    //    if (distance > 4)
    //    {
    //        moveSpeed += 0.2f;
    //    } else
    //    {
    //        if(moveSpeed != 2f) {
    //            moveSpeed -= 0.2f;
    //        }

    //        if(moveSpeed < 2f)
    //        {
    //            moveSpeed = 2f;
    //        }

    //    }
    //}

    /// <summary>
    /// Force respawn point
    /// </summary>
    /// <param name="position"></param>
    //public void SetSpawnpoint(Vector3 position)
    //{
    //    transform.position = position;
    //}

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.name == "HelperConnection")
        {
            UnityEngine.Debug.Log("Inside connection point");
            rb.velocity = Vector2.zero;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {

        if (collision.transform.name == "HelperConnection" && !connectedToPlayer)
        {
            Vector3 offset = capsuleContainer.offset;
            if (Vector3.Distance(capsuleContainer.transform.position - transform.position, transform.position) < 0.1f)
            {
                UnityEngine.Debug.Log("Centered inside point");
                connectedToPlayer = true;
                rb.velocity = Vector2.zero;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.name == "HelperConnection")
        {
            UnityEngine.Debug.Log("Outside connection point");
            progress = 0f;
            connectedToPlayer = false;
        }
    }
}
