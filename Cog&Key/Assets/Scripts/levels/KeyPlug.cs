using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyPlug : KeyWindable
{
    [SerializeField] private DoorScript target;

    private DoorOpener doorLock;

    void Start() {
        doorLock = new DoorOpener(target);
    }

    protected override void OnKeyInserted(KeyState newKey) {
        if(newKey == KeyState.Fast || newKey == KeyState.Reverse) {
            doorLock.Activated = true;
        }
        else if(newKey == KeyState.Lock) {
            target.Locked = true;
        }
    }

    protected override void OnKeyRemoved(KeyState removedKey) {
        if(removedKey == KeyState.Fast || removedKey == KeyState.Reverse) {
            doorLock.Activated = false;
        }
        else if(removedKey == KeyState.Lock) {
            target.Locked = false;
        }
    }
}
