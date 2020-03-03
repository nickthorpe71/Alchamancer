using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Allows user to change their character skin
/// </summary>
public class SkinSelect : MonoBehaviour
{
    public List<GameObject> skins = new List<GameObject>(); //List of all skins
    public List<bool> availability = new List<bool>();

    public Image visibleSkin;
    private int currentSkin;

    private SaveLoad saveLoad;

    public GameObject selectButton;
    public GameObject rightButton;
    public GameObject leftButton;


    private void Start()
    {
        saveLoad = SaveLoad.instance;

        currentSkin = saveLoad.characterSkin;
        CheckSkinImage();

        CheckButtons();

        if (currentSkin == 0)
            leftButton.SetActive(false);

        if (currentSkin >= saveLoad.numOfAvailableSkins - 1)
            rightButton.SetActive(false);
    }

    /// <summary>
    /// switches displayed skin to the previous in skins list
    /// </summary>
    public void ChangeSkinLeft()
    {
        SoundManager.instance.PlayButtonClick();

        currentSkin--;
        CheckSkinImage();

        CheckButtons();

        rightButton.SetActive(true);

        if (currentSkin == 0)
            leftButton.SetActive(false);
    }

    /// <summary>
    /// switches displayed skin to the next in skins list
    /// </summary>
    public void ChangeSkinRight()
    {
        SoundManager.instance.PlayButtonClick();

        currentSkin++;
        CheckSkinImage();

        CheckButtons();

        leftButton.SetActive(true);

        if (currentSkin >= saveLoad.numOfAvailableSkins - 1)
            rightButton.SetActive(false);
    }

    /// <summary>
    /// Sets the "select" button to active if this is not the players current skin else sets "select" button to inactive
    /// </summary>
    private void CheckButtons()
    {
        if (currentSkin == saveLoad.characterSkin)
            selectButton.SetActive(false);
        else
            selectButton.SetActive(true);
    }

    /// <summary>
    /// Displays the appropriate skin from the kins list
    /// </summary>
    private void CheckSkinImage()
    {
        foreach(GameObject skin in skins)
        {
            skin.SetActive(false);
        }

        skins[currentSkin].SetActive(true);
    }

    /// <summary>
    /// Sets whichever skin is being displayed as the players current skin
    /// </summary>
    public void SelectSkin()
    {
        SoundManager.instance.PlayButtonClick();

        selectButton.SetActive(false);

        saveLoad.characterSkin = currentSkin;
        saveLoad.Save();
    }

    /// <summary>
    /// Allows player to purchase the dispalyed skin
    /// </summary>
    public void BuySkin()
    {
        SoundManager.instance.PlayButtonClick();
        selectButton.SetActive(true);

        saveLoad.skinAvailability[currentSkin] = true;
        saveLoad.Save();
    }
}
