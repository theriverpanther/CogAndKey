using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;

public class TitlescreenUI : MonoBehaviour
{
    // First scene must be the starting scene shown to player
    public List<GameObject> screens;
    private GameObject currentlyEnabled;
    // Start is called before the first frame update
    void Start()
    {
        currentlyEnabled = screens[0];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Used on button press but also a simple switch.
    /// </summary>
    /// <param name="screenName">The name of the screen</param>
    /// <param name="showScreen"> Defaults to true, determines to show popup screen or just set internally</param>
    public void SwitchScreen(string screenName, bool showScreen = true)
    {
        try
        {
            foreach(GameObject go in screens) { 
                if(go.name.ToLower() == screenName.ToLower())
                {
                    // Disable the gameobject
                    currentlyEnabled.SetActive(false);

                    // Change the enanbled to the switch and set active
                    currentlyEnabled = go;
                    currentlyEnabled.SetActive(showScreen); 
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("Screen does not exist.");
        }
    }

    /// <summary>
    /// Toggle the current screen to hide itself.
    /// </summary>
    /// <param name="screenName"></param>
    public void ToggleScreen(string screenName) {
        try
        {
                foreach (GameObject go in screens)
                {
                    if (go.name.ToLower() == screenName.ToLower())
                    {
                        // Toggle screen to opposite state
                        currentlyEnabled.SetActive(!currentlyEnabled.activeSelf);
                    }
                }

        }
        catch (Exception e)
        {
            Debug.Log("Screen does not exist.");
        }
    }

    /// <summary>
    /// Changes the scenes in Unity
    /// </summary>
    /// <param name="sceneName">Name of scene with capitalization needed</param>
    public void SetScene(string sceneName)
    {
        try
        {
            SceneManager.LoadScene(sceneName);
        } catch(Exception e)
        {
            Debug.Log("Scene does not exist in current format; Check your capitalization.");
        }
    }
}
