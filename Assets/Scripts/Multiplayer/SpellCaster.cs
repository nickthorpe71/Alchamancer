using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// Multiplayer Version - Handles calculating damage, effects and animations for all spells
/// </summary>
public class SpellCaster : MonoBehaviourPunCallbacks
{
    [Header("Management")]
    public AreaColorManager colorManager;
    public Terraform terraScript;
    private GameManager gameManager;
    private SoundManager soundManager;
    private StatsManager statsManager;
    private PlayerControl myPlayerControl;
    private PlayerControl theirPlayerControl;

    [Header("UI")]
    public GameObject noSpells; //Message to state that there are no spells available
    public bool sorted; //Bool indicating whether spells have been sorted

    [Header("Projectiles")]
    private Vector3 projectileStart;
    private Vector3 projectileEnd;

    [Header("Stationary")]
    private Vector3 targetPos;

    [Header("Spells")]
    public SpellListSO spellLibrary; //Scriptable object holding all spell scriptable objects
    public SpellSO[] spellsStandard; //Array of standard spell list
    public SpellListing spellListing; //Prefab for a spell listing button
    public Transform content; //Area where spell listings will be displayed
    public List<GameObject> spellButtons = new List<GameObject>(); //List of all spell listing buttons
    public List<GameObject> animations = new List<GameObject>(); //List of all spell animations
    public Dictionary<string, GameObject> animationDict = new Dictionary<string, GameObject>(); //Dictionary where the key is the animation name and value is the corrisponding animation
    public Dictionary<string, string> oppositeDict = new Dictionary<string, string>(); //Dictionary for specifying each element's opposite
    public bool mirror;

