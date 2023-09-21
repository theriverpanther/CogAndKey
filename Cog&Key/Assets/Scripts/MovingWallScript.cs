using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// path is defined by children with the waypoint tag
public class MovingWallScript : MonoBehaviour
{
    [SerializeField] private GameObject[] Path;
    [SerializeField] private bool LoopPath; // false, back and forth
    private const float MOVE_SPEED = 3.5f;

    private List<Vector2> pathPoints;
    private int nextPointIndex;
    private bool forward = true; // false: moving backwards through the path

    public KeyState InsertedKey { get; set; }

    void Awake()
    {
        // construct path
        pathPoints = new List<Vector2>();
        pathPoints.Add(transform.position);

        foreach(GameObject point in Path) {
            pathPoints.Add(point.transform.position);
        }
    }

    void Update()
    {
        if(InsertedKey == KeyState.Lock) {
            return;
        }

        Vector2 target = pathPoints[nextPointIndex];
        float displacement = MOVE_SPEED * Time.deltaTime;
        if(InsertedKey == KeyState.Fast) {
            displacement *= 2;
        }

        if(Vector2.Distance(target, transform.position) <= displacement) {
            // reached target point
            transform.position = target;
            NextWaypoint();
        } else {
            // moving towards starting point
            transform.position += displacement * ((Vector3)target - transform.position).normalized;
        }
    }

    private void NextWaypoint() {
        if(forward) {
            nextPointIndex++;

            if(nextPointIndex > pathPoints.Count - 1) {
                if(LoopPath) {
                    nextPointIndex = 0;
                } else {
                    nextPointIndex -= 2;
                    forward = false;
                }
            }
        } else {
            nextPointIndex--;

            if(nextPointIndex < 0) {
                if(LoopPath) {
                    nextPointIndex = pathPoints.Count - 1;
                } else {
                    nextPointIndex = 1;
                    forward = true;
                }
            }
        }
    }
}
