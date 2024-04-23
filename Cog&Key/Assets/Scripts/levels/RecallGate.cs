using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecallGate : MonoBehaviour
{
    private void Start()
    {
        GetComponent<SpriteRenderer>().enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.GetComponent<PlayerScript>() != null)
        {
            GameObject[] keys = GameObject.FindGameObjectsWithTag("Key");
            foreach(GameObject key in keys)
            {
                key.GetComponent<KeyScript>().Detach();
            }
        }
    }
}
