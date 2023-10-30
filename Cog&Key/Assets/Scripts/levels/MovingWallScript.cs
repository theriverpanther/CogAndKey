using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// path is defined by objects with the waypoint tag
public class MovingWallScript : Rideable, IKeyWindable
{
    [SerializeField] private GameObject[] Path;
    [SerializeField] private bool LoopPath; // false, back and forth
    [SerializeField] private bool StartReversed;
    [SerializeField] private GameObject TrackPrefab;
    private const float MOVE_SPEED = 3.5f;

    private List<Vector2> pathPoints;
    private int nextPointIndex;
    private bool forward; // false: moving backwards through the path
    private KeyState currentKey;
    private float CurrentSpeed { get { return MOVE_SPEED * (currentKey == KeyState.Fast ? 2 : 1); } }
    private Vector2 Momentum { get { return CurrentSpeed * (pathPoints[nextPointIndex] - (Vector2)transform.position).normalized; } }

    void Awake()
    {
        forward = !StartReversed;

        // construct path
        pathPoints = new List<Vector2>() { transform.position };

        foreach(GameObject point in Path) {
            pathPoints.Add(point.transform.position);
        }

        // place visual tracks
        float trackWidth = TrackPrefab.transform.localScale.x;
        for(int i = 0; i < pathPoints.Count - 1; i++) {
            PlaceTrack(pathPoints[i], pathPoints[i+1]);
        }

        if(LoopPath) {
            PlaceTrack(pathPoints[pathPoints.Count -1], pathPoints[0]);
        }
    }

    void FixedUpdate()
    {
        if(currentKey == KeyState.Lock) {
            return;
        }

        Vector3 startPosition = transform.position;
        Vector2 target = pathPoints[nextPointIndex];
        float currentSpeed = CurrentSpeed;
        float shift = currentSpeed * Time.deltaTime;

        if(Vector2.Distance(target, transform.position) <= shift) {
            // reached target point
            transform.position = target;
            NextWaypoint();

            // apply shift momentum to riders when changing direction
            Vector2 newDirection = pathPoints[nextPointIndex] - (Vector2)transform.position;
            Vector2 momentum = currentSpeed * (transform.position - startPosition).normalized;
            if(currentKey == KeyState.Fast && riders.Count > 0 && newDirection.y < 0 && momentum.y > 0) {
                for(int i = 0; i < riders.Count; i++) {
                    riders[i].GetComponent<Rigidbody2D>().velocity += momentum;
                }
            }
        } else {
            // moving towards target point
            transform.position += shift * ((Vector3)target - transform.position).normalized;
        }

        CheckSideRiders();
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

    protected override void OnRiderAdded(GameObject rider) {
        rider.transform.SetParent(transform, true);
    }

    protected override void OnRiderRemoved(GameObject rider) {
        rider.transform.SetParent(null);

        // keep rider momentum if moving fast
        if(currentKey == KeyState.Fast) {
            rider.GetComponent<Rigidbody2D>().velocity += Momentum;
        }
    }

    private void PlaceTrack(Vector2 start, Vector2 end) {
        float trackWidth = TrackPrefab.transform.localScale.x;
        GameObject track = Instantiate(TrackPrefab);
        track.transform.position = (start + end) / 2;
        if (Mathf.Abs(start.x - end.x) < 1.0f)
        {
            track.transform.localScale = new Vector3(trackWidth, Mathf.Abs(start.y - end.y) + trackWidth, 1);
        }
        else if (Mathf.Abs(start.y - end.y) < 1.0f)
        {
            track.transform.localScale = new Vector3(Mathf.Abs(start.x - end.x) + trackWidth, trackWidth, 1);
        }
    }

    public void InsertKey(KeyState key) {
        if( (currentKey == KeyState.Reverse) != (key == KeyState.Reverse) ) {
            // turning reversing key on or off
            forward = !forward;
            NextWaypoint();
        }

        currentKey = key;
    }
}
