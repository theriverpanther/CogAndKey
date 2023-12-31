using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorBeltScript : Rideable
{
    [SerializeField] private bool clockwise;
    [SerializeField] private GameObject TickMarkPrefab;
    private const float SHIFT_SPEED = 5.0f;
    private float ShiftSpeed {  get { return SHIFT_SPEED * (InsertedKeyType == KeyState.Fast ? 2f : 1f) * (InsertedKeyType == KeyState.Lock ? 0f : 1f); } }

    private List<Vector3> shiftDirections = new List<Vector3>(); // index matches the list of riders. This allows the code to calculate the shift direction once when first touching
    private List<GameObject> attachedDuplicateRiders = new List<GameObject>(); // stores riders that are already being pushed in the same direction by an adjacent belt
    private GameObject stopVelocityNextFrame; // used because the player can jump the same frame it makes contact and get a super boost otherwise
    private int frameCount;

    private static Dictionary<GameObject, List<Vector3>> allBeltRiders; // prevents multiple belts affecting things at the seams

    // temp visual spin
    private List<GameObject> topTicks = new List<GameObject>();
    private List<GameObject> bottomTicks = new List<GameObject>();
    private List<GameObject> leftTicks = new List<GameObject>();
    private List<GameObject> rightTicks = new List<GameObject>();
    private float visualTimer;

    void Start()
    {
        allBeltRiders = new Dictionary<GameObject, List<Vector3>>();
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
        if(stopVelocityNextFrame != null) {
            frameCount--;
            if(frameCount <= 0) {
                stopVelocityNextFrame.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                stopVelocityNextFrame = null;
            }
        }

        CheckSideRiders();

        // check for an adjacent belt passing something onto this
        for(int i = attachedDuplicateRiders.Count - 1; i >= 0; i--) {
            Vector3 shiftDir = DetermineShiftDirection(attachedDuplicateRiders[i]);
            if(!allBeltRiders[attachedDuplicateRiders[i]].Contains(shiftDir)) {
                riders.Add(attachedDuplicateRiders[i]);
                OnRiderAdded(attachedDuplicateRiders[i]);
                attachedDuplicateRiders.RemoveAt(i);
            }
        }

        // shift riders
        for(int i = 0; i < riders.Count; i++) {
            Vector3 shiftDir = shiftDirections[i];
            riders[i].transform.position += ShiftSpeed * Time.deltaTime * shiftDir;

            // cancel out gravity when on the side
            if(shiftDir == Vector3.up && InsertedKeyType != KeyState.Lock) {
                Rigidbody2D physBod = riders[i].GetComponent<Rigidbody2D>();
                physBod.AddForce(-Physics2D.gravity * physBod.gravityScale);

                if(InsertedKeyType == KeyState.Fast) {
                    physBod.AddForce(-Physics2D.gravity * physBod.gravityScale); // shift more with upward fast key
                }
            }
        }
        
        // update temp visuals
        float lastTime = visualTimer;
        visualTimer += ShiftSpeed * 0.5f * Time.deltaTime * (clockwise ? 1 : -1);
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
        Vector3 shiftDir = DetermineShiftDirection(rider);

        if(allBeltRiders.ContainsKey(rider) && allBeltRiders[rider].Contains(shiftDir)) {
            // prevent multiple belts from shifting something in the same direction
            riders.Remove(rider);
            attachedDuplicateRiders.Add(rider);
            return;
        }

        shiftDirections.Add(shiftDir);
        if(!allBeltRiders.ContainsKey(rider)) {
            allBeltRiders[rider] = new List<Vector3>();
        }
        allBeltRiders[rider].Add(shiftDir);

        if(InsertedKeyType != KeyState.Lock && (shiftDir == Vector3.up || shiftDir == Vector3.down)) { // if on the side
            stopVelocityNextFrame = rider;
            frameCount = 2;
        }
    }

    protected override void OnRiderRemoved(GameObject rider, int index) {
        // keep rider momentum if moving fast
        if(InsertedKeyType == KeyState.Fast) {
            Vector2 launchDir = shiftDirections[index];
            rider.GetComponent<Rigidbody2D>().velocity += ShiftSpeed * (launchDir == Vector2.up ? 0.5f : 0.8f) * launchDir;
        }

        allBeltRiders[rider].Remove(shiftDirections[index]);
        shiftDirections.RemoveAt(index);
    }

    // on collision exit 2D for sub class
    protected override void SubCollisionExit(Collision2D collision) {
        if(attachedDuplicateRiders.Contains(collision.gameObject)) {
            attachedDuplicateRiders.Remove(collision.gameObject);
        }
    }

    protected override void OnKeyInserted(KeyState newKey) {
        if(newKey == KeyState.Reverse) {
            ReverseDirection();
        }
    }

    protected override void OnKeyRemoved(KeyState removedKey) {
        if(removedKey == KeyState.Reverse) {
            ReverseDirection();
        }
    }

    private void ReverseDirection() {
        clockwise = !clockwise;
        for(int i = 0; i < riders.Count; i++) {
            List<Vector3> allRiderDirections = allBeltRiders[riders[i]];
            int reverseIndex = allRiderDirections.IndexOf(shiftDirections[i]);
            allRiderDirections[reverseIndex] = -allRiderDirections[reverseIndex];
            shiftDirections[i] = -shiftDirections[i];
        }

    }

    // only works for objects touching the collider
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
