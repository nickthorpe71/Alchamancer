using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Main scrpt for the main menu scene - primarily for populating UI display
/// </summary>
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
    private string news1;
    private string news2;
    private bool newsInitialized;

    private SoundManager soundManager;
    private SaveLoad saveLoad;

    public GameObject checkVersion;
    private bool versionMsgDisplayed;
    private string currentVersion;

    public List<GameObject> skins = new List<GameObject>();

    void Start()
    {
        Application.runInBackground = true;

        database = Database.instance;
        saveLoad = SaveLoad.instance;
        soundManager = SoundManager.instance;

        version.text = "v" + Application.version;

        CheckSkinImage();

        //Set the initial UI display to a placeholder
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

        InvokeRepeating("Refresh", 0, 30);
    }

    /// <summary>
    /// Use players Rank Points to display the correct title
    /// </summary>
    /// <param name="_rp"></param>
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

    /// <summary>
    /// Sets the display of characters selected character skin
    /// </summary>
    private void CheckSkinImage()
    {
        foreach (GameObject skin in skins)
        {
            skin.SetActive(false);
        }

        skins[saveLoad.characterSkin].SetActive(true);
    }

    /// <summary>
    /// Sends player into the single player scene
    /// </summary>
    public void SinglePlayerStart()
    {
        Carry.instance.levelInfo = TwoPlayerMapGen.instance.MapGen();
        Carry.instance.levelString = Carry.instance.levelInfo.startString;

        SceneSelect.instance.SinglePlayer();
    }

    /// <summary>
    /// Refreshed the verstion number, news1 and news2
    /// </summary>
    public void Refresh()
    {
        StartPullInfo("News1");
        StartPullInfo("News2");
        StartPullInfo("Version");
        StartPullInfo(FormatForDatabase(SaveLoad.instance.playerName));
    }

    /// <summary>
    /// Starts pulling info for main menu from the database
    /// </summary>
    /// <param name="infoName"></param>
    public void StartPullInfo(string infoName)
    {
        StartCoroutine(PullInfoRoutine(infoName));
    }

    /// <summary>
    /// Runs a routine to pull main menu info from database once download is complete
    /// </summary>
    /// <param name="infoName"></param>
    /// <returns></returns>
    public IEnumerator PullInfoRoutine(string infoName)
    {
        WWW www = new WWW(Database.instance.webURL + Database.instance.privateCode + "/pipe-get/" + WWW.EscapeURL(infoName));
        yield return www;

        if (string.IsNullOrEmpty(www.error))
        {
            print(www.text);

            if (www.text == "")
            {
                if(infoName != "News1" && infoName != "News2" && infoName != "Version")
                    Database.AddNewRow(saveLoad.playerName, saveLoad.playerRP, 0, SystemInfo.deviceUniqueIdentifier);
            }
            else
                PullInfoFinal(www.text, infoName);
        }
        else
        {
            print("Error pulling info " + www.error);
        }
    }

    /// <summary>
    /// Takes the downloaded info from the database and populates the display UI
    /// </summary>
    /// <param name="textStream"></param>
    /// <param name="infoName"></param>
    public void PullInfoFinal(string textStream, string infoName)
    {
        lobbyController.quickStartButton.GetComponent<Button>().interactable = true;
        donateButton.interactable = true;

        string dataFormatNameOld = FormatForDatabase(SaveLoad.instance.playerName);

        string[] entryInfo = textStream.Split(new char[] { '|' });

        string usernameInfo = entryInfo[0];
        int rpInfo = int.Parse(entryInfo[1]);
        int extraInfo = int.Parse(entryInfo[2]);
        string IDInfo = entryInfo[3];

        Row row = new Row(usernameInfo, rpInfo, extraInfo, IDInfo);

        if (infoName == "News1")
        {
            news1 = IDInfo;
            if (!newsInitialized)
            {
                InvokeRepeating("RotateNews", 0, 10);
                newsInitialized = true;
            }
        }
        else if (infoName == "News2")
            news2 = IDInfo;
        else if (infoName == "Version")
        {
            currentVersion = IDInfo;
            if (Application.version != currentVersion && !versionMsgDisplayed && checkVersion != null)
                checkVersion.SetActive(true);
        }
        else
        {
            playerRP.text = "" + rpInfo;
            SetTitle(rpInfo);
            saveLoad.playerRP = rpInfo;
            saveLoad.Save();
        }
    }

    /// <summary>
    /// Switches form current news being displayed to the other news
    /// </summary>
    void RotateNews()
    {
        if (newsInt == 0)
        {
            news.text = news2;
            newsInt = 1;
        }
        else
        {
            news.text = news1;
            newsInt = 0;
        }
    }

    /// <summary>
    /// Takes inserted text and replaces " " with "+" so it can be read by database
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
}
