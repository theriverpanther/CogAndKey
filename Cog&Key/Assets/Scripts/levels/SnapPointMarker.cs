using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SnapPoint
{
    public Vector3 localPosition;
    public Vector3 keyDirection;
}

public class SnapPointMarker : MonoBehaviour
{
    void Awake()
    {
        Transform parent = transform.parent;
        float rotation = transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
        Vector3 angleVec = new Vector3(Mathf.Cos(rotation), Mathf.Sin(rotation), 0);
        parent.GetComponent<KeyWindable>().AddSnapPoint(new SnapPoint { localPosition = transform.position - parent.position, keyDirection = angleVec });

        Destroy(gameObject);
    }
}
