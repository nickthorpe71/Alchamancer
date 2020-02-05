using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManagerOffline : MonoBehaviour
{
    [Header("General")]
    public static GameManagerOffline instance;
    public Animator cameraAnim;
    [HideInInspector] public TerraformOffline terraScript;
    [HideInInspector] public PlayerControlOffline playerControl;
    private SoundManager soundManager;
    public int turnCounter;
    private int chance;

    [Header("Players")]
    public GameObject myPlayer;
    public GameObject theirPlayer;
    public GameObject[] energyImages;
    public GameObject[] manaImages;
    public bool myTurn;

    [Header("Timer")]
    private int timePerTurn = 90;
    private int timeRemaining;
    public GameObject timer;
    public Text timerText;
    private bool timing;
    private float elapsedTime;

    [Header("Spells")]
    public SpellCasterOffline spellCaster;
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

        if (Carry.instance != null)
            levelInfo = Carry.instance.levelInfo;
    }

    private void Start()
    {
        chance = Random.Range(0, 2);

        if(chance != 0)
            theirPlayer.GetComponent<TerraformOffline>().energy = 2;

        soundManager = SoundManager.instance;
        soundManager.RandomizeMusic(tracks);

        Invoke("Setup", 1);
    }

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

    public void CameraShake()
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
            if(myTurn)
                spellCaster.SubmitAttack(currentSpell.power, currentSpell, false);
            else
                DisplayMessage("It is not your turn.");
        }
    }

    public void GameOver()
    {
        StartCoroutine(LoseRoutine());
    }

    public IEnumerator LoseRoutine()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].interactable = false;
        }

        SaveLoad.instance.otherPlayerRP = theirPlayer.GetComponent<OpponentCreator>().rp;

        soundManager.PlaySingle(gameOverSound, 1);
        soundManager.musicSource.Stop();

        yield return new WaitForSeconds(0.5f);

        playerControl.AnimBoolFalse();
        playerControl.FacingBoolReset();

        playerControl.animator.SetBool("FaceFront", true);

        yield return new WaitForSeconds(0.5f);

        playerControl.AnimBoolFalse();
        playerControl.FacingBoolReset();

        playerControl.animator.SetBool("Death", true);
        theirPlayer.GetComponent<PlayerControlOffline>().animator.SetBool("Win", true);

        playerControl.canMove = false;

        DisplayMessage("You have been defeated!");
        yield return new WaitForSeconds(5);
        GetComponent<SceneSelect>().LoseScreen();
    }

    public void Win()
    {
        StartCoroutine(WinRoutine());
    }

    public IEnumerator WinRoutine()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].interactable = false;
        }

        if (StatsManagerOffline.instance.myHP < 0)
            StatsManagerOffline.instance.myHP = 0;

        SaveLoad.instance.otherPlayerRP = theirPlayer.GetComponent<OpponentCreator>().rp;

        playerControl.canMove = false;

        yield return new WaitForSeconds(1);

        DisplayMessage("Well done! You win!");

        yield return new WaitForSeconds(0.5f);

        playerControl.AnimBoolFalse();
        playerControl.FacingBoolReset();

        playerControl.animator.SetBool("FaceFront", true);

        yield return new WaitForSeconds(0.5f);

        playerControl.AnimBoolFalse();
        playerControl.FacingBoolReset();

        playerControl.animator.SetBool("Win", true);

        theirPlayer.GetComponent<PlayerControlOffline>().AnimBoolFalse();
        theirPlayer.GetComponent<PlayerControlOffline>().FacingBoolReset();

        theirPlayer.GetComponent<PlayerControlOffline>().animator.SetBool("Death", true);

        yield return new WaitForSeconds(3f);

        GetComponent<SceneSelect>().WinScreen();
    }
}
