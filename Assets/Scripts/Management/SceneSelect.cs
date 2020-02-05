using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSelect : MonoBehaviour
{
    public static SceneSelect instance;
    private SoundManager soundManager;

    private void Start()
    {
        instance = this;
        soundManager = SoundManager.instance;
    }

    public void Forest()
    {
        LeaveRoomAndLobby();
        soundManager.PlayButtonClick();
    }

    public void SinglePlayer()
    {
        LeaveRoomAndLobby();
        soundManager.PlayButtonClick();
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("SinglePlayer");
    }

    public void MainMenuButton()
    {
        SceneManager.LoadScene("MainMenu");
        PhotonNetwork.AutomaticallySyncScene = false;

        if(soundManager != null)
            soundManager.PlayButtonClick();
    }

    public void Profile()
    {
        LeaveRoomAndLobby();
        SceneManager.LoadScene("Profile");
        soundManager.PlayButtonClick();
    }

    public void LeaderBoard()
    {
        LeaveRoomAndLobby();
        SceneManager.LoadScene("LeaderBoard");
        soundManager.PlayButtonClick();
    }

    public void Codes()
    {
        LeaveRoomAndLobby();
        SceneManager.LoadScene("Codes");
        soundManager.PlayButtonClick();
    }

    public void NewProfile()
    {
        LeaveRoomAndLobby();
        SceneManager.LoadScene("NewProfile");
        soundManager.PlayButtonClick();
    }

    public void TutorialScreens()
    {
        LeaveRoomAndLobby();
        SceneManager.LoadScene("TutorialScreens");
        soundManager.PlayButtonClick();
    }

    public void Contact()
    {
        LeaveRoomAndLobby();
        SceneManager.LoadScene("Contact");
        soundManager.PlayButtonClick();
    }

    public void Donate()
    {
        LeaveRoomAndLobby();
        SceneManager.LoadScene("Donate");
        soundManager.PlayButtonClick();
    }

    public void WinScreen()
    {
        LeaveRoomAndLobby();
        SceneManager.LoadScene("WinScreen");
    }

    public void LoseScreen()
    {
        LeaveRoomAndLobby();
        SceneManager.LoadScene("LoseScreen");
    }

    public void Spells()
    {
        LeaveRoomAndLobby();
        SceneManager.LoadScene("Spells");
        soundManager.PlayButtonClick();
    }

    public void AddNews()
    {
        LeaveRoomAndLobby();
        SceneManager.LoadScene("AddNews");
        soundManager.PlayButtonClick();
    }

    public void Settings()
    {
        LeaveRoomAndLobby();
        SceneManager.LoadScene("Settings");
        soundManager.PlayButtonClick();
    }

    public void EndGame()
    {
        Application.Quit();
        soundManager.PlayButtonClick();
    }

    public void LeaveRoomAndLobby()
    {
        if (PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();
    }
}
