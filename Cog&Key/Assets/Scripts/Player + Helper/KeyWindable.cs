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
    private KeyScript insertedKey;
    public KeyState InsertedKeyType { get { return insertedKey == null ? KeyState.None : insertedKey.Type; } }

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
