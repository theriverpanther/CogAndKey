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

    [SerializeField]
    private GameObject lightHolder;
    [SerializeField] 
    private GameObject lightBase;

    public bool Locked { get; set; }

    void Start() {
        startY = transform.position.y;
        height = transform.localScale.y;
        open = false;
        lightHolder.transform.parent.GetComponent<RectTransform>().localScale = new Vector3(0.0104735792f, 0.00314207375f, 0.00942622125f);
    }

    void Update() {
        if(Locked) {
            return;
        }

        Vector2 direction = Vector2.zero;
        if(open && transform.position.y < startY + height) {
            //float newY = transform.position.y + MOVE_SPEED * Time.deltaTime;
            //if(newY > startY + height) {
            //    newY = startY + height;
            //}
            //transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            direction = Vector2.up;
        }
        else if(!open && transform.position.y > startY) {
            //float newY = transform.position.y - MOVE_SPEED * Time.deltaTime;
            //if(newY < startY) {
            //    newY = startY;
            //}
            //transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            direction = Vector2.down;
        }

        if(direction != Vector2.zero && !Global.IsObjectBlocked(gameObject, direction)) {
            float newY = Mathf.Clamp(transform.position.y + MOVE_SPEED * Time.deltaTime * direction.y, startY, startY + height);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    public void AddLock(DoorOpener doorLock) {
        locks.Add(doorLock);

        if(!RequireAll && lightHolder.transform.childCount != 1)
        {
            Instantiate(lightBase, lightHolder.transform);
            doorLock.light = lightHolder.transform.GetChild(0).gameObject;

        } else if (!RequireAll)
        {
            doorLock.light = lightHolder.transform.GetChild(0).gameObject;
        }

        if (RequireAll)
        {
            doorLock.light = Instantiate(lightBase, lightHolder.transform);
        }

        doorLock.light.GetComponent<DoorLight>().LinkDoor(doorLock);
    }

    public void CheckLocks() {
        open = RequireAll;
        foreach (DoorOpener opener in locks)
        {
            if (!RequireAll && opener.Activated)
            {
                open = true;
                opener.light.GetComponent<DoorLight>().UpdateDoor();
                return;
            }

            if (RequireAll && !opener.Activated)
            {
                open = false;
                opener.light.GetComponent<DoorLight>().UpdateDoor();
                return;
            } 
        }

    }
}
