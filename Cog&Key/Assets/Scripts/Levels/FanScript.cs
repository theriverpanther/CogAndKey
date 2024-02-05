using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanScript : KeyWindable
{
    private const float MAX_BASE_FORCE = 10.0f;
    private static Dictionary<KeyState, float> keyToMultiplier = new Dictionary<KeyState, float>() {
        { KeyState.Fast, 2f },
        { KeyState.None, 1f },
        { KeyState.Lock, 0f },
        { KeyState.Reverse, -1f }
    };

    private List<Rigidbody2D> affectedEntities = new List<Rigidbody2D>();
    private Vector2 direction;
    private float fanRange;

    void Start() {
        float angle = Mathf.Deg2Rad * (transform.rotation.eulerAngles.z + 90f);
        direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        fanRange = GetComponents<BoxCollider2D>()[1].size.y;
    }

    void Update() {
        foreach(Rigidbody2D physBod in affectedEntities) {
            float distanceFromFan = Vector3.Project(physBod.transform.position - transform.position, direction).magnitude;
            float force = MAX_BASE_FORCE * (fanRange - distanceFromFan) / fanRange;
            physBod.AddForce(keyToMultiplier[InsertedKeyType] * force * direction);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
        if(rb != null && !affectedEntities.Contains(rb)) {
            affectedEntities.Add(rb);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
        if(affectedEntities.Contains(rb)) {
            affectedEntities.Remove(rb);
        }
    }
}
