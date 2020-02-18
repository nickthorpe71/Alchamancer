using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinScreen : MonoBehaviour
{
    public Text playerName;
    public Text playerRP;
    public Text addedRPTxt;

    private SaveLoad saveLoad;

    public AudioClip sceneTrack;

    public bool rankedUp;
    public GameObject rankedObj;
    public GameObject arrow;

    public List<GameObject> skins = new List<GameObject>();
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

    private void CheckSkinImage()
    {
        foreach (GameObject skin in skins)
        {
            skin.SetActive(false);
        }

        skins[currentSkin].SetActive(true);
    }

    public void UpdateLeaderboards()
    {
        Database.AddNewRow(saveLoad.playerName, saveLoad.playerRP, 0, SystemInfo.deviceUniqueIdentifier);
    }

    public void LeaveEarly()
    {
        UpdateLeaderboards();
        SceneSelect.instance.MainMenuButton();
    }

    public void LeaveEarlyToLeaderboards()
    {
        UpdateLeaderboards();
        SceneSelect.instance.LeaderBoard();
    }


}
