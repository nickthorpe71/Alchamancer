using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Organizes info for Profile Scene
/// </summary>
public class ProfileManager : MonoBehaviour
{
    //Text object that displays player name
    public Text playerName;

    //Reference to script that saves and loads from local data
    private SaveLoad saveLoad;

    //Button that user clicks to edit their name
    public GameObject editName;

    void Start()
    {
        saveLoad = SaveLoad.instance;

        //Set players name
        playerName.text = "Name: " + saveLoad.playerName;
    }

    public void EditName()
    {
        //Called to turn the edit name button on and off
        if(!editName.activeSelf)
            editName.SetActive(true);
        else
            editName.SetActive(false);
    }

    
}
