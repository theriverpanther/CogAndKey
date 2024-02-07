using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoweredBlock : KeyWindable
{
    private const float SPEED = 5f;
    private float startHeight;
    private Vector2 forwardDirection;

    void Start() {
        startHeight = transform.position.y;

        float angle = transform.GetChild(0).transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
        forwardDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    void Update() {
        if(InsertedKeyType != KeyState.Lock && transform.position.y < startHeight) {
            return;
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

        transform.position += SPEED * Time.deltaTime * (Vector3)moveDirection;
    }
}
