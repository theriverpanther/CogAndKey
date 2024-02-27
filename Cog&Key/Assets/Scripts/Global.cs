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

    public static bool IsObjectBlocked(GameObject rectangleObject, Vector2 cardinalDirection) {
        float thickness = 0.05f;
        Vector2 absPerp = new Vector2(Mathf.Abs(cardinalDirection.y), Mathf.Abs(cardinalDirection.x));
        Vector2 scale = rectangleObject.transform.lossyScale * absPerp;
        scale = new Vector2(scale.x == 0 ? thickness : scale.x - 0.02f, scale.y == 0 ? thickness : scale.y - 0.02f);
        RaycastHit2D raycast = Physics2D.BoxCast((Vector2)rectangleObject.transform.position + ((Vector2)rectangleObject.transform.lossyScale / 2f + new Vector2(thickness, thickness)) * cardinalDirection,
            scale, 
            0f, cardinalDirection, 0.01f);

        // ignore children of this game object
        if(raycast.collider != null) {
            Transform current = raycast.collider.transform;
            while(current != null) {
                if(current == rectangleObject.transform) {
                    return false;
                }
                current = current.parent;
            }
        }

        return raycast.collider != null;
    }
}
