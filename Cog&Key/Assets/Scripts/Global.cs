using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Global
{
    public static Rect GetCollisionArea(GameObject objectWithBoxCollider) {
        Bounds bound = objectWithBoxCollider.GetComponent<BoxCollider2D>().bounds;
        return new Rect(bound.center - bound.size / 2, bound.size);
    }
}
