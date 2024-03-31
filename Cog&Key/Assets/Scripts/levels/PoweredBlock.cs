using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoweredBlock : Rideable
{
    private const float SPEED = 5f;
    private float startHeight;
    private Vector2 forwardDirection;
    private Rigidbody2D physBod;
    private float halfWidth;

    void Start() {
        startHeight = transform.position.y;
        physBod = GetComponent<Rigidbody2D>();
        physBod.gravityScale = 4.0f;
        //physBod.mass = 999999f;
        halfWidth = transform.localScale.x / 2f;

        float angle = transform.GetChild(0).transform.rotation.eulerAngles.z;
        switch(angle) {
            case 0:
                forwardDirection = Vector2.right;
                break;

            case 90:
                forwardDirection = Vector2.up;
                break;

            case 180:
                forwardDirection = Vector2.left;
                break;

            case 270:
            case -90:
                forwardDirection = Vector2.down;
                break;

            default:
                Debug.Log("Invalid angle for powered block");
                break;
        }
    }

    void FixedUpdate() {
        if(physBod.isKinematic)
        {
            physBod.velocity = Vector3.zero;
        }

        Vector2 moveDirection = Vector2.zero;
        if(InsertedKeyType == KeyState.Fast) {
            moveDirection = forwardDirection;
        }
        else if(InsertedKeyType == KeyState.Reverse) {
            moveDirection = -forwardDirection;
        }
        if(InsertedKeyType != KeyState.Lock && moveDirection != Vector2.down && transform.position.y < startHeight) {
            moveDirection = Vector2.up;
        }

        if(moveDirection != Vector2.zero && Global.IsObjectBlocked(gameObject, moveDirection)) {
            moveDirection = Vector2.zero;
        }

        transform.position += SPEED * Time.deltaTime * (Vector3)moveDirection;

        CheckSideRiders();
    }

    protected override void OnRiderAdded(GameObject rider) {
        if(rider.layer == LayerMask.NameToLayer("Ground")) {
            return;
        }

        rider.transform.SetParent(transform.GetChild(1), true);
    }

    protected override void OnRiderRemoved(GameObject rider, int index) {
        rider.transform.SetParent(null);

    }

    protected override void OnKeyInserted(KeyState newKey) {
        if(newKey == KeyState.Lock) {
            physBod.isKinematic = false;
            physBod.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
        }
    }

    protected override void OnKeyRemoved(KeyState removedKey) {
        if(removedKey == KeyState.Lock) {
            physBod.isKinematic = true;
            physBod.velocity = Vector3.zero;
            physBod.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }
}
