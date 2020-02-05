using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIControllerOffline : MonoBehaviour
{
    private PlayerControlOffline playerControl;
    private TerraformOffline terraScript;

    private GameManagerOffline gameManager;
    private CellManagerOffline cellManager;
    private SpellCasterOffline spellCaster;
    private StatsManagerOffline statsManager;

    public bool moveComplete = true;

    private float timeBetweenActions = 0.25f;

    public Dictionary<string, SpellSO> spellDict = new Dictionary<string, SpellSO>();
    public List<string> castableSpells = new List<string>();

    void Start()
    {
        gameManager = GameManagerOffline.instance;
        cellManager = CellManagerOffline.instance;
        spellCaster = cellManager.spellCaster;
        statsManager = StatsManagerOffline.instance;
        terraScript = GetComponent<TerraformOffline>();
        playerControl = GetComponent<PlayerControlOffline>();

        PopulateSpellDict();

        InvokeRepeating("CheckTurn", 1, 0.25f);

    }

    void PopulateSpellDict()
    {
        foreach (SpellSO spell in spellCaster.spellsStandard)
        {
            spellDict.Add(spell.spellName, spell);
        }
    }

    void CheckTurn()
    {
        if (!gameManager.myTurn)
            StartCoroutine(CheckTurnRoutine());
        else
        {
            StopAllCoroutines();
            moveComplete = true;
        }
    }

    public IEnumerator CheckTurnRoutine()
    {
        if (moveComplete)
        {
            if (terraScript.energy <= 0)
            {
                TryToCast(0);
                yield return new WaitForSeconds(timeBetweenActions * 10);
                playerControl.DoT();
                gameManager.NextTurn();
            }

            moveComplete = false;

            TryToCast(3);
            yield return new WaitForSeconds(timeBetweenActions * 4);

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

    #region Controls
    void MoveLeft()
    {
        if(playerControl.facingLeft)
        {
            playerControl.MoveLeft();
        }
        else
        {
            playerControl.MoveLeft();
            playerControl.MoveLeft();
        }

    }

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

    void CollectLeft()
    {
        if(playerControl.facingLeft)
        {
            terraScript.Take(false);
        }
        else
        {
            playerControl.MoveLeft();
            terraScript.Take(false);
        }
    }

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

    void CheckWhatsCastable()
    {
        castableSpells.Clear();

        foreach(SpellSO spell in spellCaster.spellsStandard)
        {
            if (CanAffordSpell(spell))
                castableSpells.Add(spell.spellName);
        }
    }

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
}
