using Photon.Pun;
using UnityEngine;

/// <summary>
/// Handles initial connection to the Photon Server
/// </summary>
public class NetworkController : MonoBehaviourPunCallbacks
{

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();

        InvokeRepeating("ConnectionAttempt", 0, 2);
    }

    void ConnectionAttempt()
    {
        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("We are now connected to the " + PhotonNetwork.CloudRegion + " server!");
    }
}
