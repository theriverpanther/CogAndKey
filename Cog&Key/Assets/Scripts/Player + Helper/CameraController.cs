using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject visibleWindow;

    private const float WINDOW_WIDTH = 4f;
    private const float WINDOW_HEIGHT = 2f;
    private const float WINDOW_X_LIMIT = 9f;
    private const float WINDOW_Y_LIMIT = 5f;

    private const float HORIZONTAL_MOVE_RATE = 0.1f;
    private const float VERTICAL_MOVE_RATE = 0.05f;

    private const float WINDOW_CENTER_X_LIMIT = WINDOW_X_LIMIT - WINDOW_WIDTH / 2f;
    private const float WINDOW_CENTER_Y_LIMIT = WINDOW_Y_LIMIT - WINDOW_HEIGHT / 2f;

    private const float FOCUS_OUTER_RADIUS = 12.0f;
    private const float FOCUS_INNER_RADIUS = 8.0f;
    private const float FOCUS_MAX_RATIO = 0.5f;

    private PlayerScript player;
    private float fixedZ;
    private Rect playerWindow;
    private List<Vector2> focusPoints;

    public static CameraController Instance { get; private set; }

    public Vector2 Dimensions { get {
        Vector2 dimensions = new Vector2();
        dimensions.y = GetComponent<Camera>().orthographicSize * 2;
        dimensions.x = dimensions.y * 16f/9f; // 16/9 aspect ratio
        return dimensions;
    } }

    void Awake() {
        Instance = this;
        player = GameObject.FindWithTag("Player").GetComponent<PlayerScript>();
        fixedZ = transform.position.z;
        if(LevelData.Instance != null) {
            SetInitialPosition();
        }

        playerWindow = new Rect(-WINDOW_X_LIMIT, -3f, WINDOW_WIDTH, WINDOW_HEIGHT);

        focusPoints = new List<Vector2>();
        GameObject[] focusPointList = GameObject.FindGameObjectsWithTag("Focus Point");
        foreach(GameObject focusPoint in focusPointList) {
            focusPoints.Add(focusPoint.transform.position);
            Destroy(focusPoint);
        }
    }

    void FixedUpdate() {
        Vector3 startPosition = transform.position;
        Vector3 newPosition = transform.position;

        Vector2 followPoint = player.transform.position;

        // check if there is a nearby focus point
        Vector2? focus = null;
        float focusDist = 0f;
        foreach(Vector2 focusPoint in focusPoints) {
            focusDist = Vector2.Distance(player.transform.position, focusPoint);
            if(focusDist <= FOCUS_OUTER_RADIUS) {
                focus = focusPoint;
                break;
            }
        }
        
        if(focus.HasValue) {
            float ratio = FOCUS_MAX_RATIO;
            if(focusDist > FOCUS_INNER_RADIUS) {
                float multiplier = (FOCUS_OUTER_RADIUS - focusDist) / (FOCUS_OUTER_RADIUS - FOCUS_INNER_RADIUS);
                ratio *= multiplier;
            }
            followPoint = ratio * focus.Value + (1 - ratio) * (Vector2)player.transform.position; // focus on the average of the focus point and the player
        }

        Vector2 followRelativeToCenter = followPoint - (Vector2)transform.position;

        // manage horizontal
        float? movingX = null;
        if(followRelativeToCenter.x < playerWindow.xMin) {
            movingX = playerWindow.xMin;
        }
        else if(followRelativeToCenter.x > playerWindow.xMax) {
            movingX = playerWindow.xMax;
        }

        if(movingX.HasValue) {
            newPosition.x += (followRelativeToCenter.x - movingX.Value) * HORIZONTAL_MOVE_RATE * Time.timeScale;
        }

        // manage vertical
        if(player.CurrentState == PlayerScript.State.Grounded || player.HasWallJumped || followRelativeToCenter.y < playerWindow.yMin) {
            float? movingY = null;
            
            if(followRelativeToCenter.y < playerWindow.yMin) {
                movingY = playerWindow.yMin;
            }
            else if(followRelativeToCenter.y > playerWindow.yMax) {
                movingY = playerWindow.yMax;
            }

            if(movingY.HasValue) {
                newPosition.y += (followRelativeToCenter.y - movingY.Value) * VERTICAL_MOVE_RATE * Time.timeScale;
            }
        }

        // move the camera to the new position
        Vector2 cameraSize = Dimensions;
        LevelData level = LevelData.Instance;
        newPosition.x = Mathf.Clamp(newPosition.x, player.transform.position.x - WINDOW_X_LIMIT, player.transform.position.x + WINDOW_X_LIMIT); // keep player within view
        newPosition.y = Mathf.Clamp(newPosition.y, player.transform.position.y - WINDOW_Y_LIMIT, player.transform.position.y + WINDOW_Y_LIMIT);
        newPosition.x = Mathf.Clamp(newPosition.x, level.XMin + Dimensions.x / 2f, level.XMax - Dimensions.x / 2f); // do not look beyond the level bounds
        newPosition.y = Mathf.Clamp(newPosition.y, level.YMin + Dimensions.y / 2f, level.YMax - Dimensions.y / 2f);
        transform.position = newPosition;

        // move the camera window away from the direction the player is going
        Vector3 displacement = newPosition - startPosition;
        Vector2 windowCenter = playerWindow.center;
        float xTarget = -Mathf.Sign(displacement.x) * WINDOW_CENTER_X_LIMIT;
        float yTarget = -Mathf.Sign(displacement.y) * WINDOW_CENTER_Y_LIMIT;
        const float MIN_MULT = 0.3f;
        const float MAX_MULT = 0.8f;
        float xMultiplier = Mathf.Abs(xTarget - windowCenter.x) / (2f * WINDOW_CENTER_X_LIMIT) * (1.0f + MIN_MULT - MAX_MULT) + MIN_MULT;
        float yMultiplier = Mathf.Abs(yTarget - windowCenter.y) / (2f * WINDOW_CENTER_Y_LIMIT) * (1.0f + MIN_MULT - MAX_MULT) + MIN_MULT;
        windowCenter.x += -displacement.x * xMultiplier;
        windowCenter.y += -displacement.y * yMultiplier;
        windowCenter.x = Mathf.Clamp(windowCenter.x, -WINDOW_CENTER_X_LIMIT, WINDOW_CENTER_X_LIMIT);
        windowCenter.y = Mathf.Clamp(windowCenter.y, -WINDOW_CENTER_Y_LIMIT, WINDOW_CENTER_Y_LIMIT);
        playerWindow.center = windowCenter;

         if(visibleWindow != null) {
            // REMOVE FOR FINAL VERSION
            visibleWindow.transform.position = new Vector3(transform.position.x + playerWindow.center.x, transform.position.y + playerWindow.center.y, 0);
            visibleWindow.transform.localScale = new Vector3(playerWindow.width, playerWindow.height, 1f);
         }
    }

    // called by LevelData.cs Start() after generating the level bounds
    public void SetInitialPosition() {
        LevelData level = LevelData.Instance;
        Vector3 startingPos = player.transform.position - (Vector3)playerWindow.center + new Vector3(0, 0, fixedZ);
        startingPos.x = Mathf.Clamp(startingPos.x, level.XMin + Dimensions.x / 2f, level.XMax - Dimensions.x / 2f); // do not look beyond the level bounds
        startingPos.y = Mathf.Clamp(startingPos.y, level.YMin + Dimensions.y / 2f, level.YMax - Dimensions.y / 2f);
        transform.position = startingPos;
    }
}
