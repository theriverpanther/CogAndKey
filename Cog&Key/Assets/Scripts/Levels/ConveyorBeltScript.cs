using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorBeltScript : Rideable, IKeyWindable
{
    [SerializeField] private bool clockwise;
    private const float SHIFT_SPEED = 5.0f;
    private KeyState insertedKey = KeyState.Normal;
    private float ShiftSpeed {  get { return SHIFT_SPEED * (insertedKey == KeyState.Fast ? 2f : 1f); } }

    void FixedUpdate() {
        CheckSideRiders();

        // shift riders
        foreach(GameObject rider in riders) {
            rider.transform.position += ShiftSpeed * Time.deltaTime * DetermineShiftDirection(rider);

            // cancel out gravity when on the side
            if(OnSide(rider)) {
                Rigidbody2D physBod = rider.GetComponent<Rigidbody2D>();
                physBod.AddForce(-Physics2D.gravity * physBod.gravityScale);
            }
        }
    }

    protected override void OnRiderAdded(GameObject rider) {
        if(OnSide(rider)) {
            rider.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }
    }

    protected override void OnRiderRemoved(GameObject rider) {
        // keep rider momentum if moving fast
        if(insertedKey == KeyState.Fast) {
            rider.GetComponent<Rigidbody2D>().velocity += ShiftSpeed * 0.7f * (Vector2)DetermineShiftDirection(rider);
        }
    }

    public void InsertKey(KeyState key) {
        if( (insertedKey == KeyState.Reverse) != (key == KeyState.Reverse) ) {
            // flip direction
            clockwise = !clockwise;
        }

        insertedKey = key;
    }

    private Vector3 DetermineShiftDirection(GameObject rider) {
        if(OnSide(rider)) {
            // left or right
            if(clockwise == (rider.transform.position.x < transform.position.x)) {
                return Vector3.up;
            } else {
                return Vector3.down;
            }
        } else {
            // top or bottom
            if(clockwise == (rider.transform.position.y > transform.position.y)) {
                return Vector3.right;
            } else {
                return Vector3.left;
            }
        }
    }
}
