using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// controls where the camera moves
public class CameraScript : MonoBehaviour
{
    private GameObject player;
    private float z;
    private CamerBoundType lastLook;
    private float scrollCD;

    public static CameraScript Instance { get; private set; }

    public Vector2 Dimensions { get {
        Vector2 dimensions = new Vector2();
        dimensions.y = GetComponent<Camera>().orthographicSize * 2;
        dimensions.x = dimensions.y * 16/9; // 16/9 aspect ratio
        return dimensions;
    } }

    void Awake()
    {
        Instance = this;
        player = GameObject.FindWithTag("Player");
        z = transform.position.z;
        if(LevelData.Instance != null) {
            SetInitialPosition();
            scrollCD = 0;
        }
    }

    // called by LevelData.cs Start() after generating the level bounds
    public void SetInitialPosition() {
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

        if(scrollCD > 0) {
            scrollCD -= Time.deltaTime;
        }
    }

    // determines where the camera should ideally be positioned in the current situation
    private Vector3 FindTargetPosition() {
        // ideally place the camera near the player
        Vector3 position = player.transform.position;

        // find which region the player is in
        LevelBoundScript playerZone = null;
        foreach(LevelBoundScript cameraBound in LevelData.Instance.LevelAreas) {
            if(cameraBound.Area.Contains(position)) {
                playerZone = cameraBound;
                break;
            }
        }

        if(playerZone == null) {
            return transform.position; // stay still if the player goes off screen
        }

        // prevent scrolling back and forth
        CamerBoundType currentScroll = playerZone.AreaType;
        if(scrollCD > 0) {
            currentScroll = lastLook;
        }
        else if(currentScroll != lastLook) {
            lastLook = currentScroll;
            scrollCD = 1.0f;
        }

        // face the camera towards the end of the level
        List<Rect> cameraAreas = LevelData.Instance.CameraZones;
        switch(currentScroll) {
            case CamerBoundType.Right:
                position.x += 4;
                break;

            case CamerBoundType.Left:
                position.x -= 4;
                break;

            case CamerBoundType.Up:
                position.y += 2.5f;
                break;

            case CamerBoundType.Down:
                position.y -= 2.5f;
                break;

            case CamerBoundType.Lock:
                Rect playerRect = playerZone.Area;
                Vector2 dimensions = Dimensions;
                cameraAreas = new List<Rect> { new Rect(playerRect.x + dimensions.x/2, playerRect.y + dimensions.y/2, playerRect.width - dimensions.x, playerRect.height - dimensions.y) };
                break;
        }

        // lock the camera inside the available areas
        Vector3 closestPoint = Vector3.zero;
        float closestDistance = int.MaxValue;
        foreach(Rect cameraArea in cameraAreas) {
            if(cameraArea.Contains(position)) {
                position.z = z;
                return position;
            }

            // find the closest point on this area
            Vector3 clampedPosition = new Vector3(
                Mathf.Min(Mathf.Max(cameraArea.xMin, position.x), cameraArea.xMax),
                Mathf.Min(Mathf.Max(cameraArea.yMin, position.y), cameraArea.yMax),
                0
            );

            // determine if it is the closest
            float distance = Vector3.Distance(position, clampedPosition);
            if(distance < closestDistance) {
                closestDistance = distance;
                closestPoint = clampedPosition;
            }
        }

        closestPoint.z = z;
        return closestPoint;
    }
}
