using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// one of these scripts should exist per level. Does not delete when the level is reloaded, but deletes when going to a new level
public class LevelData : MonoBehaviour
{
    private static LevelData instance;
    public static LevelData Instance { get { return instance; } }

    private string levelName;
    private CheckpointScript currentCheckpoint;
    private List<GameObject> checkpoints;
    private List<LevelBoundScript> levelAreas = new List<LevelBoundScript>();
    private List<Rect> cameraZones;

    public List<LevelBoundScript> LevelAreas { get { return levelAreas; } }
    public List<Rect> CameraZones { get { return cameraZones; } }
    public Vector2? RespawnPoint { get { return (currentCheckpoint == null ? null : currentCheckpoint.transform.position); } }
    public float XMin { get; private set; }
    public float XMax { get; private set; }
    public float YMin { get; private set; }

    public List<KeyState> StartingKeys;

    // needs CameraScript Awake() to run first
    void Start() {
        checkpoints = new List<GameObject>(GameObject.FindGameObjectsWithTag("Checkpoint"));
        GameObject[] bounds = GameObject.FindGameObjectsWithTag("LevelBound");

        // delete duplicates
        if(instance != null) {
            foreach(GameObject checkpoint in checkpoints) {
                if(!instance.checkpoints.Contains(checkpoint)) {
                    Destroy(checkpoint);
                }
            }

            foreach(GameObject bound in bounds) {
                if(!instance.levelAreas.Contains(bound.GetComponent<LevelBoundScript>())) {
                    Destroy(bound);
                }
            }

            Destroy(gameObject);
            return;
        }

        // set up the single instance
        instance = this;
        levelName = SceneManager.GetActiveScene().name;
        DontDestroyOnLoad(gameObject);
        SceneManager.activeSceneChanged += CheckNextLevel;

        // store the level's boundaries and checkpoints
        foreach(GameObject checkpoint in checkpoints) {
            checkpoint.transform.SetParent(null, true);
            DontDestroyOnLoad(checkpoint);
        }

        XMin = float.MaxValue;
        XMax = float.MinValue;
        YMin = float.MaxValue;
        foreach(GameObject bound in bounds) {
            LevelBoundScript boundScript = bound.GetComponent<LevelBoundScript>();
            boundScript.Area = new Rect(bound.transform.position - bound.transform.lossyScale / 2, bound.transform.lossyScale);
            levelAreas.Add(boundScript);
            XMin = Mathf.Min(XMin, boundScript.Area.xMin);
            XMax = Mathf.Max(XMax, boundScript.Area.xMax);
            YMin = Mathf.Min(YMin, boundScript.Area.yMin);
            bound.GetComponent<SpriteRenderer>().enabled = false;
            DontDestroyOnLoad(bound);
        }

        GenerateCameraZones();
        CameraScript.Instance.SetInitialPosition();

        // equip the player with the starting keys
        EquipStartKeys();
    }

    // called when the scene changes, deletes the instance if it is no longer the correct level
    private void CheckNextLevel(Scene current, Scene next) {
        if(next.name != levelName) {
            foreach(GameObject checkpoint in checkpoints) {
                Destroy(checkpoint);
            }

            foreach(LevelBoundScript bound in levelAreas) {
                Destroy(bound.gameObject);
            }

            SceneManager.activeSceneChanged -= CheckNextLevel;
            Destroy(gameObject);
            instance = null;
        } else {
            // level restarted
            EquipStartKeys();
        }
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

        // save keys acquired since the last checkpoint
        PlayerScript player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
        if(!StartingKeys.Contains(KeyState.Fast) && player.FastKey != null) {
            StartingKeys.Add(KeyState.Fast);
        }
        if(!StartingKeys.Contains(KeyState.Lock) && player.LockKey != null) {
            StartingKeys.Add(KeyState.Lock);
        }
        if(!StartingKeys.Contains(KeyState.Reverse) && player.ReverseKey != null) {
            StartingKeys.Add(KeyState.Reverse);
        }
    }

    private void EquipStartKeys() {
        GameObject[] keys = GameObject.FindGameObjectsWithTag("Key");
        foreach(GameObject key in keys) {
            KeyScript keyScript = key.GetComponent<KeyScript>();
            if(StartingKeys.Contains(keyScript.Type)) {
                keyScript.Equip();
            }
        }
    }

    // uses the level bounds to determine where the camera is allowed to be centered
    private void GenerateCameraZones() {
        cameraZones = new List<Rect>();
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
            Rect bufferedArea = levelAreas[i].Area.MakeExpanded(0.2f);
            for(int j = i + 1; j < levelAreas.Count; j++) {
                if(bufferedArea.Overlaps(levelAreas[j].Area)) {
                    // add a camera zone connecting these zones together
                    if(cameraZones[i].yMax > cameraZones[j].yMin && cameraZones[i].yMin < cameraZones[j].yMax) {
                        // horizontally adjacent
                        float yMax = Mathf.Min(cameraZones[i].yMax, cameraZones[j].yMax);
                        float yMin = Mathf.Max(cameraZones[i].yMin, cameraZones[j].yMin);
                        float xMax = Mathf.Max(cameraZones[i].xMin, cameraZones[j].xMin);
                        float xMin = Mathf.Min(cameraZones[i].xMax, cameraZones[j].xMax);
                        addedZones.Add(new Rect(xMin, yMin, xMax - xMin, yMax - yMin));
                        Debug.Log("added hori zone");
                    }
                    else if(cameraZones[i].xMax > cameraZones[j].xMin && cameraZones[i].xMin < cameraZones[j].xMax) {
                        // vertically adjacent
                        float xMax = Mathf.Min(cameraZones[i].xMax, cameraZones[j].xMax);
                        float xMin = Mathf.Max(cameraZones[i].xMin, cameraZones[j].xMin);
                        float yMax = Mathf.Max(cameraZones[i].yMin, cameraZones[j].yMin);
                        float yMin = Mathf.Min(cameraZones[i].yMax, cameraZones[j].yMax);
                        addedZones.Add(new Rect(xMin, yMin, xMax - xMin, yMax - yMin));
                        Debug.Log("added vert zone");
                    }
                }
            }
        }

        cameraZones.AddRange(addedZones);
    }
}
