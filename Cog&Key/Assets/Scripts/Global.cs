using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    None,
    Up,
    Down,
    Left,
    Right
}

public static class Global
{
    public static Rect GetCollisionArea(GameObject objectWithBoxCollider) {
        Bounds bound = objectWithBoxCollider.GetComponent<BoxCollider2D>().bounds;
        return new Rect(bound.center - bound.size / 2, bound.size);
    }

    public static Rect MakeExpanded(this Rect original, float amount) {
        return new Rect(original.xMin - amount, original.yMin - amount, original.width + 2 * amount, original.height + 2 * amount);
    }
}