    private void Start()
    {
        gameManager = GameManager.instance;
        soundManager = SoundManager.instance;

        PopulateOppositeDict();
        PopulateAnimationDictionary();

        if (!SaveLoad.instance.tournamentHost)
        {
            Invoke("DelayedStart", 8); //Assign scrpts that may be generated a few seconds after atart of match

            statsManager = StatsManager.instance;

            spellsStandard = spellLibrary.spells;

            PopulateSpellList(spellsStandard);
            CheckCosts();
        }
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
        terraScript = gameManager.myPlayer.GetComponent<Terraform>();
        myPlayerControl = gameManager.playerControl;
        theirPlayerControl = gameManager.theirPlayer.GetComponent<PlayerControl>();
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
                SpellListing listing = Instantiate(spellListing, content);

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
                castableSpells.Add(spellButtons[i].GetComponent<SpellListing>().spell);
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
        foreach (GameObject anim in animations)
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
            button.GetComponent<SpellListing>().CheckCost();
        }
    }

    /// <summary>
    /// Main function for submitting a spell to be played and runs all appropriate damage and effect calculationas as well as triggers animations - Takes the power of the spell and the spell scriptable object
    /// </summary>
    /// <param name="power"></param>
    /// <param name="spell"></param>
    public void SubmitAttack(int power, SpellSO spell)
    {
        int attack = statsManager.myAttack;
        int defense = statsManager.theirDefense;

        if (spell.counter) //Check if the spell that was cast is a counter
        {
            if (!statsManager.myCounter)
            {
                if (spell.spellName == "Mirror Shield")
                {
                    mirror = true;
                    gameManager.DisplayMessage("You create a reflective shield");
                }
                else
                    gameManager.DisplayMessage("You prepare to counter the next attack");

                statsManager.myCounter = true;
                PayMana(spell);
                base.photonView.RPC("RPC_Counter", RpcTarget.Others);
            }
            else
            {
                gameManager.DisplayMessage("You are already prepared to counter");
            }
        }
        else //Otherwise it is not a counter spell
        {
            soundManager.PlaySinglePublic(spell.spellName, 1);

            float environment = 1;

            gameManager.DisplayPublicMessage(gameManager.playerControl.screenName + " used " + spell.spellName);


            //Determine environment modifier
            if (spell.color == AreaColorManager.instance.currentAreaColor)
            {
                environment = 2;

                if(!spell.eatMana && spell.spellName != "Antidote")
                    gameManager.DisplayPublicMessage("It was 2x as effective due to spell color matching field color");
            }
            else if (oppositeDict.ContainsKey(spell.color))
            {
                if (oppositeDict[spell.color] == AreaColorManager.instance.currentAreaColor)
                {
                    environment = 0.5f;
                    if (!spell.eatMana && spell.spellName != "Antidote")
                        gameManager.DisplayPublicMessage("It was half as effective becuase it is the opposite color of the field color");
                }
            }
            else environment = 1;

            //Main damage formula
            float modifier = environment;
            float damagePreRounding = ((22 * power * attack / defense / 50)) * modifier; 

            if (spell.category == "Attack")
                damagePreRounding++;

            int damage = Mathf.RoundToInt(damagePreRounding);

            //Check if the spell is a mana consumption spell
            if (spell.eatMana)
            {
                if (terraScript.target.tag != "Dirt" && terraScript.target != null)
                {
                    if ((terraScript.currentMana == 8 && terraScript.target.tag == "Death") || (terraScript.currentMana == 8 && terraScript.target.tag == "Life"))
                    {
                        gameManager.DisplayMessage("Your body cannot hold any more mana. You must cast a spell to make room.");
                    }
                    else
                    {
                        PayMana(spell);
                        FactorEffects(spell, modifier);
                        StartCoroutine(DamageRoutine(damage, spell));
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
                FactorEffects(spell, modifier);
                StartCoroutine(DamageRoutine(damage, spell));
                PayMana(spell);
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
    /// <param name="spell"></param>
    /// <param name="modifier"></param>
    private void FactorEffects(SpellSO spell, float modifier)
    {
        //Screen Shake
        if (spell.screenShake)
            gameManager.CameraShake();

        //DoT
        if (!statsManager.theirCounter || spell.spellName == "Mikor Curse")
        {
            if (!theirPlayerControl.poisoned)
                StartCoroutine(CheckDot(spell.poisionChance * (int)modifier, "Poison", false, spell));
            if (!theirPlayerControl.burnt)
                StartCoroutine(CheckDot(spell.burnChance * (int)modifier, "Burn", false, spell));
        }
        else
        {
            if (!myPlayerControl.poisoned)
                StartCoroutine(CheckDot(spell.poisionChance * (int)modifier, "Poison", true, spell));
            if (!myPlayerControl.burnt)
                StartCoroutine(CheckDot(spell.burnChance * (int)modifier, "Burn", true, spell));
        }

        //Remove DoT
        if (spell.removeDot)
        {
            myPlayerControl.poisoned = false;
            myPlayerControl.burnt = false;
            statsManager.myPsnObj.SetActive(false);
            statsManager.myBrnObj.SetActive(false);
            TournamentHost.instance.SetPsnBrn(statsManager.myName.text, "psn", false);
            TournamentHost.instance.SetPsnBrn(statsManager.myName.text, "brn", false);
            base.photonView.RPC("RPC_Antidote", RpcTarget.Others, statsManager.myName.text);
        }

        //Heal
        statsManager.MyHealth(Mathf.RoundToInt(spell.healAmount * modifier), false);

        //Spirit Walk
        if (spell.spiritWalk)
            gameManager.playerControl.SpiritWalk();

        //Eat Mana
        if (spell.eatMana && terraScript.target.tag != "Dirt" && terraScript.target != null)
        {
            gameManager.terraScript.Take(true);
        }

        //My Stats
        if (spell.myAttkInt != 0)
            statsManager.MyStat("Attack", Mathf.RoundToInt(spell.myAttkInt * modifier));
        if (spell.myDefInt != 0)
            statsManager.MyStat("Defense", Mathf.RoundToInt(spell.myDefInt * modifier));

        //Their Stats
        if (spell.theirAttkInt != 0)
            statsManager.TheirStat("Attack", Mathf.RoundToInt(spell.theirAttkInt * modifier));
        if (spell.theirDefInt != 0)
            statsManager.TheirStat("Defense", Mathf.RoundToInt(spell.theirDefInt * modifier));
    }

    /// <summary>
    /// After damage amount is calculated this routine triggers animations and lowers health of damaged opponent on screen - Player feedback portion of spell casting
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="spell"></param>
    public IEnumerator DamageRoutine(int damage, SpellSO spell)
    {
        yield return new WaitForSeconds(0.3f);

        soundManager.PlaySinglePublic("preCastSound", 0.7f);
        MyLocationEffect(spell.color + "Pre");

        yield return new WaitForSeconds(1.6f);

        if (spell.hasProjectile)
        {
            LaunchProjectile(spell.projectile);
        }

        if (spell.hasTheirLocationEffect)
        {
            TheirLocationEffect(spell.theirLocationEffect);
        }

        if (spell.hasMyLocationEffect)
        {
            MyLocationEffect(spell.myLocationEffect);
        }

        yield return new WaitForSeconds(2f);
        statsManager.TheirHealth(damage * -1, false);

        yield return new WaitForSeconds(1f);
        if (spell.stealHealth)
            statsManager.MyHealth(damage, false);
    }

    /// <summary>
    /// Takes in a spell and subtracts the appropriate amount of mana for that spells cost
    /// </summary>
    /// <param name="spell"></param>
    /// <param name="AI"></param>
    private void PayMana(SpellSO spell)
    {
        terraScript.waterCount -= spell.blueCost;
        terraScript.plantCount -= spell.greenCost;
        terraScript.fireCount -= spell.redCost;
        terraScript.rockCount -= spell.yellowCost;
        terraScript.lifeCount -= spell.whiteCost;
        terraScript.deathCount -= spell.blackCost;
        terraScript.currentMana -= spell.totalcost;
        terraScript.CheckMana();
        gameManager.UpdateElements();

    }

    /// <summary>
    /// Rolls to see if the spell successfully poisoned or butned the player
    /// </summary>
    /// <param name="chance"></param>
    /// <param name="type"></param>
    /// <param name="hitMe"></param>
    /// <param name="spell"></param>
    /// <returns></returns>
    private IEnumerator CheckDot(int chance, string type, bool hitMe, SpellSO spell)
    {
        yield return new WaitForSeconds(1);

        int roll = Random.Range(1, 100);
        if (roll <= chance)
        {
            if (!statsManager.theirCounter || spell.spellName == "Mikor Curse")
            {
                if (type == "Poison" && theirPlayerControl.poisoned == false)
                {
                    theirPlayerControl.poisoned = true;
                    statsManager.theirPsnObj.SetActive(true);
                    TournamentHost.instance.SetPsnBrn(statsManager.theirName.text, "psn", true);
                    base.photonView.RPC("RPC_Poison", RpcTarget.Others, true, statsManager.theirName.text);
                }
                if (type == "Burn" && theirPlayerControl.burnt == false)
                {
                    theirPlayerControl.burnt = true;
                    statsManager.theirBrnObj.SetActive(true);
                    TournamentHost.instance.SetPsnBrn(statsManager.theirName.text, "brn", true);
                    base.photonView.RPC("RPC_Burn", RpcTarget.Others, true, statsManager.theirName.text);
                }
            }
            else
            {
                if (type == "Poison" && myPlayerControl.poisoned == false)
                {
                    myPlayerControl.poisoned = true;
                    statsManager.myPsnObj.SetActive(true);
                    TournamentHost.instance.SetPsnBrn(statsManager.myName.text, "psn", true);
                    base.photonView.RPC("RPC_Poison", RpcTarget.Others, false, statsManager.myName.text);
                }
                if (type == "Burn" && myPlayerControl.burnt == false)
                {
                    myPlayerControl.burnt = true;
                    statsManager.myBrnObj.SetActive(true);
                    TournamentHost.instance.SetPsnBrn(statsManager.myName.text, "brn", true);
                    base.photonView.RPC("RPC_Burn", RpcTarget.Others, false, statsManager.myName.text);
                }
            }
        }
    }

    /// <summary>
    /// Run whenever a counter is triggered
    /// </summary>
    /// <param name="AI"></param>
    public void Counter()
    {
        if (!mirror)
        {
            gameManager.DisplayPublicMessage("But " + theirPlayerControl.screenName + " countered!");
            colorManager.Cycle("Blue");
            MyLocationEffect("CounterMe");
            TheirLocationEffect("CounterThem");
            soundManager.PlaySinglePublic("counterSound", 1.2f);
        }
        else
        {
            gameManager.DisplayPublicMessage("But " + theirPlayerControl.screenName + " used Mirror Shield");
            colorManager.Cycle("Yellow");
            LaunchProjectile("MirrorProject");
            MyLocationEffect("MirrorSelf");
            soundManager.PlaySinglePublic("counterSound", 1.2f);
            mirror = false;
        }
    }

    /// <summary>
    /// Receives a message that the opponent has just cast counter
    /// </summary>
    [PunRPC]
    public void RPC_Counter()
    {
        if(!SaveLoad.instance.tournamentHost)
            statsManager.theirCounter = true;
    }

    /// <summary>
    /// receives a message that a player (specified by name) has just used antidote
    /// </summary>
    /// <param name="name"></param>
    [PunRPC]
    private void RPC_Antidote(string name)
    {
        gameManager.DisplayPublicMessage(name + " cured themselves of poison and burn.");

        if (!SaveLoad.instance.tournamentHost)
        {
            theirPlayerControl.poisoned = false;
            theirPlayerControl.burnt = false;
            statsManager.theirPsnObj.SetActive(false);
            statsManager.theirBrnObj.SetActive(false);
        }
    }

    /// <summary>
    /// Receives a message that a player (specified by name) has been poisoned
    /// </summary>
    /// <param name="hitMe"></param>
    /// <param name="name"></param>
    [PunRPC]
    private void RPC_Poison(bool hitMe, string name)
    {
        if (!SaveLoad.instance.tournamentHost)
        {
            if (hitMe)
            {
                myPlayerControl.poisoned = true;
                statsManager.myPsnObj.SetActive(true);
                TournamentHost.instance.SetPsnBrn(statsManager.myName.text, "psn", true);
            }
            else
            {
                gameManager.DisplayPublicMessage(name + " was poisoned");
                theirPlayerControl.poisoned = true;
                statsManager.theirPsnObj.SetActive(true);
                TournamentHost.instance.SetPsnBrn(statsManager.theirName.text, "psn", true);
            }
        }
    }

    /// <summary>
    /// Receives a message that a player (specified by name) has been burned
    /// </summary>
    /// <param name="hitMe"></param>
    /// <param name="name"></param>
    [PunRPC]
    private void RPC_Burn(bool hitMe, string name)
    {
        if (!SaveLoad.instance.tournamentHost)
        {
            if (hitMe)
            {
                myPlayerControl.burnt = true;
                statsManager.myBrnObj.SetActive(true);
                TournamentHost.instance.SetPsnBrn(statsManager.myName.text, "brn", true);
            }
            else
            {
                gameManager.DisplayPublicMessage(name + " was burned");
                theirPlayerControl.burnt = true;
                statsManager.theirBrnObj.SetActive(true);
                TournamentHost.instance.SetPsnBrn(statsManager.theirName.text, "brn", true);
            }
        }
    }
    /// <summary>
    /// Sets appropriate start and end positions for the projectile and sends message mimic this projectile on opponents screen
    /// </summary>
    /// <param name="_projectile"></param>
    public void LaunchProjectile(string _projectile)
    {
        if (_projectile == "Reap" || _projectile == "HarvestSoul")
        {
            projectileStart = gameManager.theirPlayer.transform.position;
            projectileEnd = gameManager.myPlayer.transform.position;
        }
        else
        {
            projectileStart = gameManager.myPlayer.transform.position;
            projectileEnd = gameManager.theirPlayer.transform.position;
        }

        StartCoroutine(Projectile(projectileStart, projectileEnd, _projectile));

        base.photonView.RPC("RPC_ThemToMe", RpcTarget.Others, _projectile);
    }


    /// <summary>
    /// Launches a _projectile prefab starting at specified start location that ends at specified end location
    /// </summary>
    /// <param name="_projectileStart"></param>
    /// <param name="_projectileEnd"></param>
    /// <param name="_projectile"></param>
    /// <returns></returns>
    public IEnumerator Projectile(Vector3 _projectileStart, Vector3 _projectileEnd, string _projectile)
    {
        myPlayerControl.canMove = false;

        yield return new WaitForSeconds(1);

        terraScript.Cast();
        GameObject temp = Instantiate(animationDict[_projectile], _projectileStart, Quaternion.identity);
        temp.GetComponent<Projectile>().projectileStart = _projectileStart;
        temp.GetComponent<Projectile>().projectileEnd = _projectileEnd;
        temp.transform.LookAt(_projectileEnd);

        TournamentHost.instance.SubmitAttackProjectile(_projectileStart, _projectileEnd, _projectile);
    }

    /// <summary>
    /// Receives a message to leaunch a projectile form opponents player object to my player object
    /// </summary>
    /// <param name="_projectile"></param>
    [PunRPC]
    public void RPC_ThemToMe(string _projectile)
    {
        if (!SaveLoad.instance.tournamentHost)
        {
            projectileStart = gameManager.theirPlayer.transform.position;
            projectileEnd = gameManager.myPlayer.transform.position;
            StartCoroutine(Projectile(projectileStart, projectileEnd, _projectile));
        }
    }

    /// <summary>
    /// Plays spell effect animation (specified by effect string) at opponents location - sends a message to mimic this on opponents screen
    /// </summary>
    /// <param name="effect"></param>
    public void TheirLocationEffect(string effect)
    {
        targetPos = gameManager.theirPlayer.transform.position;

        StartCoroutine(StationaryEffect(effect));

        base.photonView.RPC("RPC_TheirLocationEffect", RpcTarget.Others, effect);
    }

    /// <summary>
    /// Receives a message to play specified animation at local players location
    /// </summary>
    /// <param name="effect"></param>
    [PunRPC]
    public void RPC_TheirLocationEffect(string effect)
    {
        if (!SaveLoad.instance.tournamentHost)
        {
            targetPos = gameManager.myPlayer.transform.position;

            StartCoroutine(StationaryEffect(effect));
        }
    }

    /// <summary>
    /// Plays spell effect animation (specified by effect string) at local players location - sends a message to mimic this on opponents screen
    /// </summary>
    /// <param name="effect"></param>
    public void MyLocationEffect(string effect)
    {
        targetPos = gameManager.myPlayer.transform.position;

        StartCoroutine(StationaryEffect(effect));

        base.photonView.RPC("RPC_MyLocationEffect", RpcTarget.Others, effect);
    }

    /// <summary>
    /// Receives a message to play specified animation at opponents location
    /// </summary>
    /// <param name="effect"></param>
    [PunRPC]
    public void RPC_MyLocationEffect(string effect)
    {
        if (!SaveLoad.instance.tournamentHost)
        {
            targetPos = gameManager.theirPlayer.transform.position;

            StartCoroutine(StationaryEffect(effect));
        }
    }

    /// <summary>
    /// Plays an animation in at a target location
    /// </summary>
    /// <param name="effect"></param>
    /// <returns></returns>
    public IEnumerator StationaryEffect(string effect)
    {
        myPlayerControl.canMove = false;

        yield return new WaitForSeconds(1);

        if (!effect.Contains("Pre"))
            terraScript.Cast();
        Instantiate(animationDict[effect], targetPos, Quaternion.identity);

        TournamentHost.instance.SubmitAttackLocation(targetPos, effect); //make sure this onyl triggers once per player
    }

}
