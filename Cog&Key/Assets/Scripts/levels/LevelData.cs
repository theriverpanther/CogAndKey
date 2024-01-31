using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// one of these scripts should exist per level. Does not delete when the level is reloaded, but deletes when going to a new level
public class LevelData : MonoBehaviour
{
    public static LevelData Instance { get; private set; }
    [SerializeField] private GameObject LevelBoundary;
    private Rect levelBounds;

    private string levelName;
    private CheckpointScript currentCheckpoint;
    private List<GameObject> checkpoints;
    [SerializeField] private List<LevelBoundScript> levelAreas = new List<LevelBoundScript>();
    private Dictionary<KeyState, bool> checkpointKeys; // saves keys that are claimed before a checkpoint
    private List<Rect> cameraZones;

    public List<LevelBoundScript> LevelAreas { get { return levelAreas; } }
    public List<Rect> CameraZones { get { return cameraZones; } }
    public Vector2? RespawnPoint { get { return (currentCheckpoint == null ? null : currentCheckpoint.transform.position); } }
    public float XMin { get { return levelBounds.xMin; } }
    public float XMax { get { return levelBounds.xMax; } }
    public float YMin { get { return levelBounds.yMin; } }
    public float YMax { get { return levelBounds.yMax; } }
    //public float XMin { get { return float.MinValue; } }
    //public float XMax { get { return float.MaxValue; } }
    //public float YMin { get { return -100; } }
    //public float YMax { get { return float.MaxValue; } }
    public int DeathsSinceCheckpoint { get; private set; }

    // needs CameraScript Awake() to run first
    void Start() {
        Physics2D.queriesHitTriggers = false; // prevent raycasts from hitting triggers

        checkpoints = new List<GameObject>(GameObject.FindGameObjectsWithTag("Checkpoint"));
        GameObject[] bounds = GameObject.FindGameObjectsWithTag("LevelBound");

        // delete duplicates
        if(Instance != null) {
            foreach(GameObject checkpoint in checkpoints) {
                if(!Instance.checkpoints.Contains(checkpoint)) {
                    Destroy(checkpoint);
                    
                }
            }

            foreach(GameObject bound in bounds) {
                if(!Instance.levelAreas.Contains(bound.GetComponent<LevelBoundScript>())) {
                    Destroy(bound);
                }
            }
            Destroy(gameObject);
            return;
        }

        // set up the single instance
        Instance = this;
        levelName = SceneManager.GetActiveScene().name;
        DontDestroyOnLoad(gameObject);
        SceneManager.activeSceneChanged += CheckNextLevel;

        // store the level's boundaries and checkpoints
        Vector2 boundMid = LevelBoundary.transform.position;
        Vector2 boundDims = LevelBoundary.transform.lossyScale;
        levelBounds = new Rect(boundMid - boundDims / 2f, boundDims);
        LevelBoundary.SetActive(false);

        foreach(GameObject checkpoint in checkpoints) {
            checkpoint.transform.SetParent(null, true);
            DontDestroyOnLoad(checkpoint);
        }

        //XMin = float.MaxValue;
        //XMax = float.MinValue;
        //YMin = float.MaxValue;
        foreach(GameObject bound in bounds) {
            LevelBoundScript boundScript = bound.GetComponent<LevelBoundScript>();
            if(boundScript != null) {
                boundScript.Area = new Rect(bound.transform.position - bound.transform.lossyScale / 2, bound.transform.lossyScale);
                levelAreas.Add(boundScript);
                //XMin = Mathf.Min(XMin, boundScript.Area.xMin);
                //XMax = Mathf.Max(XMax, boundScript.Area.xMax);
                //YMin = Mathf.Min(YMin, boundScript.Area.yMin);
                bound.GetComponent<SpriteRenderer>().enabled = false;
                bound.transform.SetParent(null, true);
                DontDestroyOnLoad(bound);
            }
        }

        GenerateCameraZones();
        CameraScript.Instance?.SetInitialPosition();
        CameraController.Instance?.SetInitialPosition();

        // equip the player with the starting keys
        checkpointKeys = new Dictionary<KeyState, bool>();
        checkpointKeys[KeyState.Fast] = false;
        checkpointKeys[KeyState.Lock] = false;
        checkpointKeys[KeyState.Reverse] = false;

        EquipCheckpointKeys();
    }

    // called when the scene changes, deletes the instance if it is no longer the correct level
    private void CheckNextLevel(Scene current, Scene next) {
        if(next.name != levelName) {
            SceneManager.activeSceneChanged -= CheckNextLevel;
            SceneManager.sceneLoaded += NewLevelLoaded;
        } else {
            // level restarted
            DeathsSinceCheckpoint++;
            EquipCheckpointKeys();
        }
    }

    private void NewLevelLoaded(Scene scene, LoadSceneMode mode) {
        foreach (GameObject checkpoint in checkpoints)
        {
            Destroy(checkpoint);
        }
        checkpoints.Clear();

        foreach (LevelBoundScript bound in levelAreas)
        {
            // Unity warns against this but needs to be done or it won't be cleaned before Start is called
            DestroyImmediate(bound.gameObject);
        }
        levelAreas.Clear();

        Destroy(gameObject);
        Instance = null;
        SceneManager.sceneLoaded -= NewLevelLoaded;
    }

