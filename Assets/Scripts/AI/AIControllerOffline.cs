using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls AI in single player games
/// </summary>
public class AIControllerOffline : MonoBehaviour
{
    //All external scripts that need to be referenced
    private PlayerControlOffline playerControl;
    private TerraformOffline terraScript;
    private GameManagerOffline gameManager;
    private CellManagerOffline cellManager;
    private SpellCasterOffline spellCaster;
    private StatsManagerOffline statsManager;

    public bool moveComplete = true;

    //Set the base time between the AI's actions
    private float timeBetweenActions = 0.25f;

    //List of all spells
    public Dictionary<string, SpellSO> spellDict = new Dictionary<string, SpellSO>();
    //List of spells that AI has enough mana for
    public List<string> castableSpells = new List<string>();

    
    void Start()
    {
        //Set all external script objects.
        gameManager = GameManagerOffline.instance;
        cellManager = CellManagerOffline.instance;
        spellCaster = cellManager.spellCaster;
        statsManager = StatsManagerOffline.instance;
        terraScript = GetComponent<TerraformOffline>();
        playerControl = GetComponent<PlayerControlOffline>();

        PopulateSpellDict();

        //Initiate turn check function
        InvokeRepeating("CheckTurn", 1, 0.25f);

    }

    void PopulateSpellDict()
    {
        //Populate a dictionary of spells using the spell list in spell caster script
        foreach (SpellSO spell in spellCaster.spellsStandard)
        {
            spellDict.Add(spell.spellName, spell);
        }
    }

    void CheckTurn()
    {
        // Checks if it's the player's turn or AI's turn
        //myTurn = true means non AI player's turn
        //If not players turn then start the AI's turn
        if (!gameManager.myTurn) 
            StartCoroutine(MakeMovesRoutine());
        else
        {
            //else stop all coroutines and reset moveComplete to true
            StopAllCoroutines();
            moveComplete = true;
        }
    }

    /// <summary>
    /// Main function that decides whether to move around the board, collect mana, or cast spells
    /// </summary>
    public IEnumerator MakeMovesRoutine()
    {
        if (moveComplete)
        {
            //Check if there is still energy to spend
            if (terraScript.energy <= 0)
            {
                //If there is no energy try to cast
                TryToCast(0);
                yield return new WaitForSeconds(timeBetweenActions * 10);
                //Take damage from poison and burn then end turn
                playerControl.DoT();
                gameManager.NextTurn();
            }

            //If there is still energy then start a move
            //A move could be movement around the board, collecting mana, or casting spells
            moveComplete = false;

            //Run try cast with existing mana
            //The parameter int determines the chance that the AI will cast using their current mana
            //The higher the number the lower the chance
            TryToCast(3);
            yield return new WaitForSeconds(timeBetweenActions * 4);

            //Create an array of strings to store which objects are around the AI player
            string[] surroundings = new string[4];
            surroundings = GetSurroundings();

            yield return new WaitForSeconds(timeBetweenActions);

            
            if (ThereIsMana(surroundings))
            {
                yield return new WaitForSeconds(timeBetweenActions);

                int roll = Random.Range(1, 100);
                int chance;

                if (gameManager.turnCounter < 3)
                    chance = 45;
                else
                    chance = 15;

                if (roll <= chance)
                    RandomMove(surroundings);
                else
                    RandomCollect(surroundings);
            }
            else
            {
                RandomMove(surroundings);
                yield return new WaitForSeconds(0.01f);
            }

            moveComplete = true;
        }
        else
            yield return new WaitForEndOfFrame();
    }

    /// <summary>
    /// Determines whether to cast a spell with existing mana or to save mana
    /// </summary>
    /// <param name="chance"></param>
    void TryToCast(int chance)
    {
        if(terraScript.currentMana == 8)
        {
            CastPostChecks();
        }
        else if(statsManager.theirHP <= 22)
        {
            CastPostChecks();
        }
        else if (terraScript.currentMana <= 4)
        {
            int temp = Random.Range(0,chance);

            if (temp == 0)
            {
                CastPostChecks();
            }
        }
        else
        {
            CastPostChecks();
        }

    }

