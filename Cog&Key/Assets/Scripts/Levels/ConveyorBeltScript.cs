using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorBeltScript : Rideable, IKeyWindable
{
    [SerializeField] private bool clockwise;
    [SerializeField] private GameObject TickMarkPrefab;
    private const float SHIFT_SPEED = 5.0f;
    private KeyState insertedKey = KeyState.Normal;
    private float ShiftSpeed {  get { return SHIFT_SPEED * (insertedKey == KeyState.Fast ? 2f : 1f); } }

    // temp visual spin
    private List<GameObject> topTicks = new List<GameObject>();
    private List<GameObject> bottomTicks = new List<GameObject>();
    private List<GameObject> leftTicks = new List<GameObject>();
    private List<GameObject> rightTicks = new List<GameObject>();
    private float visualTimer;

    void Start()
    {
        Rect area = Global.GetCollisionArea(gameObject);

        float tickHalfWidth = TickMarkPrefab.transform.localScale.y / 2;

        // horizontal marks
        for(float x = area.xMin + 0.5f; x < area.xMax; x += 0.5f) {
            GameObject addedTop = Instantiate(TickMarkPrefab);
            topTicks.Add(addedTop);
            addedTop.transform.position = new Vector3(x, area.yMax - tickHalfWidth, 0);

            GameObject addedBottom = Instantiate(TickMarkPrefab);
            bottomTicks.Add(addedBottom);
            addedBottom.transform.position = new Vector3(x, area.yMin + tickHalfWidth, 0);
        }

        // vertical marks
        for(float y = area.yMin + 0.5f; y < area.yMax; y += 0.5f) {
            GameObject addedLeft = Instantiate(TickMarkPrefab);
            leftTicks.Add(addedLeft);
            addedLeft.transform.position = new Vector3(area.xMin + tickHalfWidth, y, 0);
            addedLeft.transform.rotation = Quaternion.Euler(0, 0, 90);

            GameObject addedRight = Instantiate(TickMarkPrefab);
            rightTicks.Add(addedRight);
            addedRight.transform.position = new Vector3(area.xMax - tickHalfWidth, y, 0);
            addedRight.transform.rotation = Quaternion.Euler(0, 0, 90);
        }
    }

    void FixedUpdate() {
        CheckSideRiders();

        // shift riders
        foreach(GameObject rider in riders) {
            rider.transform.position += ShiftSpeed * Time.deltaTime * DetermineShiftDirection(rider);

            // cancel out gravity when on the side
            if(OnSide(rider)) {
                Rigidbody2D physBod = rider.GetComponent<Rigidbody2D>();
                physBod.AddForce(-Physics2D.gravity * physBod.gravityScale);
            }
        }

        // update temp visuals
        float lastTime = visualTimer;
        visualTimer += ShiftSpeed * 0.7f * Time.deltaTime * (clockwise ? 1 : -1);
        if(visualTimer >= 0.5f || visualTimer <= -0.5f) {
            visualTimer = 0;
        }
        float shift = visualTimer - lastTime;

        foreach(GameObject top in topTicks) {
            Vector3 pos = top.transform.position;
            pos.x += shift;
            top.transform.position = pos;
        }

        foreach(GameObject bottom in bottomTicks) {
            Vector3 pos = bottom.transform.position;
            pos.x -= shift;
            bottom.transform.position = pos;
        }

        foreach(GameObject left in leftTicks) {
            Vector3 pos = left.transform.position;
            pos.y += shift;
            left.transform.position = pos;
        }

        foreach(GameObject right in rightTicks) {
            Vector3 pos = right.transform.position;
            pos.y -= shift;
            right.transform.position = pos;
        }
    }

    protected override void OnRiderAdded(GameObject rider) {
        if(OnSide(rider)) {
            rider.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }
    }

    protected override void OnRiderRemoved(GameObject rider) {
        // keep rider momentum if moving fast
        if(insertedKey == KeyState.Fast) {
            rider.GetComponent<Rigidbody2D>().velocity += ShiftSpeed * 0.7f * (Vector2)DetermineShiftDirection(rider);
        }
    }

    public void InsertKey(KeyState key) {
        if( (insertedKey == KeyState.Reverse) != (key == KeyState.Reverse) ) {
            // flip direction
            clockwise = !clockwise;
        }

        insertedKey = key;
    }

    private Vector3 DetermineShiftDirection(GameObject rider) {
        if(OnSide(rider)) {
            // left or right
            if(clockwise == (rider.transform.position.x < transform.position.x)) {
                return Vector3.up;
            } else {
                return Vector3.down;
            }
        } else {
            // top or bottom
            if(clockwise == (rider.transform.position.y > transform.position.y)) {
                return Vector3.right;
            } else {
                return Vector3.left;
            }
        }
    }
}
