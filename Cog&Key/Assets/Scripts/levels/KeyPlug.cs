using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyPlug : KeyWindable
{
    [SerializeField] private DoorScript target;

    private DoorOpener doorLock;

    private SpriteRenderer light;

    void Start() {
        doorLock = new DoorOpener(target);
        light = transform.GetChild(0).GetComponent<SpriteRenderer>();
        light.color = Color.red;
    }

    protected override void OnKeyInserted(KeyState newKey) {
        if(newKey == KeyState.Fast || newKey == KeyState.Reverse) {
            doorLock.Activated = true;
            light.color = Color.green;
        }
        else if(newKey == KeyState.Lock) {
            target.Locked = true;
        }
    }

    protected override void OnKeyRemoved(KeyState removedKey) {
        if(removedKey == KeyState.Fast || removedKey == KeyState.Reverse) {
            doorLock.Activated = false;
            light.color = Color.red;
        }
        else if(removedKey == KeyState.Lock) {
            target.Locked = false;
        }
    }
}
