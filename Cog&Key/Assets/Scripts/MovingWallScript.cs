using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// path is defined by children with the waypoint tag
public class MovingWallScript : MonoBehaviour, IKeyWindable
{
    [SerializeField] private GameObject[] Path;
    [SerializeField] private bool LoopPath; // false, back and forth
    private const float MOVE_SPEED = 3.5f;

    private List<Vector2> pathPoints;
    private int nextPointIndex;
    private bool forward = true; // false: moving backwards through the path
    private List<GameObject> riders = new List<GameObject>();
    private KeyState currentKey;
    private float momentumBufferTime;
    private Vector2 bufferedMomentum;

    void Awake()
    {
        // construct path
        pathPoints = new List<Vector2>() { transform.position };

        foreach(GameObject point in Path) {
            pathPoints.Add(point.transform.position);
        }
    }

    void FixedUpdate()
    {
        if(currentKey == KeyState.Lock) {
            return;
        }

        Vector3 startPosition = transform.position;
        Vector2 target = pathPoints[nextPointIndex];
        float currentSpeed = MOVE_SPEED;
        if(currentKey == KeyState.Fast) {
            currentSpeed *= 2;
        }
        float shift = currentSpeed * Time.deltaTime;

        if(Vector2.Distance(target, transform.position) <= shift) {
            // reached target point
            transform.position = target;
            NextWaypoint();

            // buffer momentum when changing direction
            momentumBufferTime = 0.2f;
            bufferedMomentum = currentSpeed * (transform.position - startPosition).normalized;
        } else {
            // moving towards target point
            transform.position += shift * ((Vector3)target - transform.position).normalized;
        }

        Vector3 displacement = transform.position - startPosition;
        Rect platformArea = Global.GetCollisionArea(gameObject);
        for(int i = 0; i < riders.Count; i++) {
            riders[i].transform.position += displacement;

            // check if no longer a rider
            Rect riderArea = Global.GetCollisionArea(riders[i]);
            Rigidbody2D riderRB = riders[i].GetComponent<Rigidbody2D>();
            float BUFFER = 0.05f;
            if(riderArea.yMin > platformArea.yMax + BUFFER
                || riderArea.yMax < platformArea.yMin - BUFFER
                || riderArea.xMin > platformArea.xMax + BUFFER
                || riderArea.xMax < platformArea.xMin - BUFFER

                || OnSide(riderArea) && 
                    (riderArea.center.x < platformArea.center.x && riderRB.velocity.x <= 0 
                    || riderArea.center.x > platformArea.center.x && riderRB.velocity.x >= 0)
            ) {
                if(currentKey == KeyState.Fast) {
                    if(momentumBufferTime > 0) {
                        riderRB.velocity += bufferedMomentum;
                    } else {
                        riderRB.velocity += currentSpeed * (Vector2)displacement.normalized;
                    }
                }

                riders.RemoveAt(i);
                i--;

            }
        }

        if(momentumBufferTime > 0) {
            momentumBufferTime -= Time.deltaTime;
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

    private void OnCollisionEnter2D(Collision2D collision) {
        Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
        if(rb != null && !riders.Contains(collision.gameObject)) {
            riders.Add(collision.gameObject);
        }
    }

    // checks if the input collider is not above or below this wall
    private bool OnSide(Rect collider) {
        Rect platformArea = Global.GetCollisionArea(gameObject);
        return collider.yMin < platformArea.yMax && collider.yMax > platformArea.yMin;
    }

    public void InsertKey(KeyState key) {
        if(currentKey != key && (currentKey == KeyState.Reverse || key == KeyState.Reverse)) {
            forward = !forward;
            NextWaypoint();
        }

        currentKey = key;
    }
}
