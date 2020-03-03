using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

/// <summary>
/// Multiplayer - Manages flow of each match - primarily turns, wins/loses and messaging
/// </summary>
public class GameManager : MonoBehaviourPunCallbacks 
{
    [Header("General")]
    public static GameManager instance; //Allows this script to be referenced easily from other scrips in scene
    public Animator cameraAnim; //Animator for screen shake
    [HideInInspector] public Terraform terraScript; //Reference to local players Terraform script
    [HideInInspector] public PlayerControl playerControl; //Reference to local players PlayerControl script
    private SoundManager soundManager; //Reference to the SoundManager
    private bool canWin = true; //Whether local player cna win currently
    public bool gameStarted;
    public int turnCounter; //Counts which turn is curently being played

    [Header("Players")]
    public List<GameObject> players = new List<GameObject>(); //List of all players
    public GameObject myPlayer; //Local player object
    public GameObject theirPlayer; //Opponents player object
    public GameObject[] energyImages; //Display imaages for local player energy
    public GameObject[] manaImages; //Display imaages for local player mana
    public bool myTurn;
    public int otherPlayerSkin; //Int of which skin the other player is using so it can be reflected in local instance of the game
    public int otherPlayerRP; //Store other players Rank Points to determine final score

    [Header("TurnTimer")]
    private int timePerTurn = 90;
    private int timeRemaining;
    public GameObject timer; //So timer can be set active and inactive
    public Text timerText;
    private bool timing;
    private float elapsedTime;

    [Header("Spells")]
    public SpellCaster spellCaster; 
    public SpellSO currentSpell; //Used to hold the current spell being cast

    [Header("UI")]
    public GameObject dialogueTextObj; //To access actual text in dialogue box
    public GameObject dialogueBox; //To set dialogue box active and inactive
    public List<string> dialogueList = new List<string>(); //List of pending dialogue
    public GameObject toolTipObj; //To set tool tip active and inactive
    public Text toolName;
    public Text toolText;
    public Text toolPow;
    public List<Button> buttons = new List<Button>();
    //Local players UI display count of each element
    public Text waterCount;
    public Text plantCount;
    public Text fireCount;
    public Text rockCount;
    public Text lifeCount;
    public Text deathCount;
    
    public Button infoYesButton; //The button that casts from within the tooltip

    [Header("Info From LevelSO")]
    public LevelSO levelInfo;
    public string startString;

    [Header("SFX")]
    public AudioClip gameOverSound;
    public AudioClip winSound;
    public AudioClip openMessageSound;
    public AudioClip closeMessageSound;

    [Header("Music")]
    public AudioClip[] tracks; //All possible battle music tracks

    void Awake()
    {
        instance = this;

        TransferPlayerSkin();

        if (Carry.instance != null)
            levelInfo = Carry.instance.levelInfo;
    }

