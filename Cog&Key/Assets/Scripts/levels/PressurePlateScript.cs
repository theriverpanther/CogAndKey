using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlateScript : MonoBehaviour
{
    [SerializeField] private List<DoorScript> targets;
    [SerializeField] private GameObject button;

    private DoorOpener[] doorLocks;
    private bool pressed;
    private float buttonTop;
    private float buttonBottom;
    private int numOnTop;

    private const float SPEED = 5.0f;

    [SerializeField] Color inactve;
    [SerializeField] Color active;
    [SerializeField] private GameObject lightOnButton;

    void Start() {
        doorLocks = new DoorOpener[targets.Count];
        for(int i = 0; i < targets.Count; i++) {
            doorLocks[i] = new DoorOpener(targets[i]);
        }
        buttonTop = button.transform.localPosition.y;
        buttonBottom = -0.3f;
        lightOnButton.GetComponent<SpriteRenderer>().color = inactve;
    }

    void Update() {
        if(pressed && button.transform.localPosition.y > buttonBottom) {
            button.transform.localPosition += new Vector3(0, -SPEED * Time.deltaTime, 0);
            if(button.transform.localPosition.y < buttonBottom) {
                button.transform.localPosition = new Vector3(button.transform.localPosition.x, buttonBottom, button.transform.localPosition.z);
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
        if((collision.attachedRigidbody.isKinematic && collision.gameObject.GetComponent<PoweredBlock>() == null) || collision.gameObject.tag == "Not Physics") {
            return;
        }

        numOnTop++;
        pressed = true;
        lightOnButton.GetComponent<SpriteRenderer>().color = active;

        foreach (DoorOpener doorLock in doorLocks) {
            doorLock.Activated = true;
            doorLock.light.GetComponent<DoorLight>().UpdateDoor("open");
        }

        
        PoweredBlock block = collision.gameObject.GetComponent<PoweredBlock>();
        if(block != null) {
            block.SetPlateDirection();
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if((collision.attachedRigidbody.isKinematic && collision.gameObject.GetComponent<PoweredBlock>() == null) || collision.gameObject.tag == "Not Physics") {
            return;
        }

        numOnTop--;
        if(numOnTop <= 0) {
            pressed = false;
            lightOnButton.GetComponent<SpriteRenderer>().color = inactve;
            foreach (DoorOpener doorLock in doorLocks) {
                doorLock.Activated = false;
                doorLock.light.GetComponent<DoorLight>().UpdateDoor();
            }
        }
    }
}
