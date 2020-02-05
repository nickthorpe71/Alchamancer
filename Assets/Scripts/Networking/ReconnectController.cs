using System.Collections;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

public class ReconnectController : MonoBehaviourPunCallbacks
{
    public Vector3 savePos;

    public int waterCount;
    public int plantCount;
    public int fireCount;
    public int rockCount;
    public int lifeCount;
    public int deathCount;

    public int energy;
    public int currentMana;
    public bool poisoned;
    public bool burnt;

    private Terraform terraScript;
    private GameManager gameManager;

    private void Start()
    {
        gameManager = GameManager.instance;
        terraScript = gameManager.terraScript;

        //InvokeRepeating("StoreData", 0, 1);
    }

    private void OnApplicationPause(bool isPaused)
    {
        if (isPaused)
        {
            StoreData();
            PhotonNetwork.Disconnect();
        }
        else
        {
            PhotonNetwork.ReconnectAndRejoin();
            StartCoroutine(ReconnectRoutine());
        }
    }
    private void Update()
    {
        /*if (PhotonNetwork.NetworkingClient.LoadBalancingPeer.PeerState == PeerStateValue.Disconnected)
        {
            PhotonNetwork.ReconnectAndRejoin();
        }*/

        if(Input.GetKeyDown(KeyCode.B))
        {
            StoreData();
            PhotonNetwork.Disconnect();
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            PhotonNetwork.ReconnectAndRejoin();
            StartCoroutine(ReconnectRoutine());
        }
    }

    private void StoreData()
    {
        if (gameManager.myPlayer != null)
        {
            savePos = gameManager.myPlayer.transform.position;

            waterCount = terraScript.waterCount;
            plantCount = terraScript.plantCount;
            fireCount = terraScript.fireCount;
            rockCount = terraScript.rockCount;
            lifeCount = terraScript.lifeCount;
            deathCount = terraScript.deathCount;

            energy = terraScript.energy;
            currentMana = terraScript.currentMana;
            poisoned = gameManager.playerControl.poisoned;
            burnt = gameManager.playerControl.burnt;
        }
    }

    public IEnumerator ReconnectRoutine()
    {
        yield return new WaitForSeconds(1.5f);
        gameManager.myPlayer.transform.position = savePos;

        terraScript = gameManager.terraScript;
        gameManager.spellCaster.terraScript = terraScript;

        gameManager.playerControl.burnt = burnt;
        gameManager.playerControl.poisoned = poisoned;

        terraScript.waterCount = waterCount;
        terraScript.plantCount = plantCount;
        terraScript.fireCount = fireCount;
        terraScript.rockCount = rockCount;
        terraScript.lifeCount = lifeCount;
        terraScript.deathCount = deathCount;

        terraScript.energy = energy;
        terraScript.currentMana = currentMana;

        terraScript.CheckEnergy();
        terraScript.CheckMana();
    }
}