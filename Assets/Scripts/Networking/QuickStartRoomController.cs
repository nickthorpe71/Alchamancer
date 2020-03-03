using Photon.Pun;
using UnityEngine;
using Photon.Realtime;
public class QuickStartRoomController : MonoBehaviourPunCallbacks
{
    /// <summary>
    /// Holds players in the room and starts the match once there are enough players
    /// </summary>
    [SerializeField]
    private int multiplayerSceneIndex; //Number for the build index to the multiplayer scene

    public int numberOfPlayersInRoom; //Current number of players in the scene

    public bool isTournament; //Where this match s a tournament match

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }
    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    private void Start()
    {
        InvokeRepeating("CheckStart", 1, 1);
    }

    /// <summary>
    /// Check to see if there are enough player in the room to start the match
    /// </summary>
    void CheckStart()
    {
        if (isTournament)
        {
            if (numberOfPlayersInRoom >= 3)
                StartGame();
        }
        else
        {
            if (numberOfPlayersInRoom >= 2)
                StartGame();
        }
    }
    /// <summary>
    /// Callback function for when we successfully create or join a room
    /// </summary>
    public override void OnJoinedRoom()
    {
        IncreasePlayers();
        Debug.Log("Joined Room");
    }

    /// <summary>
    /// Sends a buffered message to increase the amount of players in the room when this local player joins
    /// </summary>
    public void IncreasePlayers()
    {
        base.photonView.RPC("RPC_IncreasePlayers", RpcTarget.AllBuffered);
    }

    /// <summary>
    /// Receives a message to increase number of player in room when another player joins
    /// </summary>
    [PunRPC]
    public void RPC_IncreasePlayers()
    {
        numberOfPlayersInRoom++;
        Debug.Log("New Player Joined. Total: " + numberOfPlayersInRoom);

        if(numberOfPlayersInRoom > 1)
            PhotonNetwork.SetMasterClient(PhotonNetwork.PlayerList[1]);
    }

    /// <summary>
    /// Function for loading into the multiplayer scene
    /// </summary>
    private void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Starting Game");
            Carry.instance.levelInfo = TwoPlayerMapGen.instance.MapGen();
            Carry.instance.levelString = Carry.instance.levelInfo.startString;
            base.photonView.RPC("RPC_SendLevelString", RpcTarget.AllBuffered, Carry.instance.levelInfo.startString); //Master client sends the starting map layout string to other player
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.LoadLevel(multiplayerSceneIndex); //Because of AutoSyncScene all players who join the room will also be loaded into the multiplayer scene.
        }
    }

    /// <summary>
    /// Receives starting map layout string from master client
    /// </summary>
    /// <param name="levelString"></param>
    [PunRPC]
    public void RPC_SendLevelString(string levelString)
    {
        Carry.instance.levelString = levelString;
    }
}
