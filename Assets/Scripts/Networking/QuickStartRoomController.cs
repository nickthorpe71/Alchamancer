using Photon.Pun;
using UnityEngine;
public class QuickStartRoomController : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private int multiplayerSceneIndex; //Number for the build index to the multiplay scene.

    public int numberOfPlayersInRoom;

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

    void CheckStart()
    {
        if (numberOfPlayersInRoom >= 2)
            StartGame();
    }

    public override void OnJoinedRoom() //Callback function for when we successfully create or join a room.
    {
        IncreasePlayers();
        Debug.Log("Joined Room");
    }

    public void IncreasePlayers()
    {
        base.photonView.RPC("RPC_IncreasePlayers", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void RPC_IncreasePlayers()
    {
        numberOfPlayersInRoom++;
    }

    private void StartGame() //Function for loading into the multiplayer scene.
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Starting Game");
            Carry.instance.levelInfo = TwoPlayerMapGen.instance.MapGen();
            Carry.instance.levelString = Carry.instance.levelInfo.startString;
            base.photonView.RPC("RPC_SendLevelString", RpcTarget.AllBuffered, Carry.instance.levelInfo.startString);
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.LoadLevel(multiplayerSceneIndex); //because of AutoSyncScene all players who join the room will also be loaded into the multiplayer scene.
        }
    }

    [PunRPC]
    public void RPC_SendLevelString(string levelString)
    {
        Carry.instance.levelString = levelString;
    }
}
