using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Allows user to interract with the pause menu
/// </summary>
public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject settingsMenu;

    private bool pauseMenuOpen;
    private bool settingsMenuOpen;

    private SoundManager soundManager;

    private void Start()
    {
        soundManager = SoundManager.instance;
    }

    /// <summary>
    /// Opens or closes pause menu depending on it's current state
    /// </summary>
    public void ClickPauseMenu()
    {
        if (pauseMenuOpen)
        {
            pauseMenu.SetActive(false);
            pauseMenuOpen = false;
        }
        else
        {
            pauseMenu.SetActive(true);
            pauseMenuOpen = true;
        }

        soundManager.PlayButtonClick();
    }

    /// <summary>
    /// Opens or closes settings menu depending on it's current state
    /// </summary>
    public void ClickSettingsMenu()
    {
        if (settingsMenuOpen)
        {
            settingsMenu.SetActive(false);
            settingsMenuOpen = false;
        }
        else
        {
            settingsMenu.SetActive(true);
            settingsMenuOpen = true;
        }

        soundManager.PlayButtonClick();
    }


}
