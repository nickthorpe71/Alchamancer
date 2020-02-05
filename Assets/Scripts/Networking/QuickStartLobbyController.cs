using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuickStartLobbyController : MonoBehaviourPunCallbacks
{
    public GameObject quickStartButton;
    [SerializeField]
    private GameObject quickCancelButton;
    [SerializeField]
    private GameObject connectImage;
    [SerializeField]
    private int RoomSize;

    public Text playerCount;

    private Carry carry;
    private bool restartAIRoutine = true;

    public QuickStartRoomController roomController;

    private void Start()
    {
        if(PhotonNetwork.IsConnected)
            connectImage.SetActive(false);
        else
            connectImage.SetActive(true);

        carry = Carry.instance;

        if (carry.aIAcvive)
        {
            InvokeRepeating("StartAIRoutine", 1, 2);
        }
    }

    private void Update()
    {
        CalcOnlinePlayers();
    }

    public void CalcOnlinePlayers()
    {
        playerCount.text = PhotonNetwork.CountOfPlayers.ToString();
    }

    public void StartAIRoutine()
    {
        if (restartAIRoutine)
            StartCoroutine(AIRoutine());
    }

    public IEnumerator AIRoutine()
    {
        if(PhotonNetwork.IsConnected && restartAIRoutine)
        {
            restartAIRoutine = false;
            QuickStart();
            yield return new WaitForSeconds(3);
            QuickCancel();
            yield return new WaitForSeconds(1);
            restartAIRoutine = true;
        }
    }

    public override void OnConnectedToMaster() 
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        connectImage.SetActive(false);
    }

    public void QuickStart()
    {
        if (PhotonNetwork.IsConnected)
        {
            connectImage.SetActive(false);
            PhotonNetwork.AutomaticallySyncScene = true;
            quickStartButton.SetActive(false);
            quickCancelButton.SetActive(true);
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            connectImage.SetActive(true);
            quickStartButton.SetActive(false);
            quickCancelButton.SetActive(false);
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to join a room");
        CreateRoom();
    }
    void CreateRoom()
    {
        Debug.Log("Creating room now");
        int randomRoomNumber = Random.Range(0, 10000); //creating a random name for the room
        RoomOptions roomOps = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = 2, /*PlayerTtl = int.MaxValue*/ };
        PhotonNetwork.CreateRoom("Room" + randomRoomNumber, roomOps);
        Debug.Log(randomRoomNumber);
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to create room... trying again");
        CreateRoom();
    }
    public void QuickCancel()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        quickCancelButton.SetActive(false);
        quickStartButton.SetActive(true);
        PhotonNetwork.LeaveRoom();
        roomController.numberOfPlayersInRoom = 0;
    }
}