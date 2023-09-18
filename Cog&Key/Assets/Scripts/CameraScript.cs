using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// controls where the camera moves
public class CameraScript : MonoBehaviour
{
    private GameObject player;
    private float z;

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
        return position;
    }
}
