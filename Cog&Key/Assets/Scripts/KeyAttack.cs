using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// the player's key attack that tries to hit platforms and enemies
public class KeyAttack : MonoBehaviour
{
    private Vector2 direction;
    public KeyState keyType;

    private const float SPEED = 6f;
    private const float RANGE = 1.5f;

    void Update()
    {
        transform.localPosition += SPEED * Time.deltaTime * (Vector3)direction;
        if(transform.localPosition.sqrMagnitude >= RANGE * RANGE) {
            gameObject.SetActive(false);
        }
    }

    public void SendKey(KeyState keyType, Vector2 direction) {
        this.direction = direction.normalized;
        this.keyType = keyType;
        transform.localPosition = Vector3.zero;
        gameObject.SetActive(true);
    }
}
