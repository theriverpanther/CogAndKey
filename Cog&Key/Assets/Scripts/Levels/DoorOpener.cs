using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorOpener : MonoBehaviour, IKeyWindable
{
    [SerializeField] private DoorScript target;

    private KeyState insertedKey = KeyState.Normal;

    public void InsertKey(KeyState key) {
        if(key == KeyState.Fast) {
            target.SetOpen(true);
        }
        else if(insertedKey == KeyState.Fast && key != KeyState.Fast) {
            target.SetOpen(false);
        }

        insertedKey = key;
    }
}
