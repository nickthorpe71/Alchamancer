using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Singleplayer Version - Handles calculating damage, effects and animations for all spells - Every spell is run through thisone manager for both the human player and AI player
/// </summary>
public class SpellCasterOffline : MonoBehaviour
{

    [Header("Management")]
    public AreaColorManagerOffline colorManager;
    private TerraformOffline myTerraScript;
    private TerraformOffline theirTerraScript;
    private GameManagerOffline gameManager;
    private SoundManager soundManager;
    private StatsManagerOffline statsManager;
    private PlayerControlOffline myPlayerControl;
    private PlayerControlOffline theirPlayerControl;

    [Header("UI")]
    public GameObject noSpells; //Message to state that there are no spells available
    public bool sorted; //Bool indicating whether spells have been sorted

    [Header("AI")]
    public bool isAI;

    [Header("Projectiles")]
    private Vector3 projectileStart;
    private Vector3 projectileEnd;

    [Header("Stationary")]
    private Vector3 targetPos;

    [Header("Spells")]
    public SpellListSO spellLibrary; //Scriptable object holding all spell scriptable objects
    public SpellSO[] spellsStandard; //Array of standard spell list
    public SpellListingOffline spellListing; //Prefab for a spell listing button
    public Transform content; //Area where spell listings will be displayed
    public List<GameObject> spellButtons = new List<GameObject>(); //List of all spell listing buttons
    public List<GameObject> animations = new List<GameObject>(); //List of all spell animations
    public Dictionary<string, GameObject> animationDict = new Dictionary<string, GameObject>(); //Dictionary where the key is the animation name and value is the corrisponding animation
    public Dictionary<string, string> oppositeDict = new Dictionary<string, string>(); //Dictionary for specifying each element's opposite
    public bool myMirror;
    public bool theirMirror;

    private void Start()
    {
        Invoke("DelayedStart", 5); //Assign scrpts that may be generated a few seconds after atart of match

        PopulateOppositeDict();

        spellsStandard = spellLibrary.spells;

        PopulateSpellList(spellsStandard);
        CheckCosts();
		PopulateAnimationDictionary();
    }

    /// <summary>
    /// Sets up opposite dictionary 
    /// </summary>
    void PopulateOppositeDict()
    {
        oppositeDict.Add("Blue", "Red");
        oppositeDict.Add("Green", "Yellow");
        oppositeDict.Add("Red", "Blue");
        oppositeDict.Add("Yellow", "Green");
        oppositeDict.Add("White", "Black");
        oppositeDict.Add("Black", "White");
    }

    /// <summary>
    /// Runs scripts a few seconds after game starts
    /// </summary>
    void DelayedStart()
    {
        gameManager = GameManagerOffline.instance;
        soundManager = SoundManager.instance;
        statsManager = StatsManagerOffline.instance;

        myTerraScript = gameManager.myPlayer.GetComponent<TerraformOffline>();
        theirTerraScript = gameManager.theirPlayer.GetComponent<TerraformOffline>();
        myPlayerControl = gameManager.playerControl;
        theirPlayerControl = gameManager.theirPlayer.GetComponent<PlayerControlOffline>();
    }

    /// <summary>
    /// Sorts spells into a list of all spells
    /// </summary>
    public void SortButtonAll()
    {
        ClearSpellButtons();
        PopulateSpellList(spellsStandard);
        sorted = false;
    }

    /// <summary>
    /// Sorts spells into a list of only what we currently have enough mana for
    /// </summary>
    public void SortButtonSorted()
    {
        SortSpells();
        sorted = true;
    }

    /// <summary>
    /// Populates UI display of spell buttons using spell listings and lsit of spell scriptable objects
    /// </summary>
    /// <param name="spells"></param>
    private void PopulateSpellList(SpellSO[] spells)
    {
        if (spells.Length > 0)
        {
            noSpells.SetActive(false);
            for (int i = 0; i < spells.Length; i++)
            {
                SpellListingOffline listing = Instantiate(spellListing, content);

                if (listing != null)
                {
                    spellButtons.Add(listing.gameObject);
                    listing.spellName.text = spells[i].spellName;
                    listing.spell = spells[i];
                    listing.spellCaster = this;
                }
            }
        }
        else
            noSpells.SetActive(true);

        CheckCosts();
    }

