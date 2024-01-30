using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject visibleWindow;

    private const float WINDOW_WIDTH = 4f;
    private const float WINDOW_HEIGHT = 2f;
    private const float AERIAL_WINDOW_EXTENSION = 3.5f;
    private const float WINDOW_X_LIMIT = 8f;
    private const float WINDOW_Y_LIMIT = 5f;
    private const float WINDOW_X_SHIFT_RATE = 0.7f;
    private const float WINDOW_Y_SHIFT_RATE = 0.3f;
    private const float AERIAL_WINDOW_Y_SHIFT_RATE = 0.8f;
    private const float HORIZONTAL_MOVE_RATE = 0.1f;
    private const float VERTICAL_MOVE_RATE = 0.1f;
    private const float AERIAL_VERTICAL_MOVE_RATE = 0.15f;

    private const float WINDOW_CENTER_X_LIMIT = WINDOW_X_LIMIT - WINDOW_WIDTH / 2f;
    private const float WINDOW_CENTER_Y_LIMIT = WINDOW_Y_LIMIT - WINDOW_HEIGHT / 2f;

    private PlayerScript player;
    private float fixedZ;
    private Rect playerWindow;

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
    }

    void FixedUpdate() {
        Vector3 newPosition = transform.position;
        Vector3 playerRelativeToCenter = player.transform.position - transform.position;

        // manage horizontal
        float? movingX = null;
        if(playerRelativeToCenter.x < playerWindow.xMin) {
            movingX = playerWindow.xMin;
        }
        else if(playerRelativeToCenter.x > playerWindow.xMax) {
            movingX = playerWindow.xMax;
        }

        if(movingX.HasValue) {
            newPosition.x += (playerRelativeToCenter.x - movingX.Value) * HORIZONTAL_MOVE_RATE * Time.timeScale;
        }

        // manage vertical
        float? movingY = null;
        float aerialExtension = player.CurrentState == PlayerScript.State.Aerial ? AERIAL_WINDOW_EXTENSION : 0f;
        float minBound = Mathf.Max(playerWindow.yMin - aerialExtension, -WINDOW_Y_LIMIT);
        float maxBound = Mathf.Min(playerWindow.yMax + aerialExtension, WINDOW_Y_LIMIT);
        if(playerRelativeToCenter.y < minBound) {
            movingY = minBound;
        }
        else if(playerRelativeToCenter.y > maxBound) {
            movingY = maxBound;
        }

        if(movingY.HasValue) {
            newPosition.y += (playerRelativeToCenter.y - movingY.Value) * (player.CurrentState == PlayerScript.State.Aerial ? AERIAL_VERTICAL_MOVE_RATE : VERTICAL_MOVE_RATE) * Time.timeScale;
        }

        // move the camera window away from the direction the player is going
        Vector3 displacement = newPosition - transform.position;
        Vector2 windowCenter = playerWindow.center;
        windowCenter.x += -displacement.x * WINDOW_X_SHIFT_RATE;
        windowCenter.y += -displacement.y * (player.CurrentState == PlayerScript.State.Aerial ? AERIAL_WINDOW_Y_SHIFT_RATE : WINDOW_Y_SHIFT_RATE);
        windowCenter.x = Mathf.Clamp(windowCenter.x, -WINDOW_CENTER_X_LIMIT, WINDOW_CENTER_X_LIMIT);
        windowCenter.y = Mathf.Clamp(windowCenter.y, -WINDOW_CENTER_Y_LIMIT, WINDOW_CENTER_Y_LIMIT);
        playerWindow.center = windowCenter;

        Vector2 cameraSize = Dimensions;
        LevelData level = LevelData.Instance;
        newPosition.x = Mathf.Clamp(newPosition.x, level.XMin + Dimensions.x / 2f, level.XMax - Dimensions.x / 2f);
        newPosition.y = Mathf.Clamp(newPosition.y, level.YMin + Dimensions.y / 2f, level.YMax - Dimensions.y / 2f);
        transform.position = newPosition;

         if(visibleWindow != null) {
            // REMOVE FOR FINAL VERSION
            //visibleWindow.transform.position = transform.position + (Vector3)playerWindow.center + new Vector3(0, 0, -fixedZ);
            //visibleWindow.transform.localScale = new Vector3(playerWindow.width, playerWindow.height, 1f);
            visibleWindow.transform.position = new Vector3(transform.position.x + playerWindow.center.x, transform.position.y + (maxBound + minBound) / 2f, 0);
            visibleWindow.transform.localScale = new Vector3(playerWindow.width, maxBound - minBound, 1f);
        }
    }

    // called by LevelData.cs Start() after generating the level bounds
    public void SetInitialPosition() {
        transform.position = player.transform.position - (Vector3)playerWindow.center + new Vector3(0, 0, fixedZ);
    }
}
