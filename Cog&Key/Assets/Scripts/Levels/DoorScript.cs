using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorScript : MonoBehaviour
{
    private bool open;
    private float startY;
    private float height;

    private const float MOVE_SPEED = 10f;
    public bool Locked { get; set; }

    void Start() {
        startY = transform.position.y;
        height = transform.localScale.y;
    }

    void Update() {
        if(Locked) {
            return;
        }

        if(open && transform.position.y < startY + height) {
            float newY = transform.position.y + MOVE_SPEED * Time.deltaTime;
            if(newY > startY + height) {
                newY = startY + height;
            }
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
        else if(!open && transform.position.y > startY) {
            float newY = transform.position.y - MOVE_SPEED * Time.deltaTime;
            if(newY < startY) {
                newY = startY;
            }
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    public void SetOpen(bool open) {
        this.open = open;
    }
}