    private void Start()
    {
        PhotonNetwork.KeepAliveInBackground = 300; //Set the amount of time the network connection should be maintained if player minimizes app

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

    /// <summary>
    /// Sets the player who is the master client to take first turn and starts the game
    /// </summary>
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

    /// <summary>
    /// Sends local players skin int to other players
    /// </summary>
    public void TransferPlayerSkin()
    {
        int skin = SaveLoad.instance.characterSkin;
        base.photonView.RPC("RPC_TransferPlayerSkin", RpcTarget.OthersBuffered, skin);
    }

    /// <summary>
    /// Receives skin int from otehr players
    /// </summary>
    /// <param name="skin"></param>
    [PunRPC]
    public void RPC_TransferPlayerSkin(int skin)
    {
        otherPlayerSkin = skin;
    }

    /// <summary>
    /// Sends a clip by name and the volume it should be played at to other players 
    /// </summary>
    /// <param name="clipName"></param>
    /// <param name="volume"></param>
    public void PlayPublicSingle(string clipName, float volume)
    {
        base.photonView.RPC("RPC_PlaySinglePublic", RpcTarget.All, clipName, volume);
    }

    /// <summary>
    /// Receives audio clip by name from other players and plays it at specified volume through SoundManager
    /// </summary>
    /// <param name="clipName"></param>
    /// <param name="volume"></param>
    [PunRPC]
    public void RPC_PlaySinglePublic(string clipName, float volume)
    {
        soundManager.RPC_PlaySinglePublic(clipName, volume);
    }

    /// <summary>
    /// Run in update to reduce turn timer
    /// </summary>
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

    /// <summary>
    /// Ends local players turn and sends message to other player to start their turn
    /// </summary>
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

    /// <summary>
    /// Spawns numberOfSpawns int amount of new elements through CellManager
    /// </summary>
    /// <param name="numberOfSpawns"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Receives a message to run TurnFalse function for local player
    /// </summary>
    [PunRPC]
    private void RPC_TurnFalse()
    {
        TurnFalse();
    }

    /// <summary>
    /// Receives a message to run TurnTrue function for local player and increase turnCounter
    /// </summary>
    [PunRPC]
    private void RPC_TurnTrue()
    {
        TurnTrue();
        turnCounter++;
    }

    /// <summary>
    /// Starts local players turn
    /// </summary>
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

    /// <summary>
    /// Needs tobe run at end of local players turn to stop movement etc
    /// </summary>
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

    /// <summary>
    /// Sends a message to play camera shake for all players including local player
    /// </summary>
    public void CameraShake()
    {
        base.photonView.RPC("RPC_CameraShake", RpcTarget.All);
    }

    /// <summary>
    /// Receives a message to play camera shake for local player
    /// </summary>
    [PunRPC]
    public void RPC_CameraShake()
    {
        cameraAnim.SetTrigger("Shake");
    }

    /// <summary>
    /// Updates all element count display numbers
    /// </summary>
    public void UpdateElements()
    {
        waterCount.text = terraScript.waterCount.ToString();
        plantCount.text = terraScript.plantCount.ToString();
        fireCount.text = terraScript.fireCount.ToString();
        rockCount.text = terraScript.rockCount.ToString();
        lifeCount.text = terraScript.lifeCount.ToString();
        deathCount.text = terraScript.deathCount.ToString();
    }

    /// <summary>
    /// Displays a message - specified by string dialogue - on screen for local player only
    /// </summary>
    /// <param name="dialogue"></param>
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

    /// <summary>
    /// Sends a message to display an on screen message box containing parameter string to all players including local player
    /// </summary>
    /// <param name="message"></param>
    public void DisplayPublicMessage(string message)
    {
        base.photonView.RPC("RPC_DisplayPublicMessage", RpcTarget.All, message);
    }

    /// <summary>
    /// Receives message to display an on screen message box containing parameter string for local player
    /// </summary>
    /// <param name="message"></param>
    [PunRPC]
    public void RPC_DisplayPublicMessage(string message)
    {
        DisplayMessage(message);
    }

    /// <summary>
    /// Starts routine that closes current message
    /// </summary>
    public void CloseMessage()
    {
        StartCoroutine(CloseMessageRoutine());
    }

    /// <summary>
    /// Closes current message and displays next message if there is one else closes dialogue box
    /// </summary>
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

    /// <summary>
    /// Displays tool tip box with info from selected spell
    /// </summary>
    /// <param name="name"></param>
    /// <param name="message"></param>
    /// <param name="pow"></param>
    public void DisplayToolTip (string name, string message, string pow)
    {
        playerControl.canMove = false;

        soundManager.PlaySingle(openMessageSound, 1);
        toolTipObj.SetActive(true);
        toolName.text = name;
        toolText.text = message;
        toolPow.text = "Pow: " + pow;

    }

    /// <summary>
    /// Closes tool tip
    /// </summary>
    public void CloseToolTipNo()
    {
        toolTipObj.SetActive(false);
        soundManager.PlaySingle(closeMessageSound, 1);
    }

    /// <summary>
    /// Closes tool tip and casts the specified spell
    /// </summary>
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

    /// <summary>
    /// Runs if local player quits to main menu before the end of the match - records a loss for local player and a win for opponent(s)
    /// </summary>
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

    /// <summary>
    /// Starts LoseRoutine for local player and sends message to start WinRoutine for opponent(s)
    /// </summary>
    public void GameOver()
    {
        base.photonView.RPC("RPC_WinRoutine", RpcTarget.Others);
        StartCoroutine(LoseRoutine());
    }

    /// <summary>
    /// Receive message to start the lose routine for local player
    /// </summary>
    [PunRPC]
    public void RPC_LoseRoutine()
    {
        if(canWin)
            StartCoroutine(LoseRoutine());
    }

    /// <summary>
    /// Handles ending the match for local player and calculates a loss for them
    /// </summary>
    public IEnumerator LoseRoutine()
    {
        if (!SaveLoad.instance.tournamentHost)
        {
            canWin = false;

            //Turn off all buttons
            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].interactable = false;
            }

            //Save other players info in SaveLoad to calculate score after match
            SaveLoad.instance.otherPlayerRP = otherPlayerRP;
            SaveLoad.instance.otherPlayerName = StatsManager.instance.theirName.text;
            SaveLoad.instance.onlineMatch = true;

            soundManager.PlaySingle(gameOverSound, 1);
            soundManager.musicSource.Stop();

            yield return new WaitForSeconds(0.5f);

            //Have local player face front
            if (playerControl.animator != null && playerControl != null)
            {
                playerControl.AnimBoolFalse();
                playerControl.FacingBoolReset();
                playerControl.animator.SetBool("FaceFront", true);
            }

            yield return new WaitForSeconds(0.5f);

            //Play death animation for local player
            if (playerControl.animator != null && playerControl != null)
            {
                playerControl.AnimBoolFalse();
                playerControl.FacingBoolReset();
                playerControl.animator.SetBool("Death", true);
                playerControl.canMove = false;
            }


            DisplayMessage("You have been defeated!");
            yield return new WaitForSeconds(5);
            PhotonNetwork.LeaveRoom(); //Leave networked match
            GetComponent<SceneSelect>().LoseScreen(); //Go to lose screen 
        }
    }

    /// <summary>
    /// Receives a message to start WinRoutine for local player
    /// </summary>
    [PunRPC]
    public void RPC_WinRoutine()
    {
        if(canWin)
            StartCoroutine(WinRoutine());
    }

    /// <summary>
    /// Handles ending the match for local player and calculates a win for them
    /// </summary>
    public IEnumerator WinRoutine()
    {
        if (!SaveLoad.instance.tournamentHost)
        {
            canWin = false;

            //Turn off all buttons
            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].interactable = false;
            }

            if (StatsManager.instance.myHP < 0)
                StatsManager.instance.myHP = 0;

            //Save other players info in SaveLoad to calculate score after match
            SaveLoad.instance.otherPlayerRP = otherPlayerRP;
            SaveLoad.instance.otherPlayerName = StatsManager.instance.theirName.text;
            SaveLoad.instance.onlineMatch = true;

            playerControl.canMove = false; //Prevent player from moving

            yield return new WaitForSeconds(1);

            DisplayMessage("Well done! You win!");

            yield return new WaitForSeconds(0.5f);

            //Make player face forward
            if (playerControl.animator != null && playerControl != null)
            {
                playerControl.AnimBoolFalse();
                playerControl.FacingBoolReset();
                playerControl.animator.SetBool("FaceFront", true);
            }

            yield return new WaitForSeconds(0.5f);

            //Play win animation on local player
            if (playerControl.animator != null && playerControl != null)
            {
                playerControl.AnimBoolFalse();
                playerControl.FacingBoolReset();
                playerControl.animator.SetBool("Win", true);
            }

            yield return new WaitForSeconds(3f);

            PhotonNetwork.LeaveRoom(); //Leave networked match
            GetComponent<SceneSelect>().WinScreen(); //Go to win screen
        }
    }

    /// <summary>
    /// Checks if the other player is still in the game
    /// </summary>
    void CheckOtherPlayerDrop()
    {
        if (theirPlayer == null)
        {
            if (gameStarted)
            {
                //If the other player leaves part way then start WinRoutine
                StartCoroutine(WinRoutine());
                DisplayMessage("The opponent has left the match.");
            }
            else
            {
                //If the other player didn't show up at all leave to mein menu
                StartCoroutine(NoOpponentRoutine());
            }
        }
    }

    /// <summary>
    /// If the opponent dropped before the match starts this function sends local player back to main menu
    /// </summary>
    IEnumerator NoOpponentRoutine()
    {
        DisplayMessage("Opponent dropped before the match started \n\nRetruning to main menu");
        yield return new WaitForSeconds(7);
        SceneSelect.instance.MainMenuButton();
    }


    /// <summary>
    /// Runs if local player exits the app before the end of the match - records a loss for local player and a win for opponent(s)
    /// </summary>
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
