using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject visibleWindow;

    private const float WINDOW_WIDTH = 4f;
    private const float WINDOW_HEIGHT = 3f;
    private const float AERIAL_WINDOW_EXTENSION = 2f;
    private const float WINDOW_X_LIMIT = 8f;
    private const float WINDOW_Y_LIMIT = 5f;
    private const float WINDOW_X_SHIFT_RATE = 0.7f;
    private const float WINDOW_Y_SHIFT_RATE = 0.3f;
    private const float AERIAL_WINDOW_Y_SHIFT_RATE = 0.8f;
    private const float HORIZONTAL_MOVE_RATE = 0.1f;
    private const float VERTICAL_MOVE_RATE = 0.1f;

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
        if(visibleWindow != null) {
            // REMOVE FOR FINAL VERSION
            visibleWindow.transform.position = transform.position + (Vector3)playerWindow.center + new Vector3(0, 0, -fixedZ);
            visibleWindow.transform.localScale = new Vector3(playerWindow.width, playerWindow.height, 1f);
        }

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
        if(playerRelativeToCenter.y < playerWindow.yMin - aerialExtension) {
            movingY = playerWindow.yMin - aerialExtension;
        }
        else if(playerRelativeToCenter.y > playerWindow.yMax + aerialExtension) {
            movingY = playerWindow.yMax + aerialExtension;
        }

        if(movingY.HasValue) {
            newPosition.y += (playerRelativeToCenter.y - movingY.Value) * VERTICAL_MOVE_RATE * Time.timeScale;
        }

        // move the camera window away from the direction the player is going
        Vector3 displacement = newPosition - transform.position;
        Vector2 windowCenter = playerWindow.center;
        windowCenter.x += -displacement.x * WINDOW_X_SHIFT_RATE;
        windowCenter.y += -displacement.y * (player.CurrentState == PlayerScript.State.Aerial ? AERIAL_WINDOW_Y_SHIFT_RATE : WINDOW_Y_SHIFT_RATE);
        windowCenter.x = Mathf.Clamp(windowCenter.x, -WINDOW_CENTER_X_LIMIT, WINDOW_CENTER_X_LIMIT);
        windowCenter.y = Mathf.Clamp(windowCenter.y, -WINDOW_CENTER_Y_LIMIT, WINDOW_CENTER_Y_LIMIT);
        playerWindow.center = windowCenter;

        transform.position = newPosition;
    }

    // called by LevelData.cs Start() after generating the level bounds
    public void SetInitialPosition() {
        transform.position = player.transform.position - (Vector3)playerWindow.center + new Vector3(0, 0, fixedZ);
    }
}
