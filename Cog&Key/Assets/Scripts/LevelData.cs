using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// one of these scripts should exist per level. Does not delete when the level is reloaded, but deletes when going to a new level
public class LevelData : MonoBehaviour
{
    private Scene level;
    private static LevelData instance;
    public static LevelData Instance { get { return instance; } }

    private List<Rect> levelAreas = new List<Rect>();
    public List<Rect> LevelAreas { get { return levelAreas; } }

    void Awake() {
        // store the level's boundaries, must delete bounds every time the level is loaded
        GameObject[] bounds = GameObject.FindGameObjectsWithTag("LevelBound");
        foreach(GameObject bound in bounds) {
            levelAreas.Add(new Rect(bound.transform.position - bound.transform.localScale / 2, bound.transform.localScale));
            Destroy(bound);
        }

        // delete duplicates
        if(instance != null) {
            Destroy(gameObject);
            return;
        }

        // set up the single instance
        instance = this;
        level = SceneManager.GetActiveScene();
        DontDestroyOnLoad(gameObject);
        SceneManager.activeSceneChanged += CheckNextLevel;

        
    }

    // called when the scene changes, deletes the instance if it is no longer the correct level
    private void CheckNextLevel(Scene current, Scene next)
    {
        if(next != level)
        {
            Destroy(gameObject);
            instance = null;
        }
    }
}
