using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Tilemaps;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject visibleWindow;

    private const float WINDOW_WIDTH = 4f;
    private const float WINDOW_GROUND_HEIGHT = 5.0f;
    private const float WINDOW_AIR_HEIGHT = 1.0f;
    private const float WINDOW_X_LIMIT = 9f;
    private const float WINDOW_Y_LIMIT = 5f;

    private const float HORIZONTAL_MOVE_RATE = 0.1f;
    private const float VERTICAL_MOVE_RATE = 0.05f;

    private const float WINDOW_CENTER_X_LIMIT = WINDOW_X_LIMIT - WINDOW_WIDTH / 2f;
    //private const float WINDOW_CENTER_Y_LIMIT = WINDOW_Y_LIMIT - WINDOW_HEIGHT / 2f;

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

        playerWindow = new Rect(-WINDOW_X_LIMIT, -3f, WINDOW_WIDTH, WINDOW_GROUND_HEIGHT);

        focusPoints = new List<Vector2>();
        GameObject[] focusPointList = GameObject.FindGameObjectsWithTag("Focus Point");
        foreach(GameObject focusPoint in focusPointList) {
            focusPoints.Add(focusPoint.transform.position);
            Destroy(focusPoint);
        }
    }

    void FixedUpdate() {
        Vector2 cameraSize = Dimensions;
        LevelData level = LevelData.Instance;

        Vector3 startPosition = transform.position;
        Vector3 newPosition = transform.position;

        Vector2 followPoint = player.transform.position;

        Vector2 startCenter = playerWindow.center;
        playerWindow.height = player.CurrentState == PlayerScript.State.Grounded ? WINDOW_GROUND_HEIGHT : WINDOW_AIR_HEIGHT;
        playerWindow.center = startCenter;
        float windowCenterYLimit = WINDOW_Y_LIMIT - playerWindow.height / 2f;

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
            float change = (followRelativeToCenter.x - movingX.Value) * HORIZONTAL_MOVE_RATE * Time.timeScale;
            if(Mathf.Abs(change) < 0.01f) {
                change = 0;
            }
            newPosition.x += change;
        }

        // manage vertical
        bool belowWindow = followRelativeToCenter.y < playerWindow.yMin && player.Velocity.y <= 0f;
        if(player.CurrentState == PlayerScript.State.Grounded || player.HasWallSlid) {
            float? movingY = null;
            
            if(followRelativeToCenter.y < playerWindow.yMin) {
                movingY = playerWindow.yMin;
            }
            else if(followRelativeToCenter.y > playerWindow.yMax) {
                movingY = playerWindow.yMax;
            }

            if(movingY.HasValue) {
                float moveRate = belowWindow ? 3f * VERTICAL_MOVE_RATE : VERTICAL_MOVE_RATE;
                float change = (followRelativeToCenter.y - movingY.Value) * moveRate * Time.timeScale;
                if(Mathf.Abs(change) < 0.01f) {
                    change = 0;
                }
                newPosition.y += change;
            }
        }

        // prevent the camera from looking through the ground
        List<float> landBlocks = FindLandBlocks(newPosition);
        float? bottomBlock = null;
        float? topBlock = null;
        foreach(float blockHeight in landBlocks) {
            if(blockHeight > followPoint.y && (!topBlock.HasValue || blockHeight < topBlock.Value)) {
                topBlock = blockHeight;
            }
            else if(blockHeight < followPoint.y && (!bottomBlock.HasValue || blockHeight > bottomBlock.Value)) {
                bottomBlock = blockHeight;
            }
        }

        float targetY = newPosition.y;
        if(topBlock.HasValue && bottomBlock.HasValue && topBlock.Value - bottomBlock.Value < cameraSize.y) {
            targetY = (topBlock.Value + bottomBlock.Value) / 2f;
        } else {
            float bottomTarget = bottomBlock.HasValue ? bottomBlock.Value + cameraSize.y / 2f - 3f : float.MinValue;
            float topTarget = topBlock.HasValue ? topBlock.Value - cameraSize.y / 2f + 3f : float.MaxValue;
            targetY = Mathf.Clamp(newPosition.y, bottomTarget, topTarget);
        }

        if(newPosition.y != targetY) {
            if(Mathf.Sign(targetY - newPosition.y) == -Mathf.Sign(newPosition.y - startPosition.y)) {
                newPosition.y = targetY;
            } else {
                newPosition.y += (targetY - newPosition.y) * 0.08f * Time.timeScale;
            }
        }

        // move the camera to the new position
        newPosition.x = Mathf.Clamp(newPosition.x, player.transform.position.x - WINDOW_X_LIMIT, player.transform.position.x + WINDOW_X_LIMIT); // keep player within view
        newPosition.y = Mathf.Clamp(newPosition.y, player.transform.position.y - WINDOW_Y_LIMIT, player.transform.position.y + WINDOW_Y_LIMIT);
        newPosition.x = Mathf.Clamp(newPosition.x, level.XMin + Dimensions.x / 2f, level.XMax - Dimensions.x / 2f); // do not look beyond the level bounds
        newPosition.y = Mathf.Clamp(newPosition.y, level.YMin + Dimensions.y / 2f, level.YMax - Dimensions.y / 2f);
        transform.position = newPosition;

        // move the camera window away from the direction the player is going
        Vector3 displacement = newPosition - startPosition;
        Vector2 windowCenter = playerWindow.center;
        float xTarget = -Mathf.Sign(displacement.x) * WINDOW_CENTER_X_LIMIT;
        float yTarget = -Mathf.Sign(displacement.y) * windowCenterYLimit;
        const float MIN_MULT = 0.3f;
        const float MAX_MULT = 0.8f;
        float xMultiplier = Mathf.Abs(xTarget - windowCenter.x) / (2f * WINDOW_CENTER_X_LIMIT) * (1.0f + MIN_MULT - MAX_MULT) + MIN_MULT;
        float yMultiplier = Mathf.Abs(yTarget - windowCenter.y) / (2f * windowCenterYLimit) * (1.0f + MIN_MULT - MAX_MULT) + MIN_MULT;
        if(belowWindow) {
            yMultiplier += 0.2f;
        }
        windowCenter.x += -displacement.x * xMultiplier;
        windowCenter.y += -displacement.y * yMultiplier;
        windowCenter.x = Mathf.Clamp(windowCenter.x, -WINDOW_CENTER_X_LIMIT, WINDOW_CENTER_X_LIMIT);
        windowCenter.y = Mathf.Clamp(windowCenter.y, -windowCenterYLimit, windowCenterYLimit);
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

    private List<float> FindLandBlocks(Vector2 middle) {
        List<float> result = new List<float>();
        Vector2 dimensions = Dimensions;
        Tilemap walls = TilemapScript.Instance.WallGrid;

        float leftEdge = middle.x - dimensions.x / 2f;
        float topEdge = middle.y + dimensions.y / 2f;
        float bottomEdge = middle.y - dimensions.y / 2f;
        float rightEdge = middle.x + dimensions.x / 2f;
        Vector3Int topLeft = walls.WorldToCell(new Vector3(leftEdge, topEdge, 0));
        Vector3Int bottomRight = walls.WorldToCell(new Vector3(rightEdge, bottomEdge, 0));

        for(int y = bottomRight.y; y <= topLeft.y; y++) {
            // check if every tile on this row is a wall
            bool fullRow = true;
            for(int x = topLeft.x; x <= bottomRight.x; x++) {
                if(walls.GetTile(new Vector3Int(x, y, 0)) == null) {
                    fullRow = false;
                    break;
                }
            }

            if(fullRow) {
                result.Add(walls.GetCellCenterWorld(new Vector3Int(topLeft.x, y, 0)).y);
            }
        }

        return result;
    }
}
