using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorScript : MonoBehaviour
{
    [SerializeField] private bool RequireAll = true;

    private bool open;
    private List<DoorOpener> locks = new List<DoorOpener>();
    private float startY;
    private float height;

    private const float MOVE_SPEED = 10f;

    public bool Locked { get; set; }

    void Start() {
        startY = transform.position.y;
        height = transform.localScale.y;
        open = false;
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

    public void AddLock(DoorOpener doorLock) {
        locks.Add(doorLock);
    }

    public void CheckLocks() {
        open = RequireAll;
        foreach(DoorOpener opener in locks) {
            if(!RequireAll && opener.Activated) {
                open = true;
                return;
            }

            if(RequireAll && !opener.Activated) {
                open = false;
                return;
            }
        }
    }
}
