using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public AudioClip mainMenuMusic;
    public Database database;

    public Button donateButton;

    public QuickStartLobbyController lobbyController;

    public Text playerName;
    public Text playerRP;
    public Text playerTitle;
    public Text version;

    public Color eldar;
    public Color archsage;
    public Color sage;
    public Color scribe;
    public Color acolyte;

    public Text news;
    private int newsInt = 1;

    private SoundManager soundManager;
    private SaveLoad saveLoad;

    public GameObject checkVersion;
    private bool versionMsgDisplayed;

    public List<GameObject> skins = new List<GameObject>();

    void Start()
    {
        Application.runInBackground = true;

        CheckForC();

        database = Database.instance;
        saveLoad = SaveLoad.instance;
        soundManager = SoundManager.instance;

        version.text = "v" + Application.version;

        CheckSkinImage();

        if (saveLoad.doneTutorial)
        {
            playerName.text = saveLoad.playerName;
            playerRP.text = "----";
            playerTitle.text = "----";
        }
        else
        {
            SceneSelect.instance.NewProfile();
            saveLoad.playerRP = 1000;
        }

        soundManager.musicSource.clip = mainMenuMusic;

        if (!soundManager.musicSource.isPlaying)
        {
            soundManager.musicSource.Play();
        }

        InvokeRepeating("RotateNews", 1f, 10);
    }

    void CheckForC()
    {
        if (SaveLoad.instance.playerName == "CUNT")
        {
            Database.RemoveRow("CUNT");
            Database.AddNewRow("I Respect Women", SaveLoad.instance.playerRP, 0, SystemInfo.deviceUniqueIdentifier);
        }
    }

    void SetTitle(int _rp)
    {

        if (_rp > 4999)
        {
            playerTitle.color = eldar;
            playerTitle.text = "Eldar";
        }
        else if (_rp < 5000 && _rp > 3499)
        {
            playerTitle.color = archsage;
            playerTitle.text = "Archsage";
        }
        else if (_rp < 3500 && _rp > 2499)
        {
            playerTitle.color = sage;
            playerTitle.text = "Sage";
        }
        else if (_rp < 2500 && _rp > 1699)
        {
            playerTitle.color = scribe;
            playerTitle.text = "Scribe";
        }
        else if (_rp < 1700)
        {
            playerTitle.color = acolyte;
            playerTitle.text = "Acolyte";
        }
    }

    private void CheckSkinImage()
    {
        foreach (GameObject skin in skins)
        {
            skin.SetActive(false);
        }

        skins[saveLoad.characterSkin].SetActive(true);
    }

    public void SinglePlayerStart()
    {
        Carry.instance.levelInfo = TwoPlayerMapGen.instance.MapGen();
        Carry.instance.levelString = Carry.instance.levelInfo.startString;

        SceneSelect.instance.SinglePlayer();
    }

    public void RotateNews()
    {
        database.Download();
        database.SendNewsAndVersion(this);
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

    public void OnDownloadNews(Row[] rows)
    {
        lobbyController.quickStartButton.GetComponent<Button>().interactable = true;
        donateButton.interactable = true;

        bool userFound = false;

        string dataFormatNameNew = FormatForDatabase(saveLoad.playerName);

        for (int i = 0; i < rows.Length; i++)
        {
            if (dataFormatNameNew == rows[i].username)
            {
                playerRP.text = "" + rows[i].rp;
                SetTitle(rows[i].rp);
                saveLoad.playerRP = rows[i].rp;
                saveLoad.Save();
                userFound = true;
            }
        }

        if(!userFound)
            Database.AddNewRow(saveLoad.playerName, saveLoad.playerRP, 0, SystemInfo.deviceUniqueIdentifier);

        if (Application.version != database.currentVersion && !versionMsgDisplayed && checkVersion != null)
            checkVersion.SetActive(true);

        if (newsInt == 0)
        {
            news.text = database.newsArr[1];
            newsInt = 1;
        }
        else
        {
            news.text = database.newsArr[0];
            newsInt = 0;
        }
    }

}
