using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// path is defined by objects with the waypoint tag
public class MovingWallScript : Rideable
{
    [SerializeField] private GameObject[] Path;
    [SerializeField] private bool LoopPath; // false, back and forth
    [SerializeField] private bool StartReversed;
    [SerializeField] private GameObject TrackPrefab;
    private const float MOVE_SPEED = 3.5f;

    private List<Vector2> pathPoints;
    private int nextPointIndex;
    private bool forward; // false: moving backwards through the path
    private Vector2 bufferedMomentum;
    private float momentumBufferTime;
    private float CurrentSpeed { get { return MOVE_SPEED * (InsertedKeyType == KeyState.Fast ? 2 : 1); } }
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
        if(InsertedKeyType == KeyState.Lock) {
            return;
        }

        Vector3 startPosition = transform.position;
        Vector2 target = pathPoints[nextPointIndex];
        float currentSpeed = CurrentSpeed;
        float shift = currentSpeed * Time.deltaTime;

        if(Vector2.Distance(target, transform.position) <= shift) {
            // reached target point
            transform.position = target;
            bufferedMomentum = CurrentSpeed * (pathPoints[nextPointIndex] - (Vector2)startPosition).normalized;
            momentumBufferTime = 0.2f;
            NextWaypoint();

            // apply vertical bump when changing from upward to down and going fast
            Vector2 newDirection = pathPoints[nextPointIndex] - (Vector2)transform.position;
            Vector2 momentum = 1.5f * currentSpeed * (transform.position - startPosition).normalized;
            if(InsertedKeyType == KeyState.Fast && riders.Count > 0 && newDirection.y < -0.9f && momentum.y > 0.9f) {
                momentumBufferTime = 0f; // no buffered momentum in this case
                for(int i = 0; i < riders.Count; i++) {
                    riders[i].GetComponent<Rigidbody2D>().velocity += momentum;

                    if(riders[i].tag == "Player") {
                        // switch player to fall gravity because the grounded gravity is a lot stronger and it cancells the momentum
                        riders[i].GetComponent<Rigidbody2D>().gravityScale = PlayerScript.FALL_GRAVITY;

                        // give the player the boost momentum during their coyote time
                        riders[i].GetComponent<PlayerScript>().CoyoteMomentum = 0.3f * momentum;
                    }
                }
            }
        } else {
            // moving towards target point
            transform.position += shift * ((Vector3)target - transform.position).normalized;
        }

        CheckSideRiders();

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

    protected override void OnRiderAdded(GameObject rider) {
        rider.transform.SetParent(transform.GetChild(0), true);
    }

    protected override void OnRiderRemoved(GameObject rider, int index) {
        rider.transform.SetParent(null);

        // keep rider momentum if moving fast
        if(InsertedKeyType == KeyState.Fast) {
            Vector2 momentum = Momentum;
            if(momentumBufferTime > 0) {
                momentum = bufferedMomentum;
            }
            rider.GetComponent<Rigidbody2D>().velocity += (momentum.x == 0 ? 0.5f : 1f) * momentum; // less boost when upward
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

    protected override void OnKeyInserted(KeyState newKey) {
        if(newKey == KeyState.Reverse) {
            // flip direction
            forward = !forward;
            NextWaypoint();
        }
    }

    protected override void OnKeyRemoved(KeyState removedKey) {
        if(removedKey == KeyState.Reverse) {
            // flip direction
            forward = !forward;
            NextWaypoint();
        }
    }
}
