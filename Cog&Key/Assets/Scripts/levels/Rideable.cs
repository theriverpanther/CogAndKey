using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// generic class for an obstacle that affects entities attached to it
[RequireComponent(typeof(BoxCollider2D))]
public class Rideable : MonoBehaviour
{
    protected List<GameObject> riders = new List<GameObject>();

    // should be called in subclass Update(), removes riders on the side if they are not pressed towards this object
    protected void CheckSideRiders() {
        for(int i = riders.Count - 1; i >= 0; i--) {
            GameObject rider = riders[i];
            PlayerScript player = rider.GetComponent<PlayerScript>();
            if(player == null) {
                continue;
            }

            Rect riderArea = Global.GetCollisionArea(rider);
            Rect platformArea = Global.GetCollisionArea(gameObject);

            bool onSide = riderArea.yMin < platformArea.yMax && riderArea.yMax > riderArea.yMin;
            bool towardsMiddle = (rider.transform.position.x < transform.position.x && player.Input.IsPressed(PlayerInput.Action.Right)) 
                || (rider.transform.position.x > transform.position.x && player.Input.IsPressed(PlayerInput.Action.Left));
            if(onSide && !towardsMiddle) {
                riders.RemoveAt(i);
                OnRiderRemoved(rider);
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
    }

    // pseudo events for sub classes
    protected virtual void OnRiderAdded(GameObject rider) { }
    protected virtual void OnRiderRemoved(GameObject rider) { }
}
