using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TournamentHost : MonoBehaviourPunCallbacks
{
    public static TournamentHost instance;

    public GameObject blocker;
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
            //statsManager.enabled = false;
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

            if (Input.GetKey(KeyCode.T))
                base.photonView.RPC("RPC_StartTimer", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    public void RPC_StartTimer()
    {
        timer.SetActive(true);
    }

    public void SetPlayerInfo(string name, string rp)
    {
        StartCoroutine(SetPlayerInfoRoutine(name, rp));
    }

    public IEnumerator SetPlayerInfoRoutine(string name, string rp)
    {
        yield return new WaitForSeconds(3);
        base.photonView.RPC("RPC_SetPlayerInfo", RpcTarget.AllBuffered, name, rp);
    }

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
    
    public void AdjustHealth(string name, int amount)
    {
        base.photonView.RPC("RPC_AdjustHealth", RpcTarget.AllBuffered, name, amount);
    }

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

    public void SetPsnBrn(string name, string effect,  bool onOff)
    {
        base.photonView.RPC("RPC_SetPsnBrn", RpcTarget.AllBuffered, name, effect, onOff);
    }

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

    public void SubmitAttackProjectile( Vector3 start, Vector3 end, string _projectile)
    {
        if(PhotonNetwork.IsMasterClient)
            base.photonView.RPC("RPC_Projectile", RpcTarget.AllBuffered, start, end, _projectile);
    }

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

    public void SubmitAttackLocation(Vector3 location,  string effect)
    {
        if (PhotonNetwork.IsMasterClient)
            base.photonView.RPC("RPC_Location", RpcTarget.AllBuffered, location, effect);
    }

    [PunRPC]
    public void RPC_Location(Vector3 location, string effect)
    {
        if (saveLoad.tournamentHost)
        {
            Instantiate(animationDict[effect], location, Quaternion.identity);
        }
    }





}