    /// <summary>
    /// The function that chooses which spell to cast after checking wheter the AI wants to use existing mana or save mana
    /// </summary>
    void CastPostChecks()
    {
        CheckWhatsCastable();

        if (castableSpells.Contains("Spirit Walk"))
        {
            castableSpells.Remove("Spirit Walk");
        }

        if (castableSpells.Contains("Eat Mana"))
        {
            castableSpells.Remove("Eat Mana");
        }

        if (castableSpells.Contains("Transmute"))
        {
            castableSpells.Remove("Transmute");
        }

        if (castableSpells.Contains("Smith Mana"))
        {
            castableSpells.Remove("Smith Mana");
        }

        if (castableSpells.Contains("Antidote"))
        {
            if (playerControl.burnt || playerControl.poisoned)
            {

            }
            else
            {
                castableSpells.Remove("Antidote");
            }
        }

        if (castableSpells.Contains("Heal") && statsManager.theirHP > 165)
            castableSpells.Remove("Heal");

        if (castableSpells.Contains("Heal") && statsManager.theirHP > 165)
            castableSpells.Remove("Heal");

        if (castableSpells.Contains("Eternal Tao") && statsManager.theirHP > 175)
            castableSpells.Remove("Eternal Tao");

        if (castableSpells.Contains("Paradise") && statsManager.theirHP > 160)
            castableSpells.Remove("Paradise");

        if (castableSpells.Contains("Harvest Soul") && statsManager.theirHP > 160)
            castableSpells.Remove("Harvest Soul");

        if (castableSpells.Contains("Reap") && statsManager.theirHP > 185)
            castableSpells.Remove("Reap");

        if (castableSpells.Count > 0)
        {
            int randomSpellInt = Random.Range(0, castableSpells.Count);

            SpellSO spellToCast = spellDict[castableSpells[randomSpellInt]];

            spellCaster.SubmitAttack(spellToCast.power, spellToCast, true);
        }
    }


    /// <summary>
    /// Chooses a random tile to move to and uses current surroundings array
    /// to determine which tiles available for movement
    /// </summary>
    /// <param name="surroundings"></param>
    private void RandomMove(string[] surroundings)
    {
        int temp = Random.Range(0, 4);

        if (surroundings[temp] == "Dirt(Clone)")
        {
            if (temp == 0)
                MoveUp();
            if (temp == 1)
                MoveDown();
            if (temp == 2)
                MoveRight();
            if (temp == 3)
                MoveLeft();
            if(temp == 4)
                RandomMove(surroundings);
        }
        else
        {
            RandomMove(surroundings);
        }
    }

    /// <summary>
    /// Randomly chooses one of the mana surrounding the AI and collects it
    /// </summary>
    /// <param name="surroundings"></param>
    private void RandomCollect(string[] surroundings)
    {
        if (surroundings[0] != "Dirt(Clone)" && surroundings[0] != "Empty" && surroundings[0] != "DragonVein(Clone)")
            CollectUp();
        else if (surroundings[1] != "Dirt(Clone)" && surroundings[1] != "Empty" && surroundings[1] != "DragonVein(Clone)")
            CollectDown();
        else if (surroundings[2] != "Dirt(Clone)" && surroundings[2] != "Empty" && surroundings[2] != "DragonVein(Clone)")
            CollectRight();
        else if (surroundings[3] != "Dirt(Clone)" && surroundings[3] != "Empty" && surroundings[3] != "DragonVein(Clone)")
            CollectLeft();
    }

