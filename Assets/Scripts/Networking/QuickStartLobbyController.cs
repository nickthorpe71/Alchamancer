using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles searching for matches to play, canceling searches and calculating the amount of players online
/// </summary>
public class QuickStartLobbyController : MonoBehaviourPunCallbacks
{
    public GameObject quickStartButton;
    [SerializeField]
    private GameObject quickCancelButton;
    [SerializeField]
    private GameObject connectImage;
    [SerializeField]
    private int RoomSize; //How many players can be in a room

    public Text playerCount; //UI to display current amount of players online

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

    /// <summary>
    /// Displays the current amount of online players
    /// </summary>
    public void CalcOnlinePlayers()
    {
        playerCount.text = PhotonNetwork.CountOfPlayers.ToString();
    }

    /// <summary>
    /// Starts a routine that keeps trying to join matches
    /// </summary>
    public void StartAIRoutine()
    {
        if (restartAIRoutine)
            StartCoroutine(AIRoutine());
    }

    /// <summary>
    /// Tries to join a match then waits for 3 seconds and cancels the search
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Once connected to the master Photon server turn off the image that says you are offline
    /// </summary>
    public override void OnConnectedToMaster() 
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        connectImage.SetActive(false);
    }

    /// <summary>
    /// Function that is run by "Find Match" button - Looks for an open match, if found it joins else it creates a new match
    /// </summary>
    public void QuickStart()
    {
        roomController.isTournament = false;

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
            quickStartButton.GetComponent<Button>().enabled = false;
            quickCancelButton.SetActive(false);
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    /// <summary>
    /// This is run if we fail to join a room and in response creats a new room
    /// </summary>
    /// <param name="returnCode"></param>
    /// <param name="message"></param>
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to join a room");
        CreateRoom();
    }

    /// <summary>
    /// Function for creating a room
    /// </summary>
    void CreateRoom()
    {
        roomController.isTournament = false;

        Debug.Log("Creating room now");
        int randomRoomNumber = Random.Range(0, 10000); //creating a random name for the room
        RoomOptions roomOps = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = 2, /*PlayerTtl = int.MaxValue*/ };
        PhotonNetwork.CreateRoom("Room" + randomRoomNumber, roomOps);
        Debug.Log(randomRoomNumber);
    }

    /// <summary>
    /// Function for creating a tournamnet room - If you create the room you will only be spectating
    /// </summary>
    public void CreateTournament()
    {
        roomController.isTournament = true;
        SaveLoad.instance.tournamentHost = true;

        PhotonNetwork.AutomaticallySyncScene = true;

        Debug.Log("Creating tournament now");
        RoomOptions roomOps = new RoomOptions() { IsVisible = false, IsOpen = true, MaxPlayers = 3};
        PhotonNetwork.CreateRoom("Tournament" , roomOps);
    }

    /// <summary>
    /// Attempts to join a room with name "Tournament" and will allow player joining to play
    /// </summary>
    public void JoinTournament()
    {
        roomController.isTournament = true;

        if (PhotonNetwork.IsConnected)
        {
            connectImage.SetActive(false);
            PhotonNetwork.AutomaticallySyncScene = true;
            quickStartButton.SetActive(false);
            quickCancelButton.SetActive(true);
            PhotonNetwork.JoinRoom("Tournament");
        }
        else
        {
            connectImage.SetActive(true);
            quickStartButton.GetComponent<Button>().enabled = false;
            quickCancelButton.SetActive(false);
            PhotonNetwork.ConnectUsingSettings();
        }

    }

    /// <summary>
    /// If we fail to create a room then this function attempts to create another
    /// </summary>
    /// <param name="returnCode"></param>
    /// <param name="message"></param>
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to create room... trying again");
        CreateRoom();
    }

    /// <summary>
    /// Function attached to the cancel button - Cancels room search, room creation and disconnects from the server - After disconnect NetworkController will atomatically try to reconnect
    /// </summary>
    public void QuickCancel()
    {
        SaveLoad.instance.tournamentHost = false;
        PhotonNetwork.AutomaticallySyncScene = false;
        quickCancelButton.SetActive(false);
        quickStartButton.SetActive(true);
        PhotonNetwork.LeaveRoom();
        roomController.numberOfPlayersInRoom = 0;
    }
}