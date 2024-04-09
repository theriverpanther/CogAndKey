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
    private Dictionary<KeyState, bool> checkpointKeys; // saves keys that are claimed before a checkpoint

    public Vector2? RespawnPoint { get { return (currentCheckpoint == null ? null : currentCheckpoint.transform.position); } }
    public float XMin { get { return levelBounds.xMin; } }
    public float XMax { get { return levelBounds.xMax; } }
    public float YMin { get { return levelBounds.yMin; } }
    public float YMax { get { return levelBounds.yMax; } }
    public int DeathsSinceCheckpoint { get; private set; }

    // needs CameraController Awake() to run first
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
            Destroy(gameObject);
            return;
        }

        // set up the single instance
        Instance = this;
        levelName = SceneManager.GetActiveScene().name;
        transform.SetParent(null);
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
            if(PlayerScript.CurrentPlayer.EquippedKeys[keyType]) {
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
}
