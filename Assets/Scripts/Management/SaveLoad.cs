using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.UI;

/// <summary>
/// Main script for saving and loading data locally
/// </summary>
public class SaveLoad : MonoBehaviour
{
    public static SaveLoad instance;

    //** Saved Variables **\\
    public int playerRP; //Record of users Rank Points
    public string playerName; //Record of players username
    public int characterSkin; //Record of which skin the player has selected
    [HideInInspector] public int numOfAvailableSkins = 14; //Number of total available skins
    public Dictionary<int, bool> skinAvailability = new Dictionary<int, bool>(); //A dictionary that determines whether the skin is available to this user
    public bool doneTutorial; //Whether the user has gone through the initial tutorial

    public float sfxVolume = 1; //Saved setting of the SFX volume
    public float musicVolume = 1; //Saves setting of music volume


    //** Temporary Variables **\\
    public int otherPlayerRP; //After a match other players Rank Points are stored here temporarily
    public string otherPlayerName; //After a match other players Username are stored here temporarily
    public bool onlineMatch; //Whether match last player was online or not

    public bool tournamentHost; //Whether this user is hosting a tournament

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

        Load();

        if (doneTutorial)
        {
            SoundManager.instance.musicSource.volume = musicVolume;
            SoundManager.instance.volumeMod = sfxVolume;
        }
    }

    private void Start()
    {
        if(skinAvailability.Count < numOfAvailableSkins)
            InitializeSkins();

        if (!doneTutorial)
        {
            skinAvailability[0] = true;
            playerRP = 1000;
            Save();
        }
    }

    /// <summary>
    /// Initializez skins dictionary to correct size
    /// </summary>
    private void InitializeSkins()
    {
        for (int i = 0; i < numOfAvailableSkins; i++)
        {
            if(!skinAvailability.ContainsKey(i))
                skinAvailability.Add(i, false);
        }
    }

    /// <summary>
    /// Function for saving data locally
    /// </summary>
    public void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/playerInfo.dat");

        PlayerData data = new PlayerData();

        data.playerName = playerName;
        data.characterSkin = characterSkin;
        data.playerRP = playerRP;
        data.doneTutorial = doneTutorial;

        data.sfxVolume = sfxVolume;
        data.musicVolume = musicVolume;

        data.skinAvailability = skinAvailability;

        bf.Serialize(file, data);
        file.Close();
    }

    /// <summary>
    /// Function for saving loading data from persistant local data path
    /// </summary>
    public void Load()
    {
        if (File.Exists(Application.persistentDataPath + "/playerInfo.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/playerInfo.dat", FileMode.Open);
            PlayerData data = (PlayerData)bf.Deserialize(file);
            file.Close();

            playerName = data.playerName;
            characterSkin = data.characterSkin;
            playerRP = data.playerRP;
            doneTutorial = data.doneTutorial;

            sfxVolume = data.sfxVolume;
            musicVolume = data.musicVolume;

            skinAvailability = data.skinAvailability;
        }
    }
}

/// <summary>
/// All locally stored player data
/// </summary>
[Serializable]
class PlayerData
{
    public string playerName;
    public int characterSkin;
    public int playerRP;
    public bool doneTutorial;
    public float sfxVolume;
    public float musicVolume;

    public Dictionary<int, bool> skinAvailability;
}
