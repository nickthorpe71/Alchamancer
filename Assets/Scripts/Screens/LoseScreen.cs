using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoseScreen : MonoBehaviour
{
    public Text playerName;
    public Text playerRP;
    public Text lostRPTxt;

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
        print("finalRP " + finalRP);
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

    int CalculateFinalRP(int myRP, int opponentRP)
    {
        float exponent = (opponentRP - myRP) / 400f;
        float expectedRP = 1 / (1 + Mathf.Pow(10, exponent));
        float newRP = startRP + 320 * (0 - expectedRP);

        return Mathf.RoundToInt(newRP);
    }

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

    private void CheckSkinImage()
    {
        foreach (GameObject skin in skins)
        {
            skin.SetActive(false);
        }

        skins[currentSkin].SetActive(true);
    }

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

    public void UpdateLeaderboards()
    {
        Database.AddNewRow(saveLoad.playerName, saveLoad.playerRP, 0, SystemInfo.deviceUniqueIdentifier);
    }

    public void LeaveEarly()
    {
        SceneSelect.instance.MainMenuButton();
    }

    public void LeaveEarlyToLeaderboards()
    {
        SceneSelect.instance.LeaderBoard();
    }


}