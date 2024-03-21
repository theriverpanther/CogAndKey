using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum KeyState
{
    None, Reverse, Lock, Fast
}

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public abstract class KeyWindable : MonoBehaviour
{
    private List<SnapPoint> snapPoints = new List<SnapPoint>();
    private KeyScript insertedKey;
    public KeyState InsertedKeyType { get { return insertedKey == null ? KeyState.None : insertedKey.Type; } }
    public bool Insertible { get; set; } = true;

    public void AddSnapPoint(SnapPoint snap) {
        snapPoints.Add(snap);
    }

    // check if there is a snap point that this key can attach to
    public SnapPoint? FindSnapPoint(KeyScript key) {
        if(snapPoints.Count == 0) {
            // if there are no snap points, allow snapping anywhere
            return new SnapPoint { localPosition = key.transform.position - transform.position, keyDirection = key.velocity };
        }

        foreach(SnapPoint snap in snapPoints) {
            if(Vector3.Dot(key.velocity.normalized, snap.keyDirection) > 0.8f && Vector2.Distance(key.transform.position, transform.position + snap.localPosition) < 1.3f) {
                return snap;
            }
        }

        return null;
    }

    public void InsertKey(KeyScript key) {
        if(key == null) {
            return;
        }

        if(insertedKey != null) {
            insertedKey.Detach();
        }

        insertedKey = key;
        OnKeyInserted(InsertedKeyType);
    }

    public void RemoveKey() {
        if(insertedKey == null) {
            return;
        }

        OnKeyRemoved(insertedKey.Type);
        insertedKey = null;
    }

    protected virtual void OnKeyInserted(KeyState newKey) { }
    protected virtual void OnKeyRemoved(KeyState removedKey) { }
}
