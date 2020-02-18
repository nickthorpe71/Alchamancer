using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks 
{
    [Header("General")]
    public static GameManager instance;
    public Animator cameraAnim;
    [HideInInspector] public Terraform terraScript;
    [HideInInspector] public PlayerControl playerControl;
    private SoundManager soundManager;
    private bool canWin = true;
    public bool gameStarted;
    public int turnCounter;

    [Header("Players")]
    public List<GameObject> players = new List<GameObject>();
    public GameObject myPlayer;
    public GameObject theirPlayer;
    public GameObject[] energyImages;
    public GameObject[] manaImages;
    public bool myTurn;
    public bool collectPhase;
    public int otherPlayerSkin;
    public int otherPlayerRP;

    [Header("TurnTimer")]
    private int timePerTurn = 90;
    private int timeRemaining;
    public GameObject timer;
    public Text timerText;
    private bool timing;
    private float elapsedTime;

    [Header("Spells")]
    public SpellCaster spellCaster;
    public SpellSO currentSpell;

    [Header("UI")]
    public GameObject dialogueTextObj;
    public GameObject dialogueBox;
    public List<string> dialogueList = new List<string>();
    public GameObject toolTipObj;
    public Text toolName;
    public Text toolText;
    public Text toolPow;
    public List<Button> buttons = new List<Button>();
    public Text waterCount;
    public Text plantCount;
    public Text fireCount;
    public Text rockCount;
    public Text lifeCount;
    public Text deathCount;
    public Button infoYesButton;

    [Header("Info From LevelSO")]
    public LevelSO levelInfo;
    public string startString;

    [Header("SFX")]
    public AudioClip gameOverSound;
    public AudioClip winSound;
    public AudioClip openMessageSound;
    public AudioClip closeMessageSound;

    [Header("Music")]
    public AudioClip[] tracks;

    void Awake()
    {
        instance = this;

        TransferPlayerSkin();

        if (Carry.instance != null)
            levelInfo = Carry.instance.levelInfo;
    }

    private void Start()
    {
        PhotonNetwork.KeepAliveInBackground = 300;

        soundManager = SoundManager.instance;
        soundManager.gameManager = this;
        soundManager.RandomizeMusic(tracks);

        if (!SaveLoad.instance.tournamentHost)
        {
            Invoke("TurnFalse", 1);
            Invoke("Setup", 3);

            InvokeRepeating("CheckOtherPlayerDrop", 10, 2);

            canWin = true;
        }

        turnCounter = 0;

    }

    private void Setup()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            TurnTrue();
            myPlayer.transform.position = new Vector2(1, 4);

            terraScript.energy = 2;
            terraScript.CheckEnergy();
        }
        else
        {
            myPlayer.transform.position = new Vector2(7, 4);
        }

        gameStarted = true;
        UpdateElements();
    }

    private void Update()
    {
        if (timing)
            CountDownUpdate();
    }

    public void TransferPlayerSkin()
    {
        int skin = SaveLoad.instance.characterSkin;
        base.photonView.RPC("RPC_TransferPlayerSkin", RpcTarget.OthersBuffered, skin);
    }

    [PunRPC]
    public void RPC_TransferPlayerSkin(int skin)
    {
        otherPlayerSkin = skin;
    }

    public void PlayPublicSingle(string clipName, float volume)
    {
        base.photonView.RPC("RPC_PlaySinglePublic", RpcTarget.All, clipName, volume);
    }

    [PunRPC]
    public void RPC_PlaySinglePublic(string clipName, float volume)
    {
        soundManager.RPC_PlaySinglePublic(clipName, volume);
    }

    private void CountDownUpdate()
    {
        if (timeRemaining > 0)
        {
            elapsedTime += Time.deltaTime;

            if (elapsedTime >= 1)
            {
                elapsedTime = 0f;
                timeRemaining--;
                timerText.text = timeRemaining.ToString();
            }

        }
        else
        {
            timing = false;
            NextTurn();
        }
    }

    public void NextTurn()
    {
        if (myTurn && !SaveLoad.instance.tournamentHost)
        {
            if (terraScript.energy == 0 || timeRemaining == 0)
            {
                TurnFalse();
                base.photonView.RPC("RPC_TurnTrue", RpcTarget.OthersBuffered);
                if(turnCounter >= 4)
                    StartCoroutine(SpawnRandom(4));
                soundManager.PlayButtonClick();

                if (playerControl.spiritWalk)
                    playerControl.SpiritWalk();
            }
            else
                DisplayMessage("You still have energy. Collect mana until your energy is depleted.");
        }
    }

    public IEnumerator SpawnRandom(int numberOfSpawns)
    {
        yield return new WaitForSeconds(2f);

        for (int i = 0; i < numberOfSpawns; i++)
        {
            CellManager.instance.SpawnRandom();
            print("ceas");
            yield return new WaitForSeconds(0.5f);
        }
    }

    [PunRPC]
    private void RPC_TurnFalse()
    {
        TurnFalse();
    }

    [PunRPC]
    private void RPC_TurnTrue()
    {
        TurnTrue();
        turnCounter++;
    }

    void TurnTrue()
    {
        if (!SaveLoad.instance.tournamentHost)
        {
            DisplayMessage("Your turn");
            turnCounter++;

            myTurn = true;

            playerControl.NextTurn();
            terraScript.canTake = true;

            timer.SetActive(true);
            timeRemaining = timePerTurn;
            timerText.text = timeRemaining.ToString();
            timing = true;
        }

    }

    void TurnFalse()
    {
        if (!SaveLoad.instance.tournamentHost)
        {
            myTurn = false;
            playerControl.canMove = false;
            playerControl.DoT();
            terraScript.canTake = false;

            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].interactable = false;
            }

            timer.SetActive(false);
        }
    }

    public void CameraShake()
    {
        base.photonView.RPC("RPC_CameraShake", RpcTarget.All);
    }

    [PunRPC]
    public void RPC_CameraShake()
    {
        cameraAnim.SetTrigger("Shake");
    }

    public void UpdateElements()
    {
        waterCount.text = terraScript.waterCount.ToString();
        plantCount.text = terraScript.plantCount.ToString();
        fireCount.text = terraScript.fireCount.ToString();
        rockCount.text = terraScript.rockCount.ToString();
        lifeCount.text = terraScript.lifeCount.ToString();
        deathCount.text = terraScript.deathCount.ToString();
    }

    public void DisplayMessage(string dialogue)
    {
        if (!SaveLoad.instance.tournamentHost)
        {
            playerControl.canMove = false;
        }

        dialogueList.Add(dialogue);

        if (dialogueList.Count == 1)
        {
            dialogueBox.SetActive(true);
            dialogueTextObj.GetComponent<Text>().text = dialogueList[0];

            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].interactable = false;
            }
        }
    }

    public void DisplayPublicMessage(string message)
    {
        base.photonView.RPC("RPC_DisplayPublicMessage", RpcTarget.All, message);
    }

    [PunRPC]
    public void RPC_DisplayPublicMessage(string message)
    {
        DisplayMessage(message);
    }

    public void CloseMessage()
    {
        StartCoroutine(CloseMessageRoutine());
    }

    IEnumerator CloseMessageRoutine()
    {
        soundManager.PlaySingle(closeMessageSound, 1);

        if (dialogueList.Count <= 1)
        {
            dialogueBox.SetActive(false);
            dialogueList.Clear();

            yield return new WaitForSeconds(0.25f);

            if (myTurn)
            {
                for (int i = 0; i < buttons.Count; i++)
                {
                    buttons[i].interactable = true;
                }
            }
        }
        else
        {
            dialogueTextObj.GetComponent<Text>().text = dialogueList[1];
            dialogueList.Remove(dialogueList[0]);
        }
    }

    public void DisplayToolTip (string name, string message, string pow)
    {
        playerControl.canMove = false;

        soundManager.PlaySingle(openMessageSound, 1);
        toolTipObj.SetActive(true);
        toolName.text = name;
        toolText.text = message;
        toolPow.text = "Pow: " + pow;

    }

    public void CloseToolTipNo()
    {
        toolTipObj.SetActive(false);
        soundManager.PlaySingle(closeMessageSound, 1);
    }

    public void CloseToolTipYes()
    {
        toolTipObj.SetActive(false);
        soundManager.PlaySingle(closeMessageSound, 1);

        if (currentSpell != null)
        {
            if (myTurn)
                spellCaster.SubmitAttack(currentSpell.power, currentSpell);
            else
                DisplayMessage("It is not your turn.");
        }
    }

    public void RageQuit()
    {
        if (!SaveLoad.instance.tournamentHost)
        {
            StatsManager.instance.MyHealth(1000 * -1, false);
            DisplayPublicMessage(playerControl.screenName + " has exited the game");

            canWin = false;

            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].interactable = false;
            }

            SaveLoad.instance.otherPlayerRP = otherPlayerRP;
            soundManager.musicSource.Stop();

            PhotonNetwork.LeaveRoom();
            GetComponent<SceneSelect>().LoseScreen();

            GetComponent<PauseMenu>().pauseMenu.SetActive(false);
            GetComponent<PauseMenu>().settingsMenu.SetActive(false);
        }
    }

    public void GameOver()
    {
        base.photonView.RPC("RPC_WinRoutine", RpcTarget.Others);
        StartCoroutine(LoseRoutine());
    }

    [PunRPC]
    public void RPC_LoseRoutine()
    {
        if(canWin)
            StartCoroutine(LoseRoutine());
    }

    public IEnumerator LoseRoutine()
    {
        if (!SaveLoad.instance.tournamentHost)
        {
            canWin = false;

            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].interactable = false;
            }

            SaveLoad.instance.otherPlayerRP = otherPlayerRP;
            SaveLoad.instance.otherPlayerName = StatsManager.instance.theirName.text;
            SaveLoad.instance.onlineMatch = true;

            soundManager.PlaySingle(gameOverSound, 1);
            soundManager.musicSource.Stop();

            yield return new WaitForSeconds(0.5f);

            if (playerControl.animator != null && playerControl != null)
            {
                playerControl.AnimBoolFalse();
                playerControl.FacingBoolReset();
                playerControl.animator.SetBool("FaceFront", true);
            }

            yield return new WaitForSeconds(0.5f);

            if (playerControl.animator != null && playerControl != null)
            {
                playerControl.AnimBoolFalse();
                playerControl.FacingBoolReset();
                playerControl.animator.SetBool("Death", true);
                playerControl.canMove = false;
            }

            DisplayMessage("You have been defeated!");
            yield return new WaitForSeconds(5);
            PhotonNetwork.LeaveRoom();
            GetComponent<SceneSelect>().LoseScreen();
        }
    }

    [PunRPC]
    public void RPC_WinRoutine()
    {
        if(canWin)
            StartCoroutine(WinRoutine());
    }

    public IEnumerator WinRoutine()
    {
        if (!SaveLoad.instance.tournamentHost)
        {
            canWin = false;

            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].interactable = false;
            }

            if (StatsManager.instance.myHP < 0)
                StatsManager.instance.myHP = 0;

            SaveLoad.instance.otherPlayerRP = otherPlayerRP;
            SaveLoad.instance.otherPlayerName = StatsManager.instance.theirName.text;
            SaveLoad.instance.onlineMatch = true;

            playerControl.canMove = false;

            yield return new WaitForSeconds(1);

            DisplayMessage("Well done! You win!");

            yield return new WaitForSeconds(0.5f);

            if (playerControl.animator != null && playerControl != null)
            {
                playerControl.AnimBoolFalse();
                playerControl.FacingBoolReset();
                playerControl.animator.SetBool("FaceFront", true);
            }

            yield return new WaitForSeconds(0.5f);

            if (playerControl.animator != null && playerControl != null)
            {
                playerControl.AnimBoolFalse();
                playerControl.FacingBoolReset();
                playerControl.animator.SetBool("Win", true);
            }

            yield return new WaitForSeconds(3f);

            PhotonNetwork.LeaveRoom();
            GetComponent<SceneSelect>().WinScreen();
        }
    }

    void CheckOtherPlayerDrop()
    {
        if (theirPlayer == null)
        {
            if (gameStarted)
            {
                StartCoroutine(WinRoutine());
                DisplayMessage("The opponent has left the match.");
            }
            else
            {
                StartCoroutine(NoOpponentRoutine());
            }
        }
    }

    IEnumerator NoOpponentRoutine()
    {
        DisplayMessage("Opponent dropped before the match started \n\nRetruning to main menu");
        yield return new WaitForSeconds(7);
        SceneSelect.instance.MainMenuButton();
    }

    public void OnApplicationQuit()
    {
        if (!SaveLoad.instance.tournamentHost)
        {
            Scene currentScene = SceneManager.GetActiveScene();

            string sceneName = currentScene.name;

            if (sceneName == "Forest")
            {
                string tempRP = StatsManager.instance.theirRP.text.Substring(2, StatsManager.instance.theirRP.text.Length - 3);
                Debug.Log("Their RP from Stats " + StatsManager.instance.theirRP.text);

                int otherPlayerRP = int.Parse(tempRP);

                int startRP = SaveLoad.instance.playerRP;
                int finalRP = CalculateFinalRP(startRP, otherPlayerRP);
                SaveLoad.instance.playerRP = finalRP;
                SaveLoad.instance.Save();

                int CalculateFinalRP(int myRP, int opponentRP)
                {
                    float exponent = (opponentRP - myRP) / 400;
                    float expectedRP = 1 / (1 + Mathf.Pow(10, exponent));
                    float newRP = startRP + 320 * (0 - expectedRP);

                    return Mathf.RoundToInt(newRP);
                }

                Debug.Log("Player Quit Durring Match - Logging Loss - RP = " + SaveLoad.instance.playerRP);
                string dataFormatName = Database.instance.FormatUsername(SaveLoad.instance.playerName);
                Database.RemoveRow(dataFormatName);
                Database.AddNewRow(SaveLoad.instance.playerName, SaveLoad.instance.playerRP, 0, SystemInfo.deviceUniqueIdentifier);
            }
        }
    }

}
