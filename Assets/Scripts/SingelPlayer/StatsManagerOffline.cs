using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

/// <summary>
/// Singleplayer Version - Handles both players Attack, Defense and HP
/// </summary>
public class StatsManagerOffline : MonoBehaviourPunCallbacks
{
    
    [Header("General")]
    public static StatsManagerOffline instance; //Allows this script to be easily accessable from other scripts
    public SpellCasterOffline spellCaster;
    private GameManagerOffline gameManager;
    private SaveLoad saveLoad;

    //UI display elements for human player
    [Header("MyStats")]
    public Text myName;
    public Text myRP;
    public Text myAttkText;
    public Text myDefText;
    public BarScript myBar;
    public int myHP;
    public int myHPMax;
	public int myAttack = 100;
	public int myDefense = 100;
    public GameObject myPsnObj;
    public GameObject myBrnObj;
    public bool myCounter;

    //UI display elements for AI player
    [Header("TheirStats")]
    public Text theirName;
    public Text theirRP;
    public BarScript theirBar;
    public int theirHP;
    public int theirHPMax;
    public int theirAttack = 100;
	public int theirDefense = 100;
    public GameObject theirPsnObj;
    public GameObject theirBrnObj;
    public bool theirCounter;

    void Awake()
	{
		instance = this;
	}

    private void Start()
    {
        gameManager = GameManagerOffline.instance;
        saveLoad = SaveLoad.instance;

        //Populate values in UI fields for human player
        myHP = myHPMax;
		theirHP = theirHPMax;

        myBar.MaxValue = myHPMax;
        myBar.SetValue(myHP);
        theirBar.MaxValue = theirHPMax;
        theirBar.SetValue(theirHP);

        myAttkText.text = myAttack.ToString();
        myDefText.text = myDefense.ToString();

        myName.text = saveLoad.playerName;
        myRP.text = "RP " + saveLoad.playerRP.ToString();

    }

    /// <summary>
    /// Used to alter the health of the human player and checks if there is a counter to reverse the damage - amount can be positive or negative
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="psnOrBurn"></param>
    public void MyHealth(int amount, bool psnOrBrn)
	{
        if (myCounter && CalculateAmount(amount, myHP, myHPMax) < 0 && !psnOrBrn)
        {
            myCounter = false;
            spellCaster.Counter(false);

            TheirHealth(amount, false);
        }
        else
        {
            myHP += CalculateAmount(amount, myHP, myHPMax);
            myBar.SetValue(myHP);

            if (myHP <= 0)
                gameManager.GameOver();
        }
    }

    /// <summary>
    /// Used to alter the health of the AI player and checks if there is a counter to reverse the damage - amount can be positive or negative
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="psnOrBurn"></param>
    public void TheirHealth(int amount, bool psnOrBrn)
    {
        if (theirCounter && CalculateAmount(amount, myHP, myHPMax) < 0 && !psnOrBrn)
        {
            theirCounter = false;
            spellCaster.Counter(true);

            MyHealth(amount, false);
        }
        else
        {
            theirHP += CalculateAmount(amount, theirHP, theirHPMax);
            theirBar.SetValue(theirHP);

            if (theirHP <= 0)
                gameManager.Win();
        }
    }

    /// <summary>
    /// Takes in the amount that health should be altered by and makes sure health does not go above max health
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="currentHP"></param>
    /// <param name="maxHP"></param>
    /// <returns></returns>
    int CalculateAmount(int amount, int currentHP, int maxHP)
    {
        if (amount > 0)
        {
            if (currentHP == maxHP)
            {
                gameManager.DisplayMessage("Already at full health");
                return 0;
            }

            int diff = maxHP - currentHP;

            if (amount <= diff)
                return amount;
            else
                return diff;
        }
        else
            return amount;

    }

    /// <summary>
    /// Used to start a change of a "stat" of the human player (attack/defense/) by an amount - amount can be negative or positive
    /// </summary>
    /// <param name="stat"></param>
    /// <param name="amount"></param>
    public void MyStat(string stat, int amount)
    {
        MyStatAdjust(stat, amount);
    }

    /// <summary>
    /// Used to start a change of a "stat" of the AI player (attack/defense/) by an amount - amount can be negative or positive
    /// </summary>
    /// <param name="stat"></param>
    /// <param name="amount"></param>
    public void TheirStat(string stat, int amount)
    {
        TheirStatAdjust(stat, amount);
    }

    /// <summary>
    /// Alters a "stat" of the AI player (attack/defense/) by an amount - amount can be negative or positive
    /// </summary>
    /// <param name="stat"></param>
    /// <param name="amount"></param>
    private void TheirStatAdjust(string stat, int amount)
    {
        if (amount < 0)
        {
            if (theirDefense <= amount * -1 && stat == "Defense")
            {
                amount = (theirDefense - 1) * - 1;
            }

            if (theirAttack <= amount * -1 && stat == "Attack")
            {
                amount = (theirAttack - 1) * - 1;
            }

            if (amount == 0 && stat == "Attack")
            {
                gameManager.DisplayMessage("Attack cannot be lower than 1");
            }

            if (amount == 0 && stat == "Defense")
            {
                gameManager.DisplayMessage("Defense cannot be lower than 1");
            }
        }

        if (stat == "Attack")
            theirAttack += amount;
        if (stat == "Defense")
            theirDefense += amount;

        if (amount <= 0)
            gameManager.DisplayMessage(theirName.text + "'s " + stat + " was reduced by " + amount);
        else
            gameManager.DisplayMessage(theirName.text + "'s " + stat + " raised by " + amount);
    }

    /// <summary>
    /// Alters a "stat" of the human player (attack/defense/) by an amount - amount can be negative or positive
    /// </summary>
    /// <param name="stat"></param>
    /// <param name="amount"></param>
    private void MyStatAdjust(string stat, int amount)
    {
        if (amount < 0)
        {
            if (myDefense <= amount * -1 && stat == "Defense")
            {
                amount = (myDefense - 1) * -1;
            }

            if (myAttack <= amount * -1 && stat == "Attack")
            {
                amount = (myAttack - 1) * -1;
            }

            if (amount == 0 && stat == "Attack")
            {
                gameManager.DisplayMessage("Attack cannot be lower than 1");
            }

            if (amount == 0 && stat == "Defense")
            {
                gameManager.DisplayMessage("Defense cannot be lower than 1");
            }
        }

        if (stat == "Attack")
            myAttack += amount;
        if (stat == "Defense")
            myDefense += amount;

        if (amount <= 0)
            gameManager.DisplayMessage("Your " + stat + " was reduced by " + amount);
        else
            gameManager.DisplayMessage("Your " + stat + " raised by " + amount);

        myAttkText.text = myAttack.ToString();
        myDefText.text = myDefense.ToString();
    }

}
