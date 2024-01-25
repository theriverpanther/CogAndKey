using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// generic class for an obstacle that affects entities attached to it
[RequireComponent(typeof(BoxCollider2D))]
public abstract class Rideable : MonoBehaviour
{
    protected List<GameObject> riders = new List<GameObject>();
    private List<GameObject> attachedNonRiders = new List<GameObject>(); // colliding objects which are not attached

    // should be called in subclass Update(), checks objects on the side to determine if they are riding
    protected void CheckSideRiders() {
        // check for side riders not pressed towards the middle
        for(int i = riders.Count - 1; i >= 0; i--) {
            GameObject rider = riders[i];
            if(OnSide(rider) && !PressedTowardsMiddle(rider)) {
                riders.RemoveAt(i);
                attachedNonRiders.Add(rider);
                OnRiderRemoved(rider);
            }
        }

        // check for objects on the side becoming pressed towards the middle again
        for(int i = attachedNonRiders.Count - 1; i >= 0; i--) {
            GameObject possibleRider = attachedNonRiders[i];
            if(OnSide(possibleRider) && PressedTowardsMiddle(possibleRider)) {
                attachedNonRiders.RemoveAt(i);
                riders.Add(possibleRider);
                OnRiderAdded(possibleRider);
            }
        }
    }

    // add riders when they collide
    private void OnCollisionEnter2D(Collision2D collision) {
        Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
        if(rb != null && !riders.Contains(collision.gameObject)) {
            riders.Add(collision.gameObject);
            OnRiderAdded(collision.gameObject);
        }
    }

    // remove riders when they are not connected
    private void OnCollisionExit2D(Collision2D collision) {
        if(riders.Contains(collision.gameObject)) {
            riders.Remove(collision.gameObject);
            OnRiderRemoved(collision.gameObject);
        }
        else if(attachedNonRiders.Contains(collision.gameObject)) {
            attachedNonRiders.Remove(collision.gameObject);
        }
    }

    protected bool OnSide(GameObject rider) {
        Rect riderArea = Global.GetCollisionArea(rider);
        Rect platformArea = Global.GetCollisionArea(gameObject);
        return riderArea.yMin < platformArea.yMax && riderArea.yMax > riderArea.yMin;
    }

    // returns true if the input game object is attempting to move towards the middle of this rideable
    private bool PressedTowardsMiddle(GameObject rider) {
        PlayerScript player = rider.GetComponent<PlayerScript>();
        if(player != null) {
            return (rider.transform.position.x < transform.position.x && player.Input.IsPressed(PlayerInput.Action.Right))
                || (rider.transform.position.x > transform.position.x && player.Input.IsPressed(PlayerInput.Action.Left));
        }

        return true;
    }

    // pseudo events for sub classes
    protected virtual void OnRiderAdded(GameObject rider) { }
    protected virtual void OnRiderRemoved(GameObject rider) { }
}