    /// <summary>
    /// Clears all spell buttons from UI display
    /// </summary>
    private void ClearSpellButtons()
    {
        for (int i = 0; i < spellButtons.Count; i++)
        {
            Destroy(spellButtons[i]);
        }

        spellButtons.Clear();
    }

    /// <summary>
    /// Function called by sort button on UI display
    /// </summary>
    private void SortSpells()
    {
        ClearSpellButtons();
        PopulateSpellList(spellsStandard);
        CheckCosts();

        
        List<SpellSO> castableSpells = new List<SpellSO>();

        for (int i = 0; i < spellButtons.Count; i++)
        {
            if (spellButtons[i].GetComponent<Button>().interactable)
                castableSpells.Add(spellButtons[i].GetComponent<SpellListingOffline>().spell);
        }

        SpellSO[] spellsSorted = new SpellSO[castableSpells.Count];

        for (int i = 0; i < castableSpells.Count; i++)
        {
            spellsSorted[i] = castableSpells[i];
        }

        ClearSpellButtons();
        PopulateSpellList(spellsSorted);
    }

    /// <summary>
    /// Sets up animation dictionary using animations list
    /// </summary>
    private void PopulateAnimationDictionary()
    {
        foreach(GameObject anim in animations)
        {
            animationDict.Add(anim.name, anim);
        }
    }

    /// <summary>
    /// Runs the CheckCost function on each spell listing to make that spell button interactable if we have enough mana or not interactable if we don't
    /// </summary>
    public void CheckCosts()
    {
        foreach (GameObject button in spellButtons)
        {
            button.GetComponent<SpellListingOffline>().CheckCost();
        }
    }