    // called by checkpoint objects when they are triggered
    public void TriggerCheckpoint(CheckpointScript checkpoint) {
        if(currentCheckpoint == checkpoint) {
            return;
        }

        if(currentCheckpoint != null) {
            currentCheckpoint.SetAsCheckpoint(false);
        }

        currentCheckpoint = checkpoint;
        currentCheckpoint.SetAsCheckpoint(true);
        DeathsSinceCheckpoint = 0;

        // save keys acquired since the last checkpoint
        foreach(KeyState keyType in new KeyState[3] { KeyState.Fast, KeyState.Lock, KeyState.Reverse }) {
            if(PlayerInput.Instance.EquippedKeys[keyType]) {
                checkpointKeys[keyType] = true;
            }
        }
    }

    private void EquipCheckpointKeys() {
        GameObject[] keys = GameObject.FindGameObjectsWithTag("Key");
        foreach(GameObject key in keys) {
            KeyScript keyScript = key.GetComponent<KeyScript>();
            if(checkpointKeys[keyScript.Type]) {
                keyScript.Equip();
            }
        }
    }

    // uses the level bounds to determine where the camera is allowed to be centered
    private void GenerateCameraZones() {
        cameraZones = new List<Rect>();
        if(CameraScript.Instance == null) {
            return;
        }
        Vector2 cameraDims = CameraScript.Instance.Dimensions;

        // add areas in the middle of each level boundary
        foreach(LevelBoundScript levelBound in levelAreas) {
            Rect middleZone = new Rect(levelBound.Area.xMin + cameraDims.x/2, levelBound.Area.yMin + cameraDims.y/2, levelBound.Area.width - cameraDims.x, levelBound.Area.height - cameraDims.y);
            if(middleZone.yMin > middleZone.yMax) {
                
                middleZone = new Rect(middleZone.x, (middleZone.yMax + middleZone.yMin) / 2, middleZone.width, 0);
            }
            if(middleZone.xMin > middleZone.xMax) {
                middleZone = new Rect((middleZone.xMax + middleZone.xMin) / 2, middleZone.y, 0, middleZone.height);
            }
            cameraZones.Add(middleZone);
        }

        // add areas connecting adjacent boundaries together
        List<Rect> addedZones = new List<Rect>();
        for(int i = 0; i < levelAreas.Count; i++) {
            // find which level bounds are adjacent to this one
            Rect bufferedArea = levelAreas[i].Area.MakeExpanded(0.5f);
            for(int j = i + 1; j < levelAreas.Count; j++) {
                if(bufferedArea.Overlaps(levelAreas[j].Area) && !AreZonesOppositeDirection(levelAreas[i], levelAreas[j])) {
                    // add a camera zone connecting these zones together
                    const float BUFFER = 0.1f;
                    if(cameraZones[i].yMax + BUFFER > cameraZones[j].yMin - BUFFER && cameraZones[i].yMin - BUFFER < cameraZones[j].yMax + BUFFER) {
                        // horizontally adjacent
                        float yMax = Mathf.Min(cameraZones[i].yMax, cameraZones[j].yMax);
                        float yMin = Mathf.Max(cameraZones[i].yMin, cameraZones[j].yMin);
                        float xMax = Mathf.Max(cameraZones[i].xMin, cameraZones[j].xMin);
                        float xMin = Mathf.Min(cameraZones[i].xMax, cameraZones[j].xMax);
                        addedZones.Add(new Rect(xMin, yMin, xMax - xMin, yMax - yMin));
                    }
                    else if(cameraZones[i].xMax > cameraZones[j].xMin && cameraZones[i].xMin < cameraZones[j].xMax) {
                        // vertically adjacent
                        float xMax = Mathf.Min(cameraZones[i].xMax, cameraZones[j].xMax);
                        float xMin = Mathf.Max(cameraZones[i].xMin, cameraZones[j].xMin);
                        float yMax = Mathf.Max(cameraZones[i].yMin, cameraZones[j].yMin);
                        float yMin = Mathf.Min(cameraZones[i].yMax, cameraZones[j].yMax);
                        addedZones.Add(new Rect(xMin, yMin, xMax - xMin, yMax - yMin));
                    }
                }
            }
        }

        cameraZones.AddRange(addedZones);
    }

    // Note to future self: delete this awful function
    private bool AreZonesOppositeDirection(LevelBoundScript one, LevelBoundScript other)
    {
        bool hasUp = one.AreaType == CamerBoundType.Up || other.AreaType == CamerBoundType.Up;
        bool hasDown = one.AreaType == CamerBoundType.Down || other.AreaType == CamerBoundType.Down;
        bool hasLeft = one.AreaType == CamerBoundType.Left || other.AreaType == CamerBoundType.Left;
        bool hasRight = one.AreaType == CamerBoundType.Right || other.AreaType == CamerBoundType.Right;
        return hasUp && hasDown || hasLeft && hasRight;
    }
}
