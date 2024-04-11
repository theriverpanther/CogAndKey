using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    // Parameters

    public bool isPaused = false;

    public GameObject pauseMenuUI;
    public GameObject optionsMenuUI;

    [SerializeField]
    GameObject player;
    [SerializeField]
    PlayerScript playerScript;

    [SerializeField]
    EventSystem current;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerScript = player.GetComponent<PlayerScript>();
        current = EventSystem.current;
        Debug.Log("Pause Screen Loaded.");
    }

    /// <summary>
    /// Checks for input
    /// </summary>
    void Update()
    {
        //Debug.Log("Checking for pause");
        //Debug.Log(PlayerInput.Instance.JustPressed(PlayerInput.Action.Pause));
        //If the start button is pressed on the controller
        if (PausedPressed())
        {
            if(!isPaused)
            {
                Debug.Log("Pausing");
                Pause();
            }
            else
            {
                Debug.Log("Unpausing");
                current.enabled = false;
                Resume();
            }
        }
    }

    /// <summary>
    /// Resumes game and resets gametime
    /// </summary>
    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        current.enabled = false;
        current.SetSelectedGameObject(null);
    }

    /// <summary>
    /// Pauses the game and the gametime
    /// </summary>
    public void Pause()
    {
        current.enabled = true;
        pauseMenuUI.SetActive(true);
        Button b = pauseMenuUI.transform.GetChild(0).gameObject.GetComponent<Button>();

        current.SetSelectedGameObject(pauseMenuUI.transform.GetChild(0).gameObject);

        b.Select();
        b.OnSelect(null);

        Time.timeScale = 0f;
        isPaused = true;
    }

    /// <summary>
    /// Shows the option  menu
    /// 
    /// </summary>
    public void Options()
    {
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(true);
    }

    /// <summary>
    /// Returns back to pause menu from options menu
    /// </summary>
    public void OptionsBack()
    {
        optionsMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);
    }

    /// <summary>
    /// Returns back to titlescreen
    /// </summary>
    public void Quit()
    {
        Resume();
        SceneManager.LoadScene("Titlescreen");
    }

    /// <summary>
    /// Used on respawn button. Spawns back at last checkpoint or first spawn.
    /// </summary>
    public void Checkpoint()
    {
        Resume();
        //respawn at checkpoint
        playerScript.Die();

    }

    /// <summary>
    /// Used on respawn button. Spawns back at last checkpoint or first spawn.
    /// </summary>
    public void SkipLevel()
    {
        Resume();
        if(SceneManager.GetActiveScene().buildIndex + 1 < SceneManager.loadedSceneCount)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        } else
        {
            SceneManager.LoadScene(0);
        }
        
    }

    private bool PausedPressed() {
        return Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame
            || Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
    }
}
