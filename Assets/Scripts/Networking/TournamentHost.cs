using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Allows tournament host to spectate the match without affecting gameplay and allows players to send messages to the tournament host
/// </summary>
public class TournamentHost : MonoBehaviourPunCallbacks
{
    public static TournamentHost instance;

    public GameObject blocker; //blocks out main game controls UI from being displayed
    public GameObject timer;

    private SaveLoad saveLoad;
    public SpellCaster spellCaster;
    public StatsManager statsManager;

    public Dictionary<string, GameObject> animationDict = new Dictionary<string, GameObject>();

    private bool p1In;

    [Header("P1Info")]
    public Text p1Name;
    public Text p1RP;
    public int p1HP = 200;
    public BarScript p1Bar;
    public GameObject p1PsnObj;
    public GameObject p1BrnObj;

    [Header("P2Info")]
    public Text p2Name;
    public Text p2RP;
    public int p2HP = 200;
    public BarScript p2Bar;
    public GameObject p2PsnObj;
    public GameObject p2BrnObj;

    void Awake()
    {
        instance = this;
        saveLoad = SaveLoad.instance;

        if (SaveLoad.instance.tournamentHost)
        {
            blocker.SetActive(true);
        }
    }

    private void Start()
    {
        if (saveLoad.tournamentHost)
        {
            animationDict = spellCaster.animationDict;

            p1Bar.MaxValue = 200;
            p2Bar.MaxValue = 200;

            p1Bar.SetValue(200);
            p2Bar.SetValue(200);
        }
    }

    private void Update()
    {
        if (saveLoad.tournamentHost)
        {
            if (Input.GetKey(KeyCode.Space))
                GetComponent<PauseMenu>().ClickPauseMenu();

            //Starts countdown timer on all clients
            if (Input.GetKey(KeyCode.T))
                base.photonView.RPC("RPC_StartTimer", RpcTarget.AllBuffered); 
        }
    }

    /// <summary>
    /// Receives message to start countdown timer
    /// </summary>
    [PunRPC]
    public void RPC_StartTimer()
    {
        timer.SetActive(true);
    }

    /// <summary>
    /// Starts a routine that sends local players name and rp to tournament host
    /// </summary>
    /// <param name="name"></param>
    /// <param name="rp"></param>
    public void SetPlayerInfo(string name, string rp)
    {
        StartCoroutine(SetPlayerInfoRoutine(name, rp));
    }

    /// <summary>
    /// Sends local players name and rp to tournament host
    /// </summary>
    /// <param name="name"></param>
    /// <param name="rp"></param>
    public IEnumerator SetPlayerInfoRoutine(string name, string rp)
    {
        yield return new WaitForSeconds(3);
        base.photonView.RPC("RPC_SetPlayerInfo", RpcTarget.AllBuffered, name, rp);
    }

    /// <summary>
    /// Receives other players info and sets it in the UI display
    /// </summary>
    /// <param name="name"></param>
    /// <param name="rp"></param>
    [PunRPC]
    public void RPC_SetPlayerInfo(string name, string rp)
    {
        if (saveLoad.tournamentHost)
        {
            if (!p1In)
            {
                p1Name.text = name;
                p1RP.text = rp;
                p1In = true;
            }
            else
            {
                p2Name.text = name;
                p2RP.text = rp;
            }
        }
    }

    /// <summary>
    /// Used by players to send tournament host a message that their health changed
    /// </summary>
    /// <param name="name"></param>
    /// <param name="amount"></param>
    public void AdjustHealth(string name, int amount)
    {
        base.photonView.RPC("RPC_AdjustHealth", RpcTarget.AllBuffered, name, amount);
    }

    /// <summary>
    /// Reveices health adjustments from players to tournament host
    /// </summary>
    /// <param name="name"></param>
    /// <param name="amount"></param>
    [PunRPC]
    public void RPC_AdjustHealth(string name, int amount)
    {
        if (saveLoad.tournamentHost)
        {
            if(name == p1Name.text)
            {
                p1HP += amount;
                p1Bar.SetValue(p1HP);
            }
            else
            {
                p2HP += amount;
                p2Bar.SetValue(p2HP);
            }
        }
    }

    /// <summary>
    /// Used by players to send tournament host a message that their poison or burn status changed
    /// </summary>
    /// <param name="name"></param>
    /// <param name="amount"></param>
    public void SetPsnBrn(string name, string effect,  bool onOff)
    {
        base.photonView.RPC("RPC_SetPsnBrn", RpcTarget.AllBuffered, name, effect, onOff);
    }

    /// <summary>
    /// Reveices poison and burn status adjustments from players to tournament host
    /// </summary>
    /// <param name="name"></param>
    /// <param name="amount"></param>
    [PunRPC]
    public void RPC_SetPsnBrn(string name, string effect, bool onOff)
    {
        if (saveLoad.tournamentHost)
        {
            if (name == p1Name.text)
            {
                if (effect == "psn")
                    p1PsnObj.SetActive(onOff);
                if (effect == "brn")
                    p1BrnObj.SetActive(onOff);
            }
            else
            {
                if (effect == "psn")
                    p2PsnObj.SetActive(onOff);
                if (effect == "brn")
                    p2BrnObj.SetActive(onOff);
            }
        }
    }

    /// <summary>
    /// Sends projectile info from players to tournament host to be played on tournament host client
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="_projectile"></param>
    public void SubmitAttackProjectile( Vector3 start, Vector3 end, string _projectile)
    {
        if(PhotonNetwork.IsMasterClient)
            base.photonView.RPC("RPC_Projectile", RpcTarget.AllBuffered, start, end, _projectile);
    }

    /// <summary>
    /// Receives projectile from players to be played on tournament host client
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="_projectile"></param>
    [PunRPC]
    public void RPC_Projectile(Vector3 start, Vector3 end, string _projectile)
    {
        if (saveLoad.tournamentHost)
        {
            GameObject temp = Instantiate(animationDict[_projectile], start, Quaternion.identity);
            temp.GetComponent<Projectile>().projectileStart = start;
            temp.GetComponent<Projectile>().projectileEnd = end;
            temp.transform.LookAt(end);
        }
    }

    /// <summary>
    /// Sends stationart attack info from players to tournament host to be played on tournament host client
    /// </summary>
    /// <param name="location"></param>
    /// <param name="effect"></param>
    public void SubmitAttackLocation(Vector3 location,  string effect)
    {
        if (PhotonNetwork.IsMasterClient)
            base.photonView.RPC("RPC_Location", RpcTarget.AllBuffered, location, effect);
    }

    /// <summary>
    /// Receives stationary attack from players to be played on tournament host client
    /// </summary>
    /// <param name="location"></param>
    /// <param name="effect"></param>
    [PunRPC]
    public void RPC_Location(Vector3 location, string effect)
    {
        if (saveLoad.tournamentHost)
        {
            Instantiate(animationDict[effect], location, Quaternion.identity);
        }
    }

}
