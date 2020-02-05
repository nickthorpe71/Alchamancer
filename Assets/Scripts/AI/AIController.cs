using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    private PlayerControl playerControl;
    private Terraform terraScript;

    private GameManager gameManager;
    private CellManager cellManager;
    private SpellCaster spellCaster;
    private StatsManager statsManager;

    public bool moveComplete = true;

    private float timeBetweenActions = 0.65f;

    public Dictionary<string, SpellSO> spellDict = new Dictionary<string, SpellSO>();
    public List<string> castableSpells = new List<string>();

    private void Awake()
    {

    }

    void Start()
    {
        gameManager = GameManager.instance;
        cellManager = CellManager.instance;
        spellCaster = cellManager.spellCaster;
        statsManager = StatsManager.instance;
        terraScript = GetComponent<Terraform>();
        playerControl = GetComponent<PlayerControl>();

        PopulateSpellDict();

        InvokeRepeating("CloseMessages", 1, 1);
        InvokeRepeating("CheckTurn", 1, 1);

    }

    void PopulateSpellDict()
    {
        foreach (SpellSO spell in spellCaster.spellsStandard)
        {
            spellDict.Add(spell.spellName, spell);
        }
    }

    void CloseMessages()
    {
        GameObject[] messages = GameObject.FindGameObjectsWithTag("MsgTxt");

        if (messages.Length > 0)
        {
            foreach (GameObject message in messages)
            {
                message.GetComponent<MessageBox>().DestroyMe();
            }
        }
    }

    void CheckTurn()
    {
        if (gameManager.myTurn)
            StartCoroutine(CheckTurnRoutine());

        if (statsManager.theirName.text == "P2" || statsManager.theirName.text == "P1")
        {
            gameManager.RageQuit();
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
                gameManager.NextTurn();
            }

            moveComplete = false;

            TryToCast(3);
            yield return new WaitForSeconds(timeBetweenActions);

            string[] surroundings = new string[4];
            surroundings = GetSurroundings();
            Debug.Log("Above me is " + surroundings[0]);
            Debug.Log("Below me is " + surroundings[1]);
            Debug.Log("Right of me is " + surroundings[2]);
            Debug.Log("Left of me is " + surroundings[3]);

            yield return new WaitForSeconds(timeBetweenActions);

            if (ThereIsMana(surroundings))
            {
                yield return new WaitForSeconds(timeBetweenActions);

                bool foundUsefulMana = false;

                foreach (SpellSO spell in spellCaster.spellsStandard)
                {
                    if (LeftToCast(spell) != "NotEffective")
                    {
                        string manaNeeded = LeftToCast(spell);

                        for (int i = 0; i < 4; i++)
                        {
                            if (surroundings[i] == manaNeeded)
                            {
                                if (i == 0) CollectUp();
                                if (i == 1) CollectDown();
                                if (i == 2) CollectRight();
                                if (i == 3) CollectLeft();

                                foundUsefulMana = true;
                                yield return new WaitForSeconds(timeBetweenActions);
                            }
                        }
                    }
                }

                if (foundUsefulMana)
                {
                    TryToCast(6);
                    yield return new WaitForSeconds(timeBetweenActions * 2);
                }
                else
                {
                    RandomCollect(surroundings);
                    yield return new WaitForSeconds(timeBetweenActions);
                }

            }
            else
            {
                RandomMove(surroundings);
                yield return new WaitForSeconds(timeBetweenActions);
            }

            moveComplete = true;
        }
        else
            yield return new WaitForEndOfFrame();
    }

    void TryToCast(int chance)
    {
        if (terraScript.energy == 8)
        {
            CastPostChecks();
        }
        else if (statsManager.theirHP <= 22)
        {
            CastPostChecks();
        }
        else if (terraScript.energy <= 4)
        {
            int temp = Random.Range(0, chance);

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

        if (castableSpells.Contains("HealingLight") && statsManager.myHP < 160)
            castableSpells.Remove("HealingLight");

        if (castableSpells.Contains("Paradise") && statsManager.myHP < 140)
            castableSpells.Remove("Paradise");

        if (castableSpells.Contains("HarvestSoul") && statsManager.myHP < 140)
            castableSpells.Remove("HarvestSoul");

        if (castableSpells.Contains("Reap") && statsManager.myHP < 180)
            castableSpells.Remove("Reap");

        if (castableSpells.Count > 0)
        {
            foreach (string spell in castableSpells)
            {
                Debug.Log(spell);
            }

            int randomSpellInt = Random.Range(0, castableSpells.Count);

            SpellSO spellToCast = spellDict[castableSpells[randomSpellInt]];

            spellCaster.SubmitAttack(spellToCast.power, spellToCast);

        }
    }

    #region Controls
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
            if (temp == 4)
                RandomMove(surroundings);
        }
        else
        {
            RandomMove(surroundings);
        }
    }

    private void RandomCollect(string[] surroundings)
    {
        if (surroundings[0] != "Dirt(Clone)" && surroundings[0] != "Empty")
            CollectUp();
        else if (surroundings[1] != "Dirt(Clone)" && surroundings[1] != "Empty")
            CollectDown();
        else if (surroundings[2] != "Dirt(Clone)" && surroundings[2] != "Empty")
            CollectRight();
        else if (surroundings[3] != "Dirt(Clone)" && surroundings[3] != "Empty")
            CollectLeft();
    }

    private bool ThereIsMana(string[] surroundings)
    {
        foreach (string potential in surroundings)
        {
            if (potential != "Dirt(Clone)" && potential != "Empty")
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
            if (cellManager.masterGrid.ContainsKey(directions[i]))
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

        foreach (SpellSO spell in spellCaster.spellsStandard)
        {
            if (CanAffordSpell(spell))
                castableSpells.Add(spell.spellName);
        }
    }

    private string LeftToCast(SpellSO spell)
    {
        List<int> costs = new List<int>();

        string manaType = "";

        int total = 0;

        int blueLeft = spell.blueCost - terraScript.waterCount;
        int greenLeft = spell.greenCost - terraScript.plantCount;
        int redLeft = spell.redCost - terraScript.fireCount;
        int yellowLeft = spell.yellowCost - terraScript.rockCount;
        int whiteLeft = spell.whiteCost - terraScript.lifeCount;
        int blackLeft = spell.blackCost - terraScript.deathCount;

        costs.Add(blueLeft); costs.Add(greenLeft); costs.Add(redLeft);
        costs.Add(yellowLeft); costs.Add(whiteLeft); costs.Add(blackLeft);

        for (int i = 0; i < 6; i++)
        {
            if (costs[i] < 0)
                costs[i] = 0;

            if (costs[i] == 1)
            {
                if (i == 0) manaType = "Water(Clone)";
                if (i == 1) manaType = "Plant(Clone)";
                if (i == 2) manaType = "Fire(Clone)";
                if (i == 3) manaType = "Rock(Clone)";
                if (i == 4) manaType = "Light(Clone)";
                if (i == 5) manaType = "Death(Clone)";
            }

            total += costs[i];
        }

        if (total == 1)
        {
            return manaType;
        }

        return "NotEffective";
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