    /// <summary>
    /// Main function for submitting a spell to be played and runs all appropriate damage and effect calculationas as well as triggers animations - Takes the power of the spell and the spell scriptable object
    /// </summary>
    /// <param name="power"></param>
    /// <param name="spell"></param>
    public void SubmitAttack(int power, SpellSO spell, bool AI)
    {
        int attack = statsManager.myAttack;
        int defense = statsManager.theirDefense;

        //Determine environment modifier
        float environment = 1;

        if (spell.color == AreaColorManagerOffline.instance.currentAreaColor)
        {
            environment = 2;

            if (!spell.eatMana && spell.spellName != "Antidote")
                gameManager.DisplayMessage("It was 2x as effective due to spell color matching field color");
        }
        else if (oppositeDict.ContainsKey(spell.color))
        {
            if (oppositeDict[spell.color] == AreaColorManagerOffline.instance.currentAreaColor)
            {
                environment = 0.5f;
                if (!spell.eatMana && spell.spellName != "Antidote")
                    gameManager.DisplayMessage("It was half as effective becuase it is the opposite color of the field color");
            }
        }
        else environment = 1;

        float modifier = environment;

        if (AI)
        {
            attack = statsManager.theirAttack;
            defense = statsManager.myDefense;
        }

        if (spell.counter) //Check if the spell that was cast is a counter
        {
            if (!AI)
            {
                if (!statsManager.myCounter)
                {
                    if (spell.spellName == "Mirror Shield")
                    {
                        myMirror = true;
                        gameManager.DisplayMessage("You create a reflective shield");
                    }
                    else
                        gameManager.DisplayMessage("You prepare to counter the next attack");

                    statsManager.myCounter = true;
                    
                    PayMana(spell, AI);
                }
                else
                {
                    gameManager.DisplayMessage("You are already prepared to counter");
                }

            }
            else
            {
                if (!statsManager.theirCounter)
                {
                    if (spell.spellName == "Mirror Shield")
                        theirMirror = true;

                    statsManager.theirCounter = true;
                    PayMana(spell, AI);
                }

            }
        }
        else  //Otherwise it is not a counter spell
        {
            soundManager.RPC_PlaySinglePublic(spell.spellName, 1);

            if (!AI)
                gameManager.DisplayMessage(statsManager.myName.text + " used " + spell.spellName);
            else
            {
                if(spell.eatMana)
                {
                    if(theirTerraScript.target.tag != "Dirt")
                        gameManager.DisplayMessage(statsManager.theirName.text + " used " + spell.spellName);
                }
                else
                {
                    gameManager.DisplayMessage(statsManager.theirName.text + " used " + spell.spellName);
                }
            }

            //Main damage formula
            float damagePreRounding = ((22 * power * attack / defense/ 50)) * modifier;

            if (spell.category == "Attack")
                damagePreRounding++;

            int damage = Mathf.RoundToInt(damagePreRounding);

            //Check if the spell is a mana consumption spell
            if (spell.eatMana)
            {
                if (!AI)
                {
                    if (myTerraScript.target.tag != "Dirt" && myTerraScript.target != null)
                    {
                        if ((myTerraScript.currentMana == 8 && myTerraScript.target.tag == "Death") || (myTerraScript.currentMana == 8 && myTerraScript.target.tag == "Life"))
                        {
                            gameManager.DisplayMessage("Your body cannot hold any more mana. You must cast a spell to make room.");
                        }
                        else
                        {
                            PayMana(spell, AI);
                            FactorEffects(spell, modifier, AI);
                            StartCoroutine(DamageRoutine(damage, spell, AI));
                            colorManager.Cycle(spell.color);
                        }
                    }
                    else
                    {
                        gameManager.DisplayMessage("Nothing here.");
                    }

                }
                else
                {
                    if (spell.eatMana && theirTerraScript.target.tag != "Dirt" && theirTerraScript.currentMana <= 6 && theirTerraScript.target != null)
                    {
                        PayMana(spell, AI);
                        FactorEffects(spell, modifier, AI);
                        StartCoroutine(DamageRoutine(damage, spell, AI));
                        colorManager.Cycle(spell.color);
                    }
                }
            }
            else
            {
                FactorEffects(spell, modifier, AI);
                StartCoroutine(DamageRoutine(damage, spell, AI));
                PayMana(spell, AI);
                colorManager.Cycle(spell.color);
            }
        }

        if (sorted)
            SortButtonSorted();
        CheckCosts(); //Check which spells we can afford after casting the last spell
    }

