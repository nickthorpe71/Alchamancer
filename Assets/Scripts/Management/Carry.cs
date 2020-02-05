using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System.IO;

public class Carry : MonoBehaviourPunCallbacks
{
    public static Carry instance = null;

    public LevelSO levelInfo;

    public int levelScene = 1;
    public int currentScene;

    public bool isOnline;

    public string levelString;

    public bool aIAcvive;

    void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

}
