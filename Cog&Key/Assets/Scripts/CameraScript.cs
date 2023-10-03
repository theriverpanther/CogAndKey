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
        // ideally place the camera near the player
        Vector3 position = player.transform.position;
        position.x += 3; // face the future of the level
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
        if(!topLeftCovered && !topRightCovered) {

        }
        else if(!topLeftCovered && !topRightCovered) {

        }
        if(!topLeftCovered && !topRightCovered) {

        }
        else if(!topLeftCovered && !topRightCovered) {

        }

        // form a rectangle from the furthest edges of all overlapped areas and another from the closest edges
        //Rect maxArea = overlapZones[0];
        //Rect minArea = overlapZones[0];
        //for(int i = 1; i < overlapZones.Count; i++) {
        //    maxArea.xMin = Mathf.Min(maxArea.xMin, overlapZones[i].xMin);
        //    maxArea.xMax = Mathf.Max(maxArea.xMax, overlapZones[i].xMax);
        //    maxArea.yMin = Mathf.Min(maxArea.yMin, overlapZones[i].yMin);
        //    maxArea.yMax = Mathf.Max(maxArea.yMax, overlapZones[i].yMax);

        //    minArea.xMin = Mathf.Max(minArea.xMin, overlapZones[i].xMin);
        //    minArea.xMax = Mathf.Min(minArea.xMax, overlapZones[i].xMax);
        //    minArea.yMin = Mathf.Max(minArea.yMin, overlapZones[i].yMin);
        //    minArea.yMax = Mathf.Min(minArea.yMax, overlapZones[i].yMax);
        //}

        //if(maxArea.width < dimensions.x) {
        //    // average x
        //    position.x = maxArea.center.x;
        //}
        //else if(cameraArea.xMax > maxArea.xMax) {
        //    position.x = maxArea.xMax - dimensions.x / 2;
        //}
        //else if(cameraArea.xMin < maxArea.xMin) {
        //    position.x = maxArea.xMin + dimensions.x / 2;
        //}

        //if(maxArea.height < dimensions.y) {
        //    // average y
        //    position.y = maxArea.center.y;
        //}
        //else if(cameraArea.yMax > maxArea.yMax) {
        //    position.y = maxArea.yMax - dimensions.y / 2;
        //}
        //else if(cameraArea.yMin < maxArea.yMin) {
        //    position.y = maxArea.yMin + dimensions.y / 2;
        //}

        return position;
    }
}
