using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sense : MonoBehaviour
{
    public bool collidedPlayer = false;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag.Equals("Player"))
        {
            collidedPlayer = true;
            transform.parent.GetComponent<Agent>().PlayerPosition = collision.transform.position;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        collidedPlayer = false;
    }
}
