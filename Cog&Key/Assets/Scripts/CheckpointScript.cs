using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointScript : MonoBehaviour
{
    private Color uncheckedColor;

    private void Start()
    {
        uncheckedColor = GetComponent<SpriteRenderer>().color;
    }

    // used to create the visual difference between the current checkpoint and an unused checkpoint
    public void SetAsCheckpoint(bool isCheckpoint) {
        if(isCheckpoint) {
            GetComponent<SpriteRenderer>().color = Color.green;
        } else {
            GetComponent<SpriteRenderer>().color = uncheckedColor;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        PlayerScript player = collision.gameObject.GetComponent<PlayerScript>();
        if(player != null) {
            LevelData.Instance.TriggerCheckpoint(this);
        }
    }
}
