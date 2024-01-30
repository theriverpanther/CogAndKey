using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointScript : MonoBehaviour
{
    private Color uncheckedColor;

    [SerializeField]
    public Texture2D unlitT, litT;

    [SerializeField]
    private ParticleSystem litParticles;

    private void Start()
    {
        uncheckedColor = GetComponent<SpriteRenderer>().color;
        litParticles.enableEmission = false;
    }

    // used to create the visual difference between the current checkpoint and an unused checkpoint
    public void SetAsCheckpoint(bool isCheckpoint) {
        //if(isCheckpoint) {
        //    GetComponent<SpriteRenderer>().color = Color.green;
        //} else {
        //    GetComponent<SpriteRenderer>().color = uncheckedColor;
        //}

        if (isCheckpoint)
        {
            litParticles.enableEmission = true;
            GetComponent<SpriteRenderer>().sprite = Sprite.Create(litT, new Rect(0.0f, 0.0f, litT.width, litT.height), new Vector2(0.5f, 0.5f), 100.0f);
        }
        else
        {
            litParticles.enableEmission = false;
            GetComponent<SpriteRenderer>().sprite = Sprite.Create(unlitT, new Rect(0.0f, 0.0f, unlitT.width, unlitT.height), new Vector2(0.5f, 0.5f), 100.0f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        PlayerScript player = collision.gameObject.GetComponent<PlayerScript>();
        if(player != null) {
            LevelData.Instance.TriggerCheckpoint(this);
        }
    }
}
