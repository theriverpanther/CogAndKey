using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DoorLight : MonoBehaviour
{
    // Start is called before the first frame update
    private DoorOpener lockAttachedTo;
    [SerializeField]
    Color inactive;

    [SerializeField]
    Color active;

    private Image lightImg;

    public void Awake()
    {
        lightImg = gameObject.GetComponent<Image>();
    }

    public void LinkDoor(DoorOpener door)
    {
        lockAttachedTo = door;
        UpdateDoor();
    }

    public void UpdateDoor(string status = "")
    {
        if(status != "")
        {
            if(status == "open")
            {
                lightImg.color = active;
                return;
            }
            lightImg.color = inactive;
            return;
        }

        if(lockAttachedTo.Activated)
        {
            lightImg.color = active;
            return;
        }
        lightImg.color = inactive;
    }
}
