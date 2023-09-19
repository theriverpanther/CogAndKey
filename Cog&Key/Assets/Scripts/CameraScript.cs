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

        List<Rect> adjacentOverlapZones = new List<Rect>();
        Rect? currentZoneOption = null;
        foreach(Rect cameraBound in cameraBounds) {
            if(cameraBound.Contains(position)) {
                if(currentZoneOption != null) {
                    Debug.Log("error: level bounds should not overlap");
                }
                currentZoneOption = cameraBound;
            }
            else if(cameraBound.Overlaps(cameraArea)) {
                adjacentOverlapZones.Add(cameraBound);
            }
        }

        if(!currentZoneOption.HasValue) {
            return position;
        }

        Rect currentZone = currentZoneOption.Value;

        Vector2 topLeft = new Vector2(cameraArea.xMin, cameraArea.yMax);
        Vector2 topRight = new Vector2(cameraArea.xMax, cameraArea.yMax);
        Vector2 bottomLeft = new Vector2(cameraArea.xMin, cameraArea.yMin);
        Vector2 bottomRight = new Vector2(cameraArea.xMax, cameraArea.yMin);
        if(cameraArea.yMax > currentZone.yMax) {
            // top
            if(!IsSideCovered(adjacentOverlapZones, topLeft, topRight)) { 
                position.y = currentZone.yMax - dimensions.y / 2;
            }
        }
        else if(cameraArea.yMin < currentZone.yMin) { 
            // bottom
            if(!IsSideCovered(adjacentOverlapZones, bottomLeft, bottomRight)) {
                position.y = currentZone.yMin + dimensions.y / 2;
            }
        }
        if(cameraArea.xMax > currentZone.xMax) { 
            // right
            if(!IsSideCovered(adjacentOverlapZones, topRight, bottomRight)) {
                position.x = currentZone.xMax - dimensions.x / 2;
            }
        }
        else if(cameraArea.xMin < currentZone.xMin) { 
            // left
            if(!IsSideCovered(adjacentOverlapZones, topLeft, bottomLeft)) {
                position.x = currentZone.xMin + dimensions.x / 2;
            }
        }

        return position;
    }

    // determines if the two corners of a side of a rectangle are within a list of rectangle areas, used for containing the camera
    private bool IsSideCovered(List<Rect> testZones, Vector2 corner1, Vector2 corner2) {
        bool corner1Covered = false;
        bool corner2Covered = false;
        foreach(Rect zone in testZones) { 
            if(zone.Contains(corner1)) {
                corner1Covered = true;
            }
            if(zone.Contains(corner2)) {
                corner2Covered = true;
            }
        }

        return corner1Covered || corner2Covered;
    }
}