    /// <summary>
    /// Run for each spell cast to trigger any effects outside of dealing damage
    /// </summary>
    private void FactorEffects(SpellSO spell, float modifier, bool AI)
    {
        //Screen Shake
        if (spell.screenShake)
            gameManager.CameraShake();

        //DoT
        if (!AI)
        {
            if (!theirPlayerControl.poisoned)
                StartCoroutine(CheckDot(spell.poisionChance * (int)modifier, "Poison", AI, spell));
            if (!theirPlayerControl.burnt)
                StartCoroutine(CheckDot(spell.burnChance * (int)modifier, "Burn", AI, spell));
        }
        else
        {
            if (!myPlayerControl.poisoned)
                StartCoroutine(CheckDot(spell.poisionChance * (int)modifier, "Poison", AI, spell));
            if (!myPlayerControl.burnt)
                StartCoroutine(CheckDot(spell.burnChance * (int)modifier, "Burn", AI, spell));
        }

        //Remove DoT
        if (spell.removeDot)
        {
            if (!AI)
            {
                myPlayerControl.poisoned = false;
                myPlayerControl.burnt = false;
                statsManager.myPsnObj.SetActive(false);
                statsManager.myBrnObj.SetActive(false);
                gameManager.DisplayMessage("You cured yourself of poison and burn");
            }
            else
            {
                theirPlayerControl.poisoned = false;
                theirPlayerControl.burnt = false;
                statsManager.theirPsnObj.SetActive(false);
                statsManager.theirBrnObj.SetActive(false);
                gameManager.DisplayMessage("Opponent cured themself of poison and burn");
            }
        }

        //Heal
        if (!AI)
            statsManager.MyHealth(Mathf.RoundToInt(spell.healAmount * modifier), false);
        else
            statsManager.TheirHealth(Mathf.RoundToInt(spell.healAmount * modifier), false);

        //Spirit Walk
        if (spell.spiritWalk)
            gameManager.playerControl.SpiritWalk();

        //Eat Mana
        if(spell.eatMana)
        {
            if (!AI && spell.eatMana && myTerraScript.target.tag != "Dirt" && myTerraScript.target != null)
                gameManager.terraScript.Take(true);
            else if(spell.eatMana && theirTerraScript.target.tag != "Dirt" && theirTerraScript.target != null)
                gameManager.theirPlayer.GetComponent<TerraformOffline>().Take(true);
        }

        

        //My Stats
        if (!AI)
        {
            if (spell.myAttkInt != 0)
                statsManager.MyStat("Attack", Mathf.RoundToInt(spell.myAttkInt * modifier));
            if (spell.myDefInt != 0)
                statsManager.MyStat("Defense", Mathf.RoundToInt(spell.myDefInt * modifier));
        }
        else
        {
            if (spell.myAttkInt != 0)
                statsManager.TheirStat("Attack", Mathf.RoundToInt(spell.myAttkInt * modifier));
            if (spell.myDefInt != 0)
                statsManager.TheirStat("Defense", Mathf.RoundToInt(spell.myDefInt * modifier));
        }


        //Their Stats
        if (!AI)
        {
            if (spell.theirAttkInt != 0)
                statsManager.TheirStat("Attack", Mathf.RoundToInt(spell.theirAttkInt * modifier));
            if (spell.theirDefInt != 0)
                statsManager.TheirStat("Defense", Mathf.RoundToInt(spell.theirDefInt * modifier));
        }
        else
        {
            if (spell.theirAttkInt != 0)
                statsManager.MyStat("Attack", Mathf.RoundToInt(spell.theirAttkInt * modifier));
            if (spell.theirDefInt != 0)
                statsManager.MyStat("Defense", Mathf.RoundToInt(spell.theirDefInt * modifier));
        }
    }

    /// <summary>
    /// After damage amount is calculated this routine triggers animations and lowers health of damaged opponent on screen - Player feedback portion of spell casting
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="spell"></param>
    public IEnumerator DamageRoutine(int damage, SpellSO spell, bool AI)
    {
        yield return new WaitForSeconds(0.3f);

        soundManager.RPC_PlaySinglePublic("preCastSound", 0.7f);
        MyLocationEffect(spell.color + "Pre", AI);

        yield return new WaitForSeconds(1.6f);

		if (spell.hasProjectile)
		{
			LaunchProjectile(spell.projectile, AI);
		}

		if (spell.hasTheirLocationEffect)
		{
			TheirLocationEffect(spell.theirLocationEffect, AI);
        }

		if (spell.hasMyLocationEffect)
		{
			MyLocationEffect(spell.myLocationEffect, AI);
        }

		yield return new WaitForSeconds(2f);
        if(!AI)
		    statsManager.TheirHealth(damage * -1, false);
        else
            statsManager.MyHealth(damage * -1, false);

        yield return new WaitForSeconds(1f);
        if (spell.stealHealth)
        {
            if(!AI)
                statsManager.MyHealth(damage, false);
            else
                statsManager.TheirHealth(damage, false);
        }
    }

