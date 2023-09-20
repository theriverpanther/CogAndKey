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
        Rect cameraArea = new Rect((Vector2)position - dimensions / 2, dimensions);
        List<Rect> overlapZones = new List<Rect>();
        foreach(Rect cameraBound in LevelData.Instance.LevelAreas) {
            if(cameraBound.Overlaps(cameraArea)) {
                overlapZones.Add(cameraBound);
            }
        }

        if(overlapZones.Count <= 0) {
            return position;
        }

        // form a rectangle from the furthest edges of all overlapped areas
        Rect overlapArea = overlapZones[0]; 
        for(int i = 1; i < overlapZones.Count; i++) {
            overlapArea.xMin = Mathf.Min(overlapArea.xMin, overlapZones[i].xMin);
            overlapArea.xMax = Mathf.Max(overlapArea.xMax, overlapZones[i].xMax);
            overlapArea.yMin = Mathf.Min(overlapArea.yMin, overlapZones[i].yMin);
            overlapArea.yMax = Mathf.Max(overlapArea.yMax, overlapZones[i].yMax);
        }

        if(overlapArea.width < dimensions.x) {
            // average x
            position.x = overlapArea.center.x;
        }
        else if(cameraArea.xMax > overlapArea.xMax) {
            position.x = overlapArea.xMax - dimensions.x / 2;
        }
        else if(cameraArea.xMin < overlapArea.xMin) {
            position.x = overlapArea.xMin + dimensions.x / 2;
        }

        if(overlapArea.height < dimensions.y) {
            // average y
            position.y = overlapArea.center.y;
        }
        else if(cameraArea.yMax > overlapArea.yMax) {
            position.y = overlapArea.yMax - dimensions.y / 2;
        }
        else if(cameraArea.yMin < overlapArea.yMin) {
            position.y = overlapArea.yMin + dimensions.y / 2;
        }

        return position;
    }
}
