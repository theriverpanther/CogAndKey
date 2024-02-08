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
        halfWidth = transform.localScale.x / 2f;

        float angle = transform.GetChild(0).transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
        forwardDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    void FixedUpdate() {
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

        if(moveDirection != Vector2.zero) {
            Vector2 perp = halfWidth * new Vector2(-moveDirection.y, moveDirection.x);
            Vector2 halfForward = (halfWidth + 0.01f) * moveDirection;
            Vector2 top = (Vector2)transform.position + perp + halfForward;
            Vector2 bot = (Vector2)transform.position - perp + halfForward;

            RaycastHit2D topRay = Physics2D.Raycast(top, moveDirection, 1f);
            RaycastHit2D botRay = Physics2D.Raycast(bot, moveDirection, 1f);

            if(topRay.collider != null && topRay.distance < 0.02f || botRay.collider != null && botRay.distance < 0.02f) {
                moveDirection = Vector2.zero;
            }
        }

        transform.position += SPEED * Time.deltaTime * (Vector3)moveDirection;

        CheckSideRiders();
    }

    protected override void OnRiderAdded(GameObject rider) {
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
