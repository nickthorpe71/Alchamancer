using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TerraformOffline : MonoBehaviour
{
    private GameManagerOffline gameManager; 
    private CellManagerOffline cellManager;
    private SoundManager soundManager;

    [Header("Elements")]
    //UI Display count for all elements
    public int waterCount;
    public int plantCount;
    public int fireCount;
    public int rockCount;
    public int lifeCount;
    public int deathCount;
    public GameObject takeCast;
    //Prefabs for all elements
    public GameObject dirt, water, plant, fire, rock, life, death;

    [Header("Player")]
    private PlayerControlOffline playerCon;
    public GameObject target;
    public bool canTake; //Whether this player can collect elements
    public float castSpeed = 0.4f; //Time before player can collect again
    private GameObject temp;
    private Animator animator;
    public bool isAI; //Whether this script is being controled by AI or not
    public SpellSO antidoteSpell; 

    [Header("Energy and Mana")]
    public int energy = 4;
    public GameObject[] energyImages;
    public int currentMana;
    public int maxMana = 8;
    public GameObject[] manaImages;

    [Header ("SFX")]
    public AudioClip takeSound;

    [Header("Buttons")]
    public GameObject takeButton;

    private void Start()
    {
        cellManager = CellManagerOffline.instance;
        gameManager = GameManagerOffline.instance;
        soundManager = SoundManager.instance;
        playerCon = GetComponent<PlayerControlOffline>();

        animator = playerCon.animator;

        energyImages = gameManager.energyImages;
        manaImages = gameManager.manaImages;

        CheckMana();

        dirt = cellManager.dirt;
        water = cellManager.water;
        plant = cellManager.plant;
        fire = cellManager.fire;
        rock = cellManager.rock;
        life = cellManager.life;
        death = cellManager.death;

        if (!isAI)
        {
            takeButton = GameObject.FindGameObjectWithTag("Take");
            takeButton.GetComponent<Button>().onClick.AddListener(delegate { Take(false); });
        }
    }

    /// <summary>
    /// Uses the masterGrid in CellManager to determine what is the target position
    /// </summary>
    public void DetermineTarget()
    {
        Vector3 relativePos = transform.position;

        if(playerCon.facingFront)
            target = cellManager.masterGrid[new Vector3(relativePos.x, relativePos.y - 1, relativePos.z)];
        if (playerCon.facingBack)
            target = cellManager.masterGrid[new Vector3(relativePos.x, relativePos.y + 1, relativePos.z)];
        if (playerCon.facingLeft)
            target = cellManager.masterGrid[new Vector3(relativePos.x - 1, relativePos.y, relativePos.z)];
        if (playerCon.facingRight)
            target = cellManager.masterGrid[new Vector3(relativePos.x + 1, relativePos.y, relativePos.z)];
    }

    /// <summary>
    /// Main function for collecting elements - Uses bool eatMana to see if we are collecting or eating the target element
    /// </summary>
    /// <param name="eatMana"></param>
    public void Take(bool eatMana)
    {
        DetermineTarget();

        if (currentMana < maxMana || eatMana)
        {
            if (energy > 0 || eatMana)
            {
                if (target != null)
                {
                    if (target.tag != "Dirt")
                    {
                        if (target.tag != "DragonVein")
                        {
                            Cast();
                            Vector3 tempVec = target.transform.position;
                            switch (target.tag)
                            {
                                case "Rock":
                                    cellManager.Replace(tempVec, dirt);
                                    if (eatMana)
                                        rockCount += 2;
                                    else
                                        rockCount++;
                                    break;
                                case "Water":
                                    cellManager.Replace(tempVec, dirt);
                                    if (eatMana)
                                        waterCount += 2;
                                    else
                                        waterCount++;
                                    break;
                                case "Plant":
                                    cellManager.Replace(tempVec, dirt);
                                    if (eatMana)
                                        plantCount += 2;
                                    else
                                        plantCount++;
                                    break;
                                case "Fire":
                                    cellManager.Replace(tempVec, dirt);
                                    if (eatMana)
                                        fireCount += 2;
                                    else
                                        fireCount++;
                                    break;
                                case "Life":
                                    cellManager.Replace(tempVec, dirt);
                                    if (eatMana)
                                        lifeCount += 2;
                                    else
                                        lifeCount++;
                                    break;
                                case "Death":
                                    cellManager.Replace(tempVec, dirt);
                                    if (eatMana)
                                        deathCount += 2;
                                    else
                                        deathCount++;
                                    break;
                            }
                            PostTake(eatMana);
                            DetermineTarget();
                        }
                        else
                        {
                            if (!isAI)
                                gameManager.DisplayMessage("Cannot absorb this ancient material.");
                        }

                    }
                    else
                    {
                        if (!isAI)
                            gameManager.DisplayMessage("Nothing here.");
                    }
                }
                else
                {
                    if (!isAI)
                        gameManager.DisplayMessage("Nothing here.");
                }
            }
            else
            {
                if (!isAI)
                    gameManager.DisplayMessage("You are out of energy until next turn.");
            }
        }
        else
        {
            if (!isAI)
                gameManager.DisplayMessage("Your body cannot hold any more mana. You must cast a spell to make room.");
        }

        gameManager.spellCaster.CheckCosts();
        if (gameManager.spellCaster.sorted)
            gameManager.spellCaster.SortButtonSorted();

    }

    /// <summary>
    /// Grouping of things that need to be done after the Take function is run
    /// </summary>
    /// <param name="eatMana"></param>
    public void PostTake(bool eatMana)
    {
        soundManager.PlaySingle(takeSound, 0.8f);

        if (!eatMana)
        {
            energy--;
            currentMana++;
        }
        else
            currentMana += 2;

        if (!isAI)
        {
            CheckEnergy();
            CheckMana();
            gameManager.UpdateElements();
        }

        DetermineTarget();

        Quaternion castRotation = Quaternion.Euler(0, 0, 0);

        //Determine rotation of post take animation
        temp = Instantiate(takeCast, target.transform.position, Quaternion.identity);
        if (playerCon.facingBack)
            castRotation = Quaternion.Euler(90, 0, 0);
        if (playerCon.facingFront)
            castRotation = Quaternion.Euler(270, 0, 0);
        if (playerCon.facingRight)
            castRotation = Quaternion.Euler(180, 90, 0);
        if (playerCon.facingLeft)
            castRotation = Quaternion.Euler(0, 90, 0);

        temp.transform.GetChild(0).transform.rotation = castRotation;
    }

    /// <summary>
    /// Uses direction bools to decide which cast animation to play
    /// </summary>
    public void Cast()
    {
        if (playerCon.facingFront)
            CastFront();
        if (playerCon.facingBack)
            CastBack();
        if (playerCon.facingLeft)
            CastLeft();
        if (playerCon.facingRight)
            CastRight();
        StartCoroutine(CastRoutine());
    }

    /// <summary>
    /// Uses castSpeed to delay the player after they collect an element
    /// </summary>
    /// <returns></returns>
    public IEnumerator CastRoutine()
    {
        playerCon.canMove = false;
        yield return new WaitForSeconds(castSpeed);
        playerCon.canMove = true;

    }

    /// <summary>
    /// Plays cast animation facing forward
    /// </summary>
    public void CastFront()
    {
        ResetAnim();
        animator.SetTrigger("CastFront");
    }

    /// <summary>
    /// Plays cast animation facing backward
    /// </summary>
    public void CastBack()
    {
        ResetAnim();
        animator.SetTrigger("CastBack");
    }

    /// <summary>
    /// Plays cast animation facing left
    /// </summary>
    public void CastLeft()
    {
        ResetAnim();
        animator.SetTrigger("CastLeft");
    }

    /// <summary>
    /// Plays cast animation facing right
    /// </summary>
    public void CastRight()
    {
        ResetAnim();
        animator.SetTrigger("CastRight");
    }

    /// <summary>
    /// Resets cast animation triggers
    /// </summary>
    public void ResetAnim()
    {
        animator.ResetTrigger("CastFront");
        animator.ResetTrigger("CastBack");
        animator.ResetTrigger("CastLeft");
        animator.ResetTrigger("CastRight");
    }

    /// <summary>
    /// Use after adjusting energy to make sure energy display images are reflecting current energy
    /// </summary>
    public void CheckEnergy()
    {
        
        for (int i = 0; i < energyImages.Length; i++)
        {
            energyImages[i].SetActive(false);
        }

        if (energy >= 4)
            energy = 4;

        if (energy <= 0)
        {
            canTake = false;
            energy = 0;
        }
        else
            canTake = true;

        for (int i = 0; i < energy; i++)
        {
            energyImages[i].SetActive(true);
        }
    }

    /// <summary>
    /// Use after adjusting mana to make sure mana display images are reflecting current mana
    /// </summary>
    public void CheckMana()
    {

        for (int i = 0; i < manaImages.Length; i++)
        {
            manaImages[i].SetActive(false);
        }

        if (currentMana >= maxMana)
            currentMana = maxMana;

        if (currentMana <= 0)
            currentMana = 0;

        for (int i = 0; i < currentMana; i++)
        {
            manaImages[i].SetActive(true);
        }
    }
}
