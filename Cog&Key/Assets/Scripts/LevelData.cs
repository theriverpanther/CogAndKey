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
    private List<Rect> levelAreas = new List<Rect>();

    public List<Rect> LevelAreas { get { return levelAreas; } }
    public Vector2? RespawnPoint { get { return (currentCheckpoint == null ? null : currentCheckpoint.transform.position); } }

    void Awake() {
        // store the level's boundaries and checkpoints
        GameObject[] bounds = GameObject.FindGameObjectsWithTag("LevelBound");
        foreach(GameObject bound in bounds) {
            levelAreas.Add(new Rect(bound.transform.position - bound.transform.localScale / 2, bound.transform.localScale));
            Destroy(bound);
        }

       checkpoints = new List<GameObject>(GameObject.FindGameObjectsWithTag("Checkpoint"));

        // delete duplicates
        if(instance != null) {
            foreach(GameObject checkpoint in checkpoints) {
                if(!instance.checkpoints.Contains(checkpoint)) {
                    Destroy(checkpoint);
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

        foreach(GameObject checkpoint in checkpoints) {
            checkpoint.transform.SetParent(null, true);
            DontDestroyOnLoad(checkpoint);
        }
    }

    // called when the scene changes, deletes the instance if it is no longer the correct level
    private void CheckNextLevel(Scene current, Scene next) {
        if(next.name != levelName) {
            foreach(GameObject checkpoint in checkpoints) {
                Destroy(checkpoint);
            }

            Destroy(gameObject);
            instance = null;
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
    }
}
