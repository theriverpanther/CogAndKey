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
    private float freezeTime;
    private bool forward; // false: moving backwards through the path
    private Vector2 bufferedMomentum;
    private float momentumBufferTime;
    private float CurrentSpeed { get { return MOVE_SPEED * (InsertedKeyType == KeyState.Fast ? 2 : 1); } }

    [SerializeField]
    Gear[] gears;

    void Awake()
    {
        gears = transform.GetComponentsInChildren<Gear>();
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

        if(StartReversed)
        {
            UpdateGears(KeyState.Reverse);
        } else
        {
            UpdateGears(KeyState.None);
        }


    }

    void FixedUpdate()
    {
        if(freezeTime > 0) {
            freezeTime -= (InsertedKeyType == KeyState.Fast ? 2 : 1) * Time.deltaTime;
            return;
        }

        if(InsertedKeyType == KeyState.Lock) {
            return;
        }

        Vector3 startPosition = transform.position;
        Vector2 target = pathPoints[nextPointIndex];
        float currentSpeed = CurrentSpeed;
        float shift = currentSpeed * Time.deltaTime;

        if(Vector2.Distance(target, transform.position) <= Mathf.Max(0.1f, shift)) {
            // reached target point
            transform.position = target;
            bufferedMomentum = BecomeMomentum(pathPoints[nextPointIndex] - (Vector2)startPosition);
            momentumBufferTime = 0.2f;
            freezeTime = 0.05f;
            NextWaypoint();

            // apply vertical bump when changing from upward to down and going fast
            Vector2 newDirection = pathPoints[nextPointIndex] - (Vector2)transform.position;
            if(InsertedKeyType == KeyState.Fast && riders.Count > 0 && newDirection.y < -0.9f && bufferedMomentum.y > 0.9f) {
                momentumBufferTime = 0f; // no buffered momentum in this case
                for(int i = 0; i < riders.Count; i++) {
                    riders[i].GetComponent<Rigidbody2D>().velocity += bufferedMomentum;
                    if(riders[i].tag == "Player") {
                        // switch player to fall gravity because the grounded gravity is a lot stronger and it cancells the momentum
                        riders[i].GetComponent<Rigidbody2D>().gravityScale = PlayerScript.JUMP_GRAVITY;
                        riders[i].GetComponent<PlayerScript>().CoyoteMomentum = bufferedMomentum;
                    }

                    riders[i].transform.SetParent(null);
                    riders.RemoveAt(i);
                    i--;
                }
            }
        } else {
            // moving towards target point
            Vector3 direction = ((Vector3)target - transform.position).normalized;
            if(!Global.IsObjectBlocked(gameObject, direction)) {
                transform.position += shift * direction;
            }
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
        if(rider.transform.parent == null || rider.transform.parent.gameObject.activeInHierarchy) {
            rider.transform.SetParent(null);
        }

        // keep rider momentum if moving fast
        if(InsertedKeyType == KeyState.Fast) {
            rider.GetComponent<Rigidbody2D>().velocity += momentumBufferTime > 0 ? bufferedMomentum : BecomeMomentum(pathPoints[nextPointIndex] - (Vector2)transform.position);
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

        UpdateGears(newKey);
    }

    protected override void OnKeyRemoved(KeyState removedKey) {
        if(removedKey == KeyState.Reverse) {
            // flip direction
            forward = !forward;
            NextWaypoint();
        }

        UpdateGears(KeyState.None);
    }

    private void UpdateGears(KeyState s)
    {
        foreach (Gear g in gears)
        {
            g.ChangeDirection(s, forward);
        }
    }

    private Vector2 BecomeMomentum(Vector2 directionNonNorm) {
        directionNonNorm.Normalize();
        directionNonNorm *= CurrentSpeed;
        if(directionNonNorm.x == 0) {
            directionNonNorm *= 0.5f;
        }
        return directionNonNorm;
    }
}
