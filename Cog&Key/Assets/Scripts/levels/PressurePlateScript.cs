using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlateScript : MonoBehaviour
{
    [SerializeField] private DoorScript target;
    [SerializeField] private GameObject button;

    private DoorOpener doorLock;
    private bool pressed;
    private float buttonTop;
    private int numOnTop;

    private const float SPEED = 5.0f;

    void Start() {
        doorLock = new DoorOpener(target);
        buttonTop = button.transform.localPosition.y;
    }

    void Update() {
        if(pressed && button.transform.localPosition.y > 0) {
            button.transform.localPosition += new Vector3(0, -SPEED * Time.deltaTime, 0);
            if(button.transform.localPosition.y < 0) {
                button.transform.localPosition = new Vector3(button.transform.localPosition.x, 0, button.transform.localPosition.z);
            }
        }
        else if(!pressed && button.transform.localPosition.y < buttonTop) {
            button.transform.localPosition += new Vector3(0, SPEED * Time.deltaTime, 0);
            if(button.transform.localPosition.y > buttonTop) {
                button.transform.localPosition = new Vector3(button.transform.localPosition.x, buttonTop, button.transform.localPosition.z);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.attachedRigidbody.isKinematic || collision.gameObject.tag == "Not Physics") {
            return;
        }

        numOnTop++;
        pressed = true;
        doorLock.Activated = true;
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if(collision.attachedRigidbody.isKinematic || collision.gameObject.tag == "Not Physics") {
            return;
        }

        numOnTop--;
        if(numOnTop <= 0) {
            pressed = false;
            doorLock.Activated = false;
        }
    }
}