    /// <summary>
    /// Takes in a spell and subtracts the appropriate amount of mana for that spells cost - AI bool determines whether to do this for human player or AI palyer
    /// </summary>
    /// <param name="spell"></param>
    /// <param name="AI"></param>
    private void PayMana(SpellSO spell, bool AI)
	{
        if (!AI)
        {
            myTerraScript.waterCount -= spell.blueCost;
            myTerraScript.plantCount -= spell.greenCost;
            myTerraScript.fireCount -= spell.redCost;
            myTerraScript.rockCount -= spell.yellowCost;
            myTerraScript.lifeCount -= spell.whiteCost;
            myTerraScript.deathCount -= spell.blackCost;
            myTerraScript.currentMana -= spell.totalcost;
            myTerraScript.CheckMana();
            gameManager.UpdateElements();
        }
        else
        {
            theirTerraScript.waterCount -= spell.blueCost;
            theirTerraScript.plantCount -= spell.greenCost;
            theirTerraScript.fireCount -= spell.redCost;
            theirTerraScript.rockCount -= spell.yellowCost;
            theirTerraScript.lifeCount -= spell.whiteCost;
            theirTerraScript.deathCount -= spell.blackCost;
            theirTerraScript.currentMana -= spell.totalcost;
        }
	}

    /// <summary>
    /// Rolls to see if the spell successfully poisoned or butned the player
    /// </summary>
    /// <param name="chance"></param>
    /// <param name="type"></param>
    /// <param name="hitMe"></param>
    /// <param name="spell"></param>
    /// <returns></returns>
    private IEnumerator CheckDot(int chance, string type, bool AI, SpellSO spell)
    {
        yield return new WaitForSeconds(1);

        int roll = Random.Range(1, 100);
        if(roll <= chance)
        {
            if (!AI)
            {
                if(!statsManager.theirCounter || spell.spellName == "Mikor Curse")
                    PoisonBurnThem(type);
                else
                    PoisonBurnMe(type);
            }
            else
            {
                if(!statsManager.theirCounter || spell.spellName == "Mikor Curse")
                    PoisonBurnMe(type);
                else
                    PoisonBurnThem(type);
            }
        }
    }

    private void PoisonBurnMe(string type)
    {
        if (type == "Poison" && myPlayerControl.poisoned == false)
        {
            myPlayerControl.poisoned = true;
            statsManager.myPsnObj.SetActive(true);
            gameManager.DisplayMessage("You were poisoned");
        }
        if (type == "Burn" && myPlayerControl.burnt == false)
        {
            myPlayerControl.burnt = true;
            statsManager.myBrnObj.SetActive(true);
            gameManager.DisplayMessage("You were burned");
        }
    }

    private void PoisonBurnThem(string type)
    {
        if (type == "Poison" && theirPlayerControl.poisoned == false)
        {
            theirPlayerControl.poisoned = true;
            statsManager.theirPsnObj.SetActive(true);
            gameManager.DisplayMessage("Opponent was poisoned");
        }
        if (type == "Burn" && theirPlayerControl.burnt == false)
        {
            theirPlayerControl.burnt = true;
            statsManager.theirBrnObj.SetActive(true);
            gameManager.DisplayMessage("Opponent was burned");
        }
    }

    /// <summary>
    /// Run whenever a counter is triggered
    /// </summary>
    /// <param name="AI"></param>
    public void Counter(bool AI)
    {
        if (AI)
        {
            if (theirMirror)
            {
                gameManager.DisplayMessage("But " + statsManager.theirName.text + " used Mirror Shield");

                LaunchProjectile("MirrorProject", AI);
                TheirLocationEffect("MirrorSelf", AI);
                colorManager.Cycle("Yellow");
            }
            else
            {
                gameManager.DisplayMessage("But " + statsManager.theirName.text + " countered!");

                MyLocationEffect("CounterMe", AI);
                TheirLocationEffect("CounterThem", AI);
                colorManager.Cycle("Blue");
            }
        }
        else
        {
            if (myMirror)
            {
                gameManager.DisplayMessage("But " + statsManager.myName.text + " used Mirror Shield");

                LaunchProjectile("MirrorProject", AI);
                MyLocationEffect("MirrorSelf", AI);
                colorManager.Cycle("Yellow");
            }
            else
            {
                gameManager.DisplayMessage("But " + statsManager.myName.text + " countered!");

                MyLocationEffect("CounterThem", AI);
                TheirLocationEffect("CounterMe", AI);
                colorManager.Cycle("Blue");
            }
        }

        soundManager.RPC_PlaySinglePublic("counterSound", 1.2f);
        

    }

