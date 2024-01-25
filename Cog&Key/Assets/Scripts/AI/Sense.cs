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
        if(collision.gameObject.tag.Equals("Player"))
        {
            if(type == SenseType.Sight) 
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

    private void OnTriggerExit2D(Collider2D collision)
    {
        collidedPlayer = false;
        if(collision.tag == "Player")
        {
            player = null;
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

    private void Update()
    {
        if(player != null)
        {
            if (type == SenseType.Sight)
            {
                RaycastHit2D results;
                results = Physics2D.Raycast(transform.position, (player.transform.position - transform.position).normalized, 10f);
                //Debug.DrawLine(transform.position, results.point, Color.red, 2f);
                //DebugDisplay.Instance.PlaceDot("Raycast", results.point);
                if (results.collider != null && results.collider.gameObject.tag == "Player")
                {
                    //Debug.Log($"Pos:{transform.position}, Collider: {player.transform.position}");

                    //Debug.DrawLine(transform.position, (player.transform.position - transform.position).normalized, Color.red, 2f);
                    PlayerSensed(player);
                }
            }
        }
    }
}
