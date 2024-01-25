using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorOpener : KeyWindable
{
    [SerializeField] private DoorScript target;

    protected override void OnKeyInserted(KeyState newKey) {
        if(newKey == KeyState.Fast) {
            target.SetOpen(true);
        }
        else if(newKey == KeyState.Lock) {
            target.Locked = true;
        }
    }

    protected override void OnKeyRemoved(KeyState removedKey) {
        if(removedKey == KeyState.Fast) {
            target.SetOpen(false);
        }
        else if(removedKey == KeyState.Lock) {
            target.Locked = false;
        }
    }
}
