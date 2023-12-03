using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

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
    void FixedUpdate()
    {
        Debug.Log("Checking for pause");
        Debug.Log(playerScript.Input.JustPressed(PlayerInput.Action.Pause));
        //If the start button is pressed on the controller
        if (playerScript.Input.JustPressed(PlayerInput.Action.Pause))
        {
            if(!isPaused)
            {
                Debug.Log("Pausing");
                Pause();
                return;
            }

            // Spam opens/closes if put in -- figure out something or just leave it?
            //if (isPaused)
            //{
            //    Debug.Log("Unpausing");
            //    current.enabled = false;
            //    Resume();
            //    return;
            //}
        }
         
        playerScript.Input.Update();
    }

    /// <summary>
    /// Resumes game and resets gametime
    /// </summary>
    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        current.enabled = false;
    }

    /// <summary>
    /// Pauses the game and the gametime
    /// </summary>
    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        current.SetSelectedGameObject(pauseMenuUI.transform.GetChild(0).gameObject);
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
        SceneManager.LoadScene("Titlescreen");
    }

    /// <summary>
    /// Used on respawn button. Spawns back at last checkpoint or first spawn.
    /// </summary>
    public void Checkpoint()
    {
        pauseMenuUI.SetActive(false);

        //respawn at checkpoint
        playerScript.Die();

    }
}
