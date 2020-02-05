using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class StatsManagerOffline : MonoBehaviourPunCallbacks
{
    
    [Header("General")]
    public static StatsManagerOffline instance;
    public SpellCasterOffline spellCaster;
    private GameManagerOffline gameManager;
    private SaveLoad saveLoad;

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

		myHP = myHPMax;
		theirHP = theirHPMax;

        myBar.MaxValue = myHPMax;
        myBar.SetValue(myHP);
        theirBar.MaxValue = theirHPMax;
        theirBar.SetValue(theirHP);

        myAttkText.text = myAttack.ToString();
        myDefText.text = myDefense.ToString();

        SetNamesAndLevels();

    }

    void SetNamesAndLevels()
    {
        myName.text = saveLoad.playerName;
        myRP.text = "RP " + saveLoad.playerRP.ToString();
    }

    //Health Mods
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

    //Other Stat Mods
    public void MyStat(string stat, int amount)
    {
        MyStatAdjust(stat, amount);
    }

    public void TheirStat(string stat, int amount)
    {
        TheirStatAdjust(stat, amount);
    }

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
