using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// kills the player upon making contact, requies a trigger collider
[RequireComponent(typeof(Collider2D))]
public class KillOnTouch : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerScript player = collision.gameObject.GetComponent<PlayerScript>();
        if(player != null)
        {
            player.Die();
        }

        if(collision.gameObject.tag == "Agent")
        {
            Transform t = collision.transform.GetChild(collision.transform.childCount - 1);
            t.parent = null;
            t.GetComponent<KeyScript>().Detach();
            Destroy(collision.gameObject);
        }
    }
}
