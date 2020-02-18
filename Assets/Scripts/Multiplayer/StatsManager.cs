using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class StatsManager : MonoBehaviourPunCallbacks
{

    [Header("General")]
    public static StatsManager instance;
    public SpellCaster spellCaster;
    public int attDefIncrement = 15;
    private GameManager gameManager;
    private SaveLoad saveLoad;

    [Header("P1")]
    public Text p1Name;
    public Text p1RP;
    public BarScript p1Bar;
    public GameObject p1PsnObj;
    public GameObject p1BrnObj;

    [Header("P2")]
    public Text p2Name;
    public Text p2RP;
    public BarScript p2Bar;
    public GameObject p2PsnObj;
    public GameObject p2BrnObj;

    [Header("MyStats")]
    public Text myName;
    public Text myRP;
    public Text myAttkText;
    public Text myDefText;
    private BarScript myBar;
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
    private BarScript theirBar;
    [HideInInspector] public int theirHP;
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
        gameManager = GameManager.instance;
        saveLoad = SaveLoad.instance;

        if (!saveLoad.tournamentHost)
        {
            myHP = myHPMax;
            theirHP = theirHPMax;

            if (PhotonNetwork.IsMasterClient)
            {
                myName = p1Name;
                myRP = p1RP;
                myBar = p1Bar;
                myPsnObj = p1PsnObj;
                myBrnObj = p1BrnObj;

                theirName = p2Name;
                theirRP = p2RP;
                theirBar = p2Bar;
                theirPsnObj = p2PsnObj;
                theirBrnObj = p2BrnObj;
            }
            else
            {
                myName = p2Name;
                myRP = p2RP;
                myBar = p2Bar;
                myPsnObj = p2PsnObj;
                myBrnObj = p2BrnObj;

                theirName = p1Name;
                theirRP = p1RP;
                theirBar = p1Bar;
                theirPsnObj = p1PsnObj;
                theirBrnObj = p1BrnObj;
            }

            myBar.MaxValue = myHPMax;
            myBar.SetValue(myHP);
            theirBar.MaxValue = theirHPMax;
            theirBar.SetValue(theirHP);

            myAttkText.text = myAttack.ToString();
            myDefText.text = myDefense.ToString();

            SetNamesAndLevels();
        }
    }

    void SetNamesAndLevels()
    {
        if (!saveLoad.tournamentHost)
        {
            myName.text = saveLoad.playerName;
            myRP.text = "RP " + saveLoad.playerRP.ToString();
            base.photonView.RPC("RPC_SetNamesAndLevels", RpcTarget.Others, myName.text, myRP.text, saveLoad.playerRP);
            TournamentHost.instance.SetPlayerInfo(myName.text, myRP.text);
        }
    }

    [PunRPC]
    void RPC_SetNamesAndLevels(string _theirName, string _theirRP, int theirRPInt)
    {
        if (!saveLoad.tournamentHost)
        {
            theirName.text = _theirName;
            theirRP.text = _theirRP;
            gameManager.otherPlayerRP = theirRPInt;
        }
    }


    //Health Mods
    public void MyHealth(int amount, bool psnOrBurn)
    {
        if (!saveLoad.tournamentHost)
        {
            myHP += CalculateAmount(amount, myHP, myHPMax);
            myBar.SetValue(myHP);

            if (myHP <= 0)
                gameManager.GameOver();

            TournamentHost.instance.AdjustHealth(myName.text, CalculateAmount(amount, myHP, myHPMax));

            base.photonView.RPC("RPC_MyHealth", RpcTarget.Others, amount, psnOrBurn);
        }
    }


    [PunRPC]
    private void RPC_MyHealth(int amount, bool psnOrBurn)
    {
        if (!saveLoad.tournamentHost)
        {
            theirHP += CalculateAmount(amount, theirHP, theirHPMax);
            theirBar.SetValue(theirHP);
        }
    }

    public void TheirHealth(int amount, bool psnOrBurn)
    {
        if (!saveLoad.tournamentHost)
        {
            if (theirCounter && CalculateAmount(amount, myHP, myHPMax) < 0 && !psnOrBurn)
            {
                theirCounter = false;
            }
            else
            {
                theirHP += CalculateAmount(amount, theirHP, theirHPMax);
                theirBar.SetValue(theirHP);
            }

            base.photonView.RPC("RPC_TheirHealth", RpcTarget.Others, amount, psnOrBurn);
        }
    }

    [PunRPC]
    private void RPC_TheirHealth(int amount, bool psnOrBurn)
    {
        if (!saveLoad.tournamentHost)
        {
            if (myCounter && CalculateAmount(amount, myHP, myHPMax) < 0 && !psnOrBurn)
            {
                TheirHealth(amount, psnOrBurn);
                spellCaster.Counter();
                myCounter = false;
            }
            else
            {
                myHP += CalculateAmount(amount, myHP, myHPMax);
                myBar.SetValue(myHP);

                TournamentHost.instance.AdjustHealth(myName.text, CalculateAmount(amount, myHP, myHPMax));

                if (myHP <= 0)
                    gameManager.GameOver();
            }
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

    public void MyStat(string stat, int amount)
    {
        MyStatAdjust(stat, amount);
        base.photonView.RPC("RPC_TheirStat", RpcTarget.Others, stat, amount);
    }

    [PunRPC]
    public void RPC_TheirStat(string stat, int amount)
    {
        TheirStatAdjust(stat, amount);
    }

    public void TheirStat(string stat, int amount)
    {
        TheirStatAdjust(stat, amount);

        base.photonView.RPC("RPC_MyStat", RpcTarget.Others, stat, amount);
    }

    [PunRPC]
    public void RPC_MyStat(string stat, int amount)
    {
        MyStatAdjust(stat, amount);
    }

    private void TheirStatAdjust(string stat, int amount)
    {
        if (!saveLoad.tournamentHost)
        {
            if (amount < 0)
            {
                if (theirDefense <= amount * -1 && stat == "Defense")
                {
                    amount = (theirDefense - 1) * -1;
                }

                if (theirAttack <= amount * -1 && stat == "Attack")
                {
                    amount = (theirAttack - 1) * -1;
                }

                if (amount == 0 && stat == "Attack")
                {
                    gameManager.DisplayPublicMessage("Attack cannot be lower than 1");
                }

                if (amount == 0 && stat == "Defense")
                {
                    gameManager.DisplayPublicMessage("Defense cannot be lower than 1");
                }
            }

            if (stat == "Attack")
                theirAttack += amount;
            if (stat == "Defense")
                theirDefense += amount;

            /*if (amount < 0)
                gameManager.DisplayMessage(theirName.text + "'s " + stat + " was reduced by " + amount);
            else
                gameManager.DisplayMessage(theirName.text + "'s " + stat + " raised by " + amount);*/
        }
    }

    private void MyStatAdjust(string stat, int amount)
    {
        if (!saveLoad.tournamentHost)
        {
            print("amount: " + amount);

            if (amount < 0)
            {
                if (myDefense <= amount * -1 && stat == "Defense")
                {
                    amount = (myDefense - 1) * -1;
                    print("amount: " + amount);
                }

                if (myAttack <= amount * -1 && stat == "Attack")
                {
                    amount = (myAttack - 1) * -1;
                    print("amount: " + amount);
                }

                print("amount: " + amount);

                if (amount == 0 && stat == "Attack")
                {
                    gameManager.DisplayPublicMessage("Attack cannot be lower than 1");
                }

                if (amount == 0 && stat == "Defense")
                {
                    gameManager.DisplayPublicMessage("Defense cannot be lower than 1");
                }
            }

            if (stat == "Attack")
                myAttack += amount;
            if (stat == "Defense")
                myDefense += amount;

            if (amount < 0)
                gameManager.DisplayPublicMessage(myName.text + "'s " + stat + " was reduced by " + amount);
            else
                gameManager.DisplayPublicMessage(myName.text + "'s " + stat + " raised by " + amount);

            myAttkText.text = myAttack.ToString();
            myDefText.text = myDefense.ToString();
        }
    }

}
