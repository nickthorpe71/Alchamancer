using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Main script for screen were players are taken after winning a match 
/// </summary>
public class WinScreen : MonoBehaviour
{
    //UI display text fields
    public Text playerName;
    public Text playerRP;
    public Text addedRPTxt;

    private SaveLoad saveLoad;

    public AudioClip sceneTrack; //Music played on this scene

    public bool rankedUp;
    public GameObject rankedObj;
    public GameObject arrow;

    public List<GameObject> skins = new List<GameObject>(); //All potential character skins
    private int currentSkin;

    private int startRP;
    private int currentRP;
    private int finalRP;

    private int otherPlayerRP;

    void Start()
    {
        saveLoad = SaveLoad.instance;

        otherPlayerRP = saveLoad.otherPlayerRP;

        startRP = saveLoad.playerRP;
        currentRP = startRP;
        finalRP = CalculateFinalRP(startRP, otherPlayerRP);
        saveLoad.playerRP = finalRP;
        saveLoad.Save();

        currentSkin = saveLoad.characterSkin;
        CheckSkinImage();

        SoundManager.instance.RandomizeMusic(sceneTrack);
        playerName.text = "Congrats " + saveLoad.playerName + "!" + " You defeated " + saveLoad.otherPlayerName + "!";
        playerRP.text = startRP.ToString();

        int addedRP = finalRP - startRP;
        addedRPTxt.text = "+" + addedRP + "rp";

        StartCoroutine(IncrementRP());
    }

    /// <summary>
    /// Uses the opponents Rank Points (RP) and players RP to calculate the amount of RP won - uses ELO calculation
    /// </summary>
    /// <param name="myRP"></param>
    /// <param name="opponentRP"></param>
    /// <returns></returns>
    int CalculateFinalRP(int myRP, int opponentRP)
    {
        float exponent = (opponentRP - myRP) / 400f;
        float expectedRP = 1 / (1 + Mathf.Pow(10, exponent));

        float newRP = startRP + 320 * (1 - expectedRP) + 10;

        if (saveLoad.onlineMatch == true)
            newRP = startRP + 320 * (1 - expectedRP) + 75;

        if(saveLoad.otherPlayerName.Contains("Geist"))
            newRP = startRP + 320 * (1 - expectedRP) + 35;


        return Mathf.RoundToInt(newRP);
    }

    /// <summary>
    /// Decreases the players rank points rapidly 
    /// </summary>
    /// <returns></returns>
    IEnumerator IncrementRP()
    {
        while(currentRP < finalRP)
        {
            currentRP++;
            yield return new WaitForSeconds(0.025f);
            playerRP.text = currentRP.ToString();
        }

        int addedRP = finalRP - startRP;

        if (addedRP < 1)
        {
            rankedObj.SetActive(false);
            arrow.SetActive(false);
        }
        else
        {
            rankedObj.SetActive(true);
            arrow.SetActive(true);
        }
    }

    /// <summary>
    /// Sets the appropriate character skin image
    /// </summary>
    private void CheckSkinImage()
    {
        foreach (GameObject skin in skins)
        {
            skin.SetActive(false);
        }

        skins[currentSkin].SetActive(true);
    }

    /// <summary>
    /// Sends updated Rank Points to the databse
    /// </summary>
    public void UpdateLeaderboards()
    {
        Database.AddNewRow(saveLoad.playerName, saveLoad.playerRP, 0, SystemInfo.deviceUniqueIdentifier);
    }

    /// <summary>
    /// Sends user to the main menu
    /// </summary>
    public void LeaveEarly()
    {
        UpdateLeaderboards();
        SceneSelect.instance.MainMenuButton();
    }

    /// <summary>
    /// Sends user to the leaderboards page
    /// </summary>
    public void LeaveEarlyToLeaderboards()
    {
        UpdateLeaderboards();
        SceneSelect.instance.LeaderBoard();
    }


}
