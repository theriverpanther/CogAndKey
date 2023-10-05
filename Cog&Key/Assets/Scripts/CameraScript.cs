using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// controls where the camera moves
public class CameraScript : MonoBehaviour
{
    private GameObject player;
    private float z;

    private Vector2 dimensions;

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        z = transform.position.z;

        dimensions.y = GetComponent<Camera>().orthographicSize * 2;
        dimensions.x = dimensions.y * 16/9; // 16/9 aspect ratio

        transform.position = FindTargetPosition();
    }

    void FixedUpdate()
    {
        float speed = 12f;
        float shift = speed * Time.deltaTime;
        Vector3 target = FindTargetPosition();
        float distance = Vector3.Distance(transform.position, target);
        if(distance <= shift) {
            transform.position = target;
        } else {
            transform.position += shift * (target - transform.position).normalized;
        }
    }

    // determines where the camera should ideally be positioned in the current situation
    private Vector3 FindTargetPosition() {
        // ideally place the camera near the player
        Vector3 position = player.transform.position;
        position.z = z;

        // fit the camera into the level bounds
        LevelBoundScript playerBound = null;
        Rect cameraArea = new Rect((Vector2)position - dimensions / 2, dimensions);
        List<Rect> overlapZones = new List<Rect>();
        foreach(LevelBoundScript cameraBound in LevelData.Instance.LevelAreas) {
            if(cameraBound.Area.Overlaps(cameraArea)) {
                overlapZones.Add(cameraBound.Area);
            }

            if(cameraBound.Area.Contains(player.transform.position)) {
                playerBound = cameraBound;
            }
        }

        if(overlapZones.Count <= 0) {
            return position;
        }

        // shift camera towards the next part of the level
        if(playerBound != null) {
            switch(playerBound.AreaType) {
                case LevelBoundScript.Type.Right:
                    position.x += 4;
                    break;

                case LevelBoundScript.Type.Left:
                    position.x -= 4;
                    break;

                case LevelBoundScript.Type.Up:
                    position.y += 2.5f;
                    break;

                case LevelBoundScript.Type.Down:
                    position.y -= 2.5f;
                    break;

                case LevelBoundScript.Type.Lock:
                    break;
            }
        }

        // form a rectangle from the furthest edges of all overlapped areas and another from the closest edges
        Rect maxArea = overlapZones[0];
        Rect minArea = overlapZones[0];
        for(int i = 1; i < overlapZones.Count; i++) {
            maxArea.xMin = Mathf.Min(maxArea.xMin, overlapZones[i].xMin);
            maxArea.xMax = Mathf.Max(maxArea.xMax, overlapZones[i].xMax);
            maxArea.yMin = Mathf.Min(maxArea.yMin, overlapZones[i].yMin);
            maxArea.yMax = Mathf.Max(maxArea.yMax, overlapZones[i].yMax);

            minArea.xMin = Mathf.Max(minArea.xMin, overlapZones[i].xMin);
            minArea.xMax = Mathf.Min(minArea.xMax, overlapZones[i].xMax);
            minArea.yMin = Mathf.Max(minArea.yMin, overlapZones[i].yMin);
            minArea.yMax = Mathf.Min(minArea.yMax, overlapZones[i].yMax);
        }

        // determine which corners of the camera are outside the bounds
        bool topLeftCovered = false;
        bool topRightCovered = false;
        bool bottomLeftCovered = false;
        bool bottomRightCovered = false;
        Vector2 leftTop = new Vector2(cameraArea.xMin, cameraArea.yMax);
        Vector2 rightTop = new Vector2(cameraArea.xMax, cameraArea.yMax);
        Vector2 leftBot = new Vector2(cameraArea.xMin, cameraArea.yMin);
        Vector2 rightBot = new Vector2(cameraArea.xMax, cameraArea.yMin);
        foreach(Rect bound in overlapZones) {
            if(bound.Contains(leftTop)) topLeftCovered = true;
            if(bound.Contains(rightTop)) topRightCovered = true;
            if(bound.Contains(leftBot)) bottomLeftCovered = true;
            if(bound.Contains(rightBot)) bottomRightCovered = true;
        }

        // shift camera inside the bounds
        bool shifted = false;
        if(!bottomLeftCovered && !bottomRightCovered) {
            position.y = maxArea.yMin + dimensions.y/2;
            shifted = true;
        }
        else if(!topLeftCovered && !topRightCovered) {
            position.y = minArea.yMax - dimensions.y/2;
            shifted = true;
        }
        if(!topLeftCovered && !bottomLeftCovered) {
            position.x = maxArea.xMin + dimensions.x/2;
            shifted = true;
        }
        else if(!topRightCovered && !bottomRightCovered) {
            position.x = maxArea.xMax - dimensions.x/2;
            shifted = true;
        }

        if(shifted) {
            return position;
        }

        // check for individual corners sticking out
        Vector2? option1 = null;
        Vector2? option2 = null;
        Vector2 shiftRight = new Vector2(minArea.xMin + dimensions.x/2, position.y);
        Vector2 shiftLeft = new Vector2(minArea.xMax - dimensions.x/2, position.y);
        Vector2 shiftUp = new Vector2(position.x, minArea.yMin + dimensions.y/2);
        Vector2 shiftDown = new Vector2(position.x, minArea.yMax - dimensions.y / 2);
        if (!topLeftCovered) {
            option1 = shiftRight;
            option2 = shiftDown;
        }
        else if(!topRightCovered) {
            option1 = shiftLeft;
            option2 = shiftDown;
        }
        else if(!bottomLeftCovered) {
            option1 = shiftRight;
            option2 = shiftUp;
        }
        else if(!bottomRightCovered) {
            option1 = shiftLeft;
            option2 = shiftUp;
        }

        if(option1.HasValue && option2.HasValue) {
            position = (Vector2.Distance(position, option1.Value) < Vector2.Distance(position, option2.Value) ? option1.Value : option2.Value);
            position.z = z;
            return position;
        }

        return position;
    }
}
