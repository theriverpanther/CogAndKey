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
        physBod.mass = 999999f;
        halfWidth = transform.localScale.x / 2f;

        float angle = transform.GetChild(0).transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
        forwardDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    void FixedUpdate() {
        if(physBod.isKinematic)
        {
            physBod.velocity = Vector3.zero;
        }

        Vector2 moveDirection = Vector2.zero;
        if(InsertedKeyType != KeyState.Lock && transform.position.y < startHeight) {
            moveDirection = Vector2.up;
        }
        else if(InsertedKeyType == KeyState.Fast) {
            moveDirection = forwardDirection;
        }
        else if(InsertedKeyType == KeyState.Reverse) {
            moveDirection = -forwardDirection;
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
        }
    }

    protected override void OnKeyRemoved(KeyState removedKey) {
        if(removedKey == KeyState.Lock) {
            physBod.isKinematic = true;
            physBod.velocity = Vector3.zero;
        }
    }
}
