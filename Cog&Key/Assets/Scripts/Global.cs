using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

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

    public static Rect MakeExpanded(this Rect original, float amount) {
        return new Rect(original.xMin - amount, original.yMin - amount, original.width + 2 * amount, original.height + 2 * amount);
    }

    public static Rect GetCollisionArea(GameObject objectWithCollider) {
        Vector2 size = objectWithCollider.GetComponent<Collider2D>().bounds.size;
        return new Rect((Vector2)objectWithCollider.transform.position - size / 2, size);
    }

    public static Direction GetOpposite(Direction direction) {
        switch(direction) {
            case Direction.Up:
                return Direction.Down;

            case Direction.Down:
                return Direction.Up;

            case Direction.Left:
                return Direction.Right;

            case Direction.Right:
                return Direction.Left;
        }

        return Direction.None;
    }
}
