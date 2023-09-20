using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointScript : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision) {
        if(LevelData.Instance.CurrentCheckpoint == gameObject) {
            return;
        }

        PlayerScript player = collision.gameObject.GetComponent<PlayerScript>();
        if(player != null) {
            // save checkpoint
            GetComponent<SpriteRenderer>().color = Color.green;
        }
    }
}
