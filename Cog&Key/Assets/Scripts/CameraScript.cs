using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// controls where the camera moves
public class CameraScript : MonoBehaviour
{
    private GameObject player;
    private float z;

    private Vector2 dimensions = new Vector2(17.78f, 10); // hard coded for camera size 5

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        z = transform.position.z;
        transform.position = FindTargetPosition();
    }

    void Update()
    {
        transform.position = FindTargetPosition();
    }

    // determines where the camera should ideally be positioned in the current situation
    private Vector3 FindTargetPosition() {
        Vector3 position = player.transform.position;
        position.z = z;

        // fit the camera into the level bounds
        List<Rect> cameraBounds = LevelData.Instance.LevelAreas;
        Rect cameraArea = new Rect((Vector2)position - dimensions / 2, dimensions);

        List<Rect> overlapZones = new List<Rect>();
        foreach(Rect cameraBound in cameraBounds) {
            if(cameraBound.Overlaps(cameraArea)) {
                overlapZones.Add(cameraBound);
            }
        }

        if(overlapZones.Count <= 0) {
            return position;
        }

        Vector2 topLeft = new Vector2(cameraArea.xMin, cameraArea.yMax);
        Vector2 topRight = new Vector2(cameraArea.xMax, cameraArea.yMax);
        Vector2 bottomLeft = new Vector2(cameraArea.xMin, cameraArea.yMin);
        Vector2 bottomRight = new Vector2(cameraArea.xMax, cameraArea.yMin);

        float? maxY = null;
        float? minY = null;
        float? maxX = null;
        float? minX = null;

        // top
        if(!IsSideCovered(overlapZones, topLeft, topRight)) {
            float max = overlapZones[0].yMax;
            for(int i = 1; i < overlapZones.Count; i++) {
                max = Mathf.Max(overlapZones[i - 1].yMax, overlapZones[i].yMax);
            }

            maxY = max - dimensions.y / 2;
        }
        // bottom
        if(!IsSideCovered(overlapZones, bottomLeft, bottomRight)) {
            float min = overlapZones[0].yMin;
            for(int i = 1; i < overlapZones.Count; i++) {
                min = Mathf.Min(overlapZones[i - 1].yMin, overlapZones[i].yMin);
            }

            minY = min + dimensions.y / 2;
        }
        // right
        if(!IsSideCovered(overlapZones, topRight, bottomRight)) {
            float max = overlapZones[0].xMax;
            for(int i = 1; i < overlapZones.Count; i++) {
                max = Mathf.Max(overlapZones[i - 1].xMax, overlapZones[i].xMax);
            }

            maxX = max - dimensions.x / 2;
        }
        // left
        if(!IsSideCovered(overlapZones, topLeft, bottomLeft)) {
            float min = overlapZones[0].xMin;
            for(int i = 1; i < overlapZones.Count; i++) {
                min = Mathf.Min(overlapZones[i - 1].xMin, overlapZones[i].xMin);
            }

            minX = min + dimensions.x / 2;
        }

        if(maxX.HasValue && minX.HasValue) {
            // average x
            position.x = (maxX.Value + minX.Value) /  2;
        }
        else if(maxX.HasValue) {
            position.x = maxX.Value;
        }
        else if(minX.HasValue) {
            position.x = minX.Value;
        }

        if(maxY.HasValue && minY.HasValue) {
            // average y
            position.y = (maxY.Value + minY.Value) /  2;
            Debug.Log(position.y);
        }
        else if(maxY.HasValue) {
            position.y = maxY.Value;
        }
        else if(minY.HasValue) {
            position.y = minY.Value;
        }

        return position;
    }

    // determines if the two corners and middle of a side of a rectangle are within a list of rectangle areas, used for containing the camera
    // currently considers the whole side covered if either corner is covered
    private bool IsSideCovered(List<Rect> testZones, Vector2 corner1, Vector2 corner2) {
        bool corner1Covered = false;
        bool corner2Covered = false;
        bool middleCovered = false;
        Vector2 middle = (corner1 + corner2) / 2;
        foreach(Rect zone in testZones) { 
            if(zone.Contains(corner1)) {
                corner1Covered = true;
            }
            if(zone.Contains(corner2)) {
                corner2Covered = true;
            }
            if(zone.Contains(middle)) {
                middleCovered = true;
            }
        }

        return corner1Covered || corner2Covered || middleCovered;
    }
}
