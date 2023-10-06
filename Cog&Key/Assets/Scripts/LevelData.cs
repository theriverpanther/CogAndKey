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
    private float xMin;
    private float xMax;

    public List<LevelBoundScript> LevelAreas { get { return levelAreas; } }
    public Vector2? RespawnPoint { get { return (currentCheckpoint == null ? null : currentCheckpoint.transform.position); } }
    public float XMin { get { return xMin; } }
    public float XMax { get { return xMax; } }

    public List<KeyState> StartingKeys;

    void Awake() {
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

        xMin = float.MaxValue;
        xMax = float.MinValue;
        foreach(GameObject bound in bounds) {
            LevelBoundScript boundScript = bound.GetComponent<LevelBoundScript>();
            boundScript.Area = new Rect(bound.transform.position - bound.transform.lossyScale / 2, bound.transform.lossyScale);
            levelAreas.Add(boundScript);
            xMin = Mathf.Min(xMin, boundScript.Area.xMin);
            xMax = Mathf.Max(xMax, boundScript.Area.xMax);
            bound.GetComponent<SpriteRenderer>().enabled = false;
            DontDestroyOnLoad(bound);
        }

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
                Debug.Log(keyScript.Type);
                keyScript.Equip();
            }
        }
    }
}
