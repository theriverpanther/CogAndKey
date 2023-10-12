using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TitlescreenUI : MonoBehaviour
{
    // First scene must be the starting scene shown to player
    public List<GameObject> screens;
    private GameObject currentlyEnabled;
    public Animator wallpaperUI;
    private EventSystem mainEventSystem;

    // Start is called before the first frame update
    void Start()
    {
        currentlyEnabled = screens[0];
        mainEventSystem = EventSystem.current;
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

    /// <summary>
    /// Based on which type of button, do x
    /// </summary>
    /// <param name="button">Button String Name</param>
    public void ButtonPress(string button)
    {
        switch(button.Trim().ToLower()) {
            case "settings":
                wallpaperUI.SetBool("MoveWallpaper", true);
                SwitchScreen("Settings Screen");
                break;
            case "credits":
                wallpaperUI.SetBool("MoveWallpaper", true);
                SwitchScreen("Credits Screen");
                break;
            case "end":
                Application.Quit();
                break;
            case "backtitle":
                wallpaperUI.SetBool("MoveWallpaper", false);
                currentlyEnabled.GetComponent<Animator>().SetBool("FadeOut", true);
                SwitchScreen("Main Screen");
                break;
            case "play":
                SetScene("Onboarding1");
                break;
        }
    } 

    /// <summary>
    /// Changes the selected UI elemen for the Event System
    /// </summary>
    /// <param name="selectable">Object that is selectable</param>
    public void SetSelected(GameObject selectable) {
        try
        {
            mainEventSystem.SetSelectedGameObject(selectable);
        }
        catch (Exception e)
        {
            Debug.Log("Object is not selectable.");
        }
    }
}