    /// <summary>
    /// Takes in aray of current surroundings and determines whether there is mana
    /// </summary>
    /// <param name="surroundings"></param>
    /// <returns></returns>
    private bool ThereIsMana(string[] surroundings)
    {
        foreach (string potential in surroundings)
        {
            if (potential != "Dirt(Clone)" && potential != "Empty" && potential != "DragonVein(Clone)")
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Populates an array of 4 strings, each for a direction (up, down, left, right), with a string stating
    /// what is on the immediate tile in that direction
    /// </summary>
    /// <returns></returns>
    public string[] GetSurroundings()
    {
        List<Vector3> directions = new List<Vector3>();
        string[] surroundings = new string[4];

        Vector3 upVec = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
        Vector3 downVec = new Vector3(transform.position.x, transform.position.y - 1, transform.position.z);
        Vector3 rightVec = new Vector3(transform.position.x + 1, transform.position.y, transform.position.z);
        Vector3 leftVec = new Vector3(transform.position.x - 1, transform.position.y, transform.position.z);

        directions.Add(upVec); directions.Add(downVec); directions.Add(rightVec); directions.Add(leftVec);

        for (int i = 0; i < 4; i++)
        {
            if(cellManager.masterGrid.ContainsKey(directions[i]))
            {
                surroundings[i] = cellManager.masterGrid[directions[i]].name;
            }
            else
            {
                surroundings[i] = "Empty";
            }
        }

        return surroundings;
    }

    /// <summary>
    /// Clears castable spells list and uses current mana to determine what spells are castable
    /// </summary>
    void CheckWhatsCastable()
    {
        castableSpells.Clear();

        foreach(SpellSO spell in spellCaster.spellsStandard)
        {
            if (CanAffordSpell(spell))
                castableSpells.Add(spell.spellName);
        }
    }

    /// <summary>
    /// Check the cost of the param spell to see if AI has enough mana for it. If so return true else return false.
    /// </summary>
    /// <param name="spell"></param>
    /// <returns></returns>
    private bool CanAffordSpell(SpellSO spell)
    {
        if (terraScript.waterCount >= spell.blueCost && terraScript.plantCount >= spell.greenCost &&
           terraScript.fireCount >= spell.redCost && terraScript.rockCount >= spell.yellowCost &&
           terraScript.lifeCount >= spell.whiteCost && terraScript.deathCount >= spell.blackCost)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    #region Controls
    /// <summary>
    /// Feeds into the playerControl script to move left
    /// </summary>
    void MoveLeft()
    {
        if (playerControl.facingLeft)
        {
            playerControl.MoveLeft();
        }
        else
        {
            playerControl.MoveLeft();
            playerControl.MoveLeft();
        }

    }

    /// <summary>
    /// Feeds into the playerControl script to move right
    /// </summary>
    void MoveRight()
    {
        if (playerControl.facingRight)
        {
            playerControl.MoveRight();
        }
        else
        {
            playerControl.MoveRight();
            playerControl.MoveRight();
        }
    }

    /// <summary>
    /// Feeds into the playerControl script to move up
    /// </summary>
    void MoveUp()
    {
        if (playerControl.facingBack)
        {
            playerControl.MoveUp();
        }
        else
        {
            playerControl.MoveUp();
            playerControl.MoveUp();
        }
    }

    /// <summary>
    /// Feeds into the playerControl script to move down
    /// </summary>
    void MoveDown()
    {
        if (playerControl.facingFront)
        {
            playerControl.MoveDown();
        }
        else
        {
            playerControl.MoveDown();
            playerControl.MoveDown();
        }
    }

    /// <summary>
    /// Feeds into the playerControl script to collect left
    /// </summary>
    void CollectLeft()
    {
        if (playerControl.facingLeft)
        {
            terraScript.Take(false);
        }
        else
        {
            playerControl.MoveLeft();
            terraScript.Take(false);
        }
    }

    /// <summary>
    /// Feeds into the playerControl script to collect right
    /// </summary>
    void CollectRight()
    {
        if (playerControl.facingRight)
        {
            terraScript.Take(false);
        }
        else
        {
            playerControl.MoveRight();
            terraScript.Take(false);
        }
    }

    /// <summary>
    /// Feeds into the playerControl script to collect up
    /// </summary>
    void CollectUp()
    {
        if (playerControl.facingBack)
        {
            terraScript.Take(false);
        }
        else
        {
            playerControl.MoveUp();
            terraScript.Take(false);
        }
    }

    /// <summary>
    /// Feeds into the playerControl script to collect down
    /// </summary>
    void CollectDown()
    {
        if (playerControl.facingFront)
        {
            terraScript.Take(false);
        }
        else
        {
            playerControl.MoveDown();
            terraScript.Take(false);
        }
    }
    #endregion
}