    /// <summary>
    /// Sets appropriate stat and end locations for projectile
    /// </summary>
    /// <param name="_projectile"></param>
    /// <param name="AI"></param>
    public void LaunchProjectile(string _projectile, bool AI)
    {
        if (_projectile == "Reap" || _projectile == "HarvestSoul")
        {
            if (!AI)
            {
                projectileStart = gameManager.theirPlayer.transform.position;
                projectileEnd = gameManager.myPlayer.transform.position;
            }
            else
            {
                projectileStart = gameManager.myPlayer.transform.position;
                projectileEnd = gameManager.theirPlayer.transform.position;
            }
        }
        else
        {
            if (!AI)
            {
                projectileStart = gameManager.myPlayer.transform.position;
                projectileEnd = gameManager.theirPlayer.transform.position;
            }
            else
            {
                projectileStart = gameManager.theirPlayer.transform.position;
                projectileEnd = gameManager.myPlayer.transform.position;
            }
        }

        StartCoroutine(Projectile(projectileStart, projectileEnd, _projectile, AI));
    }

    /// <summary>
    /// Launches a _projectile prefab starting at specified start location that ends at specified end location 
    /// </summary>
    /// <param name="_projectileStart"></param>
    /// <param name="_projectileEnd"></param>
    /// <param name="_projectile"></param>
    /// <param name="AI"></param>
    /// <returns></returns>
    public IEnumerator Projectile(Vector3 _projectileStart, Vector3 _projectileEnd, string _projectile, bool AI)
    {
        if (!AI)
        {
            myPlayerControl.canMove = false;
            myTerraScript.Cast();
        }
        else
        {
            theirPlayerControl.canMove = false;
            theirTerraScript.Cast();
        }

        yield return new WaitForSeconds(1);

        GameObject temp = Instantiate(animationDict[_projectile], _projectileStart, Quaternion.identity);
        temp.GetComponent<Projectile>().projectileStart = _projectileStart;
        temp.GetComponent<Projectile>().projectileEnd = _projectileEnd;
        temp.transform.LookAt(_projectileEnd);
    }

    /// <summary>
    /// Plays spell effect animation (specified by effect string) at opponents location
    /// </summary>
    /// <param name="effect"></param>
    public void TheirLocationEffect(string effect, bool AI)
    {
        if(!AI)
            targetPos = gameManager.theirPlayer.transform.position;
        else
            targetPos = gameManager.myPlayer.transform.position;

        StartCoroutine(StationaryEffect(effect, AI));
    }

    /// <summary>
    /// Plays spell effect animation (specified by effect string) at local players location - sends a message to mimic this on opponents screen
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="AI"></param>
    public void MyLocationEffect(string effect, bool AI)
    {
        if (!AI)
            targetPos = gameManager.myPlayer.transform.position;
        else
            targetPos = gameManager.theirPlayer.transform.position;

        StartCoroutine(StationaryEffect(effect, AI));
    }

    /// <summary>
    /// Plays an animation in at a target location
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="AI"></param>
    /// <returns></returns>
    public IEnumerator StationaryEffect(string effect, bool AI)
    {
        if (!AI)
        {
            myPlayerControl.canMove = false;

            yield return new WaitForSeconds(1);

            if(!effect.Contains("Pre"))
                myTerraScript.Cast(); //a function will need to be created so both players see this happen
            Instantiate(animationDict[effect], targetPos, Quaternion.identity);
        }
        else
        {
            theirPlayerControl.canMove = false;

            yield return new WaitForSeconds(1);
            if (!effect.Contains("Pre"))
                theirTerraScript.Cast(); //a function will need to be created so both players see this happen
            Instantiate(animationDict[effect], targetPos, Quaternion.identity);
        }
    }

}
