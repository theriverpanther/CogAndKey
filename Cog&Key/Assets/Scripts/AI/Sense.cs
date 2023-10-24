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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag.Equals("Player"))
        {
            if(type == SenseType.Sight) 
            {
                if (Physics2D.Raycast(transform.parent.position, collision.transform.position))
                {
                    PlayerSensed(collision);
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
    }

    private void PlayerSensed(Collider2D collision)
    {
        collidedPlayer = true;
        transform.parent.GetComponent<Agent>().PlayerPosition = collision.transform.position;
    }
}
