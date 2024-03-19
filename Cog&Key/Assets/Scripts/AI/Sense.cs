using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SenseType
{
    Sight,
    Sound,
    Touch
}
public class Sense : MonoBehaviour
{
    public bool collidedPlayer = false;
    public SenseType type;
    [SerializeField] private GameObject player;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CollisionCheck(collision); 
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            player = null;
            collidedPlayer = false;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        CollisionCheck(collision);   
    }

    private void CollisionCheck(Collider2D collision)
    {
        if (collision.gameObject.tag.Equals("Player"))
        {
            if (type == SenseType.Sight)
            {
                RaycastHit2D results = Physics2D.Raycast(transform.position, (collision.transform.position - transform.position).normalized, 10f);
                //Debug.DrawLine(transform.position, results.transform.position, Color.red, 2f);
                //Debug.Log($"Pos:{transform.position}, Collider: {collision.transform.position}");

                if (results.collider != null && results.collider.gameObject.tag == "Player")
                {
                    Debug.DrawLine(transform.position, collision.transform.position, Color.red, 2f);
                    PlayerSensed(collision);
                }
                else
                {
                    player = collision.gameObject;
                }
            }
            else
            {
                PlayerSensed(collision);
            }


        }
    }

    private void PlayerSensed(Collider2D collision)
    {
        collidedPlayer = true;
        transform.parent.GetComponent<Agent>().PlayerPosition = collision.transform.position;
    }

    private void PlayerSensed(GameObject obj)
    {
        collidedPlayer = true;
        transform.parent.GetComponent<Agent>().PlayerPosition = player.transform.position;
    }
}
