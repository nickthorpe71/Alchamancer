using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Main script for the screen where players are taken to after losing a match
/// </summary>
public class LoseScreen : MonoBehaviour
{
    //UI display text fields
    public Text playerName;
    public Text playerRP;
    public Text lostRPTxt;

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

        string dataFormatName = FormatForDatabase(saveLoad.playerName);
        Database.RemoveRow(dataFormatName);
        Database.AddNewRow(saveLoad.playerName, saveLoad.playerRP, 0, SystemInfo.deviceUniqueIdentifier);

        currentSkin = saveLoad.characterSkin;
        CheckSkinImage();

        SoundManager.instance.RandomizeMusic(sceneTrack);
        playerName.text = saveLoad.playerName;
        playerRP.text = startRP.ToString();

        int lostRP = finalRP - startRP;
        lostRPTxt.text = lostRP + "rp";

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
        float newRP = startRP + 320 * (0 - expectedRP);

        return Mathf.RoundToInt(newRP);
    }

    /// <summary>
    /// Increases the players rank points rapidly 
    /// </summary>
    /// <returns></returns>
    IEnumerator IncrementRP()
    {
        while (currentRP > finalRP)
        {
            currentRP--;
            yield return new WaitForSeconds(0.025f);
            playerRP.text = currentRP.ToString();
        }

        int lostRP = finalRP - startRP;

        if(lostRP < 1)
        {
            rankedObj.SetActive(false);
            arrow.SetActive(false);
        }
        else
        {
            rankedObj.SetActive(true);
            arrow.SetActive(true);
        }

        rankedObj.SetActive(true);
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
    /// Subs out " " for "+" so the names can be read by the database
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    private string FormatForDatabase(string username)
    {
        string dataFormat = "";

        for (int i = 0; i < username.Length; i++)
        {
            if (username[i] == ' ')
                dataFormat += "+";
            else
                dataFormat += username[i];
        }
        return dataFormat;
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
        SceneSelect.instance.MainMenuButton();
    }

    /// <summary>
    /// Sends user to the leaderboards page
    /// </summary>
    public void LeaveEarlyToLeaderboards()
    {
        SceneSelect.instance.LeaderBoard();
    }


}