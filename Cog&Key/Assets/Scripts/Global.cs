using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
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
        BoxCollider2D collider = rectangleObject.GetComponent<BoxCollider2D>();
        Vector2 absPerp = new Vector2(Mathf.Abs(cardinalDirection.y), Mathf.Abs(cardinalDirection.x));
        Vector3 lossyScale = rectangleObject.transform.lossyScale;
        lossyScale = new Vector3(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y), Mathf.Abs(lossyScale.z));
        Vector2 objectSize = lossyScale * collider.size;
        Vector2 scale = objectSize * absPerp;
        scale = new Vector2(scale.x == 0 ? thickness : scale.x - 0.06f, scale.y == 0 ? thickness : scale.y - 0.06f);
        RaycastHit2D[] boxcastHits = Physics2D.BoxCastAll((Vector2)rectangleObject.transform.position + (objectSize / 2f + new Vector2(thickness, thickness)) * cardinalDirection,
            scale, 
            0f, cardinalDirection, 0.01f);

        // if there is a child blocking, check if the child is blocked instead
        foreach(RaycastHit2D boxHit in boxcastHits) {
            if(boxHit.collider.gameObject.GetComponent<KeyPlug>() != null) continue;

            Transform current = boxHit.collider.transform;
            bool isChild = false;
            while(current.parent != null) {
                if(current.parent == rectangleObject.transform) { // if the boxcast hit a child
                    isChild = true;
                    if(IsObjectBlocked(boxHit.collider.gameObject, cardinalDirection)) {
                        return true;
                    }
                    break;
                }
                current = current.parent;
            }

            if(!isChild) {
                return true;
            }
        }

        return false;
    }
}
