using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfileManager : MonoBehaviour
{
    public Text playerName;

    private SaveLoad saveLoad;

    public GameObject editName;

    void Start()
    {
        saveLoad = SaveLoad.instance;

        playerName.text = "Name: " + saveLoad.playerName;
    }

    public void EditName()
    {
        if(!editName.activeSelf)
            editName.SetActive(true);
        else
            editName.SetActive(false);
    }

    
}
