using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Singleplayer - Manages flow of each match - primarily turns, wins/loses and messaging
/// </summary>
public class GameManagerOffline : MonoBehaviour
{
    [Header("General")]
    public static GameManagerOffline instance; //Allows this script to be referenced easily from other scrips in scene
    public Animator cameraAnim; //Animator for screen shake
    [HideInInspector] public TerraformOffline terraScript;  //Reference to players Terraform script
    [HideInInspector] public PlayerControlOffline playerControl; //Reference to players PlayerControl script
    private SoundManager soundManager; //Reference to the SoundManager
    public int turnCounter; //Counts which turn is curently being played
    private int chance; //Used to determine if player or AI will go first

    [Header("Players")]
    public GameObject myPlayer; //Player object
    public GameObject theirPlayer; //AI player object
    public GameObject[] energyImages; //Display imaages for player energy
    public GameObject[] manaImages; //Display imaages for player mana
    public bool myTurn;

    [Header("Timer")]
    private int timePerTurn = 90;
    private int timeRemaining;
    public GameObject timer; //So timer can be set active and inactive
    public Text timerText;
    private bool timing;
    private float elapsedTime;

    [Header("Spells")]
    public SpellCasterOffline spellCaster;
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
    //Players UI display count of each element
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

        if (Carry.instance != null)
            levelInfo = Carry.instance.levelInfo;
    }

    private void Start()
    {
        chance = Random.Range(0, 2); //Roll to see which player goes first

        if(chance != 0)
            theirPlayer.GetComponent<TerraformOffline>().energy = 2;

        soundManager = SoundManager.instance;
        soundManager.RandomizeMusic(tracks);

        Invoke("Setup", 1);
    }

    /// <summary>
    /// Uses chance int to set player or AI as first turn and starts their turn
    /// </summary>
    private void Setup()
    {
        if (chance == 0)
        {
            TurnTrue();
            terraScript.energy = 2;
        }
        else
        {
            TurnFalse();
            terraScript.energy = 4;
        }

        terraScript.CheckEnergy();

        UpdateElements();
    }

    private void Update()
    {
        if (timing)
            CountDownUpdate();
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
    /// Ends current turn and passes turn to next player
    /// </summary>
    public void NextTurn()
    {
        turnCounter++;

        if (myTurn)
        {
            if (terraScript.energy == 0 || timeRemaining == 0)
            {
                TurnFalse();
                StartCoroutine(SpawnRandom(4));
                soundManager.PlayButtonClick();

                if (playerControl.spiritWalk)
                    playerControl.SpiritWalk();
            }
            else
                DisplayMessage("You still have energy. Collect mana until your energy is depleted.");
        }
        else
        {
            TurnTrue();
            StartCoroutine(SpawnRandom(4));

            if (theirPlayer.GetComponent<PlayerControlOffline>().spiritWalk)
                theirPlayer.GetComponent<PlayerControlOffline>().SpiritWalk();
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

        if (turnCounter > 4)
        {
            for (int i = 0; i < numberOfSpawns; i++)
            {
                CellManagerOffline.instance.SpawnRandom();
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    /// <summary>
    /// Sets up the player's turn 
    /// </summary>
    void TurnTrue()
    {
        DisplayMessage("Your turn");

        myTurn = true;

        playerControl.NextTurn();
        terraScript.canTake = true;

        timer.SetActive(true);
        timeRemaining = timePerTurn;
        timerText.text = timeRemaining.ToString();
        timing = true;

    }

    /// <summary>
    /// Stops player's turn and sets up AI's turn
    /// </summary>
    void TurnFalse()
    {
        myTurn = false;
        playerControl.canMove = false;
        playerControl.DoT();
        terraScript.canTake = false;

        if(turnCounter > 0)
            theirPlayer.GetComponent<PlayerControlOffline>().NextTurn();

        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].interactable = false;
        }

        timer.SetActive(false);
    }

    /// <summary>
    /// Triggers camera shake
    /// </summary>
    public void CameraShake()
    {
        cameraAnim.SetTrigger("Shake");
    }

    /// <summary>
    /// Updates UI display for element count
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
    /// Displays a message
    /// </summary>
    /// <param name="dialogue"></param>
    public void DisplayMessage(string dialogue)
    {
        playerControl.canMove = false;

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
    /// Starts routine that closes current dialogue
    /// </summary>
    public void CloseMessage()
    {
        StartCoroutine(CloseMessageRoutine());
    }

    /// <summary>
    /// Closes current dialogue and if there is a next dialogue it is displayed else closes dialogue box
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
    /// Displays tool tip for specified spell
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
    /// Closes the tool tip
    /// </summary>
    public void CloseToolTipNo()
    {
        toolTipObj.SetActive(false);
        soundManager.PlaySingle(closeMessageSound, 1);
    }

    /// <summary>
    /// Closes the tool tip and casts specified spell
    /// </summary>
    public void CloseToolTipYes()
    {
        toolTipObj.SetActive(false);
        soundManager.PlaySingle(closeMessageSound, 1);

        if (currentSpell != null)
        {
            if(myTurn)
                spellCaster.SubmitAttack(currentSpell.power, currentSpell, false);
            else
                DisplayMessage("It is not your turn.");
        }
    }

    /// <summary>
    /// Starts LoseRoutine for player
    /// </summary>
    public void GameOver()
    {
        StartCoroutine(LoseRoutine());
    }

    /// <summary>
    /// Handles ending the match for player and calculates a loss for them
    /// </summary>
    public IEnumerator LoseRoutine()
    {
        //Turn off all buttons
        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].interactable = false;
        }

        //Save other players info in SaveLoad to calculate score after match
        SaveLoad.instance.otherPlayerRP = theirPlayer.GetComponent<OpponentCreator>().rp;
        SaveLoad.instance.otherPlayerName = StatsManagerOffline.instance.theirName.text;
        SaveLoad.instance.onlineMatch = false;

        soundManager.PlaySingle(gameOverSound, 1);
        soundManager.musicSource.Stop();

        yield return new WaitForSeconds(0.5f);

        //Make player face forward
        playerControl.AnimBoolFalse();
        playerControl.FacingBoolReset();

        playerControl.animator.SetBool("FaceFront", true);

        yield return new WaitForSeconds(0.5f);

        //Play player death animation
        playerControl.AnimBoolFalse();
        playerControl.FacingBoolReset();

        playerControl.animator.SetBool("Death", true);

        //Play AI win animation
        theirPlayer.GetComponent<PlayerControlOffline>().animator.SetBool("Win", true);

        playerControl.canMove = false;

        DisplayMessage("You have been defeated!");
        yield return new WaitForSeconds(5);
        GetComponent<SceneSelect>().LoseScreen(); //Take player to lose screen
    }

    /// <summary>
    /// Starts WinRoutine for player
    /// </summary>
    public void Win()
    {
        StartCoroutine(WinRoutine());
    }

    /// <summary>
    /// Handles ending the match for player and calculates a win for them
    /// </summary>
    public IEnumerator WinRoutine()
    {
        //Turn off all buttons
        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].interactable = false;
        }

        if (StatsManagerOffline.instance.myHP < 0)
            StatsManagerOffline.instance.myHP = 0;

        //Save other players info in SaveLoad to calculate score after match
        SaveLoad.instance.otherPlayerRP = theirPlayer.GetComponent<OpponentCreator>().rp;
        SaveLoad.instance.otherPlayerName = StatsManagerOffline.instance.theirName.text;
        SaveLoad.instance.onlineMatch = false;

        playerControl.canMove = false;

        yield return new WaitForSeconds(1);

        DisplayMessage("Well done! You win!");

        yield return new WaitForSeconds(0.5f);


        //Make player face forward
        playerControl.AnimBoolFalse();
        playerControl.FacingBoolReset();

        playerControl.animator.SetBool("FaceFront", true);

        yield return new WaitForSeconds(0.5f);

        //Player player win animation
        playerControl.AnimBoolFalse();
        playerControl.FacingBoolReset();

        playerControl.animator.SetBool("Win", true);

        //Make AI face forward
        theirPlayer.GetComponent<PlayerControlOffline>().AnimBoolFalse();
        theirPlayer.GetComponent<PlayerControlOffline>().FacingBoolReset();

        //Play AI win death animation
        theirPlayer.GetComponent<PlayerControlOffline>().animator.SetBool("Death", true);

        yield return new WaitForSeconds(3f);

        GetComponent<SceneSelect>().WinScreen(); //Take player to win screen
    }
}
