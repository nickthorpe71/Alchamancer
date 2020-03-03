using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System.IO;

/// <summary>
/// Script for carring general info between scenes
/// </summary>
public class Carry : MonoBehaviourPunCallbacks
{
    //Creates a singleton that allows this to be carried through each scene
    public static Carry instance = null;

    //Stores a scriptable object with info about the level
    public LevelSO levelInfo;

    public int levelScene = 1;
    public int currentScene;

    //The string that populates the starting elements
    public string levelString;

    //If true the AI will be active in multiplayer
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
