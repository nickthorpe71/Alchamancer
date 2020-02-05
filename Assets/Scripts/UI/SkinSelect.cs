using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkinSelect : MonoBehaviour
{
    public List<GameObject> skins = new List<GameObject>();
    public List<bool> availability = new List<bool>();

    public Image visibleSkin;
    private int currentSkin;

    private SaveLoad saveLoad;

    public GameObject selectButton;
    //public GameObject buyButton;
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

    private void CheckButtons()
    {
        if (currentSkin == saveLoad.characterSkin)
            selectButton.SetActive(false);
        else
            selectButton.SetActive(true);

        /*if (saveLoad.skinAvailability[currentSkin] == true)
        {
            buyButton.SetActive(false);
        }
        else
        {
            buyButton.SetActive(true);
            selectButton.SetActive(false);

        }*/
    }

    private void CheckSkinImage()
    {
        foreach(GameObject skin in skins)
        {
            skin.SetActive(false);
        }

        skins[currentSkin].SetActive(true);
    }

    public void SelectSkin()
    {
        SoundManager.instance.PlayButtonClick();

        selectButton.SetActive(false);

        saveLoad.characterSkin = currentSkin;
        saveLoad.Save();
    }

    public void BuySkin()
    {
        SoundManager.instance.PlayButtonClick();

        //buyButton.SetActive(false);
        selectButton.SetActive(true);

        saveLoad.skinAvailability[currentSkin] = true;
        saveLoad.Save();
    }
}
