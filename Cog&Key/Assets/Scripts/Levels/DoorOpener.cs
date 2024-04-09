using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorOpener
{
    private bool activated;
    public bool Activated {
        get { return activated; }
        set { 
            activated = value; 
            lockedDoor.CheckLocks(); 
        }
    }

    private DoorScript lockedDoor;

    public DoorOpener(DoorScript lockedDoor) {
        this.lockedDoor = lockedDoor;
        lockedDoor.AddLock(this);
    }
}
