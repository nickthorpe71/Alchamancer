using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
