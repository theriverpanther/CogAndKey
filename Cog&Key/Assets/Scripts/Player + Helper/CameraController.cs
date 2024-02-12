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
    private const float WINDOW_X_SHIFT_RATE = 0.5f;
    private const float WINDOW_Y_SHIFT_RATE = 0.5f;
    
    private const float HORIZONTAL_MOVE_RATE = 0.1f;
    private const float VERTICAL_MOVE_RATE = 0.05f;

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
        Vector3 startPosition = transform.position;
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
        if(player.CurrentState == PlayerScript.State.Grounded || player.HasWallJumped || playerRelativeToCenter.y < playerWindow.yMin) {
            float? movingY = null;
            
            if(playerRelativeToCenter.y < playerWindow.yMin) {
                movingY = playerWindow.yMin;
            }
            else if(playerRelativeToCenter.y > playerWindow.yMax) {
                movingY = playerWindow.yMax;
            }

            if(movingY.HasValue) {
                newPosition.y += (playerRelativeToCenter.y - movingY.Value) * VERTICAL_MOVE_RATE * Time.timeScale;
            }
        }

        // scroll upward when wall jumping up, but not aerial otherwise

        // scroll downward when falling downward

        // move the camera to the new position
        Vector2 cameraSize = Dimensions;
        LevelData level = LevelData.Instance;
        //newPosition.x = Mathf.Clamp(newPosition.x, level.XMin + Dimensions.x / 2f, level.XMax - Dimensions.x / 2f);
        //newPosition.y = Mathf.Clamp(newPosition.y, level.YMin + Dimensions.y / 2f, level.YMax - Dimensions.y / 2f);
        transform.position = newPosition;

        // move the camera window away from the direction the player is going
        Vector3 displacement = newPosition - startPosition;
        Vector2 windowCenter = playerWindow.center;
        windowCenter.x += -displacement.x * WINDOW_X_SHIFT_RATE;
        windowCenter.y += -displacement.y * WINDOW_Y_SHIFT_RATE;
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
        transform.position = player.transform.position - (Vector3)playerWindow.center + new Vector3(0, 0, fixedZ);
    }
}
