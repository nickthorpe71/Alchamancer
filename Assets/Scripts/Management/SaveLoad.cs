using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.UI;

public class SaveLoad : MonoBehaviour
{
    public static SaveLoad instance;

    public int playerRP;
    public string playerName;
    public int characterSkin;
    [HideInInspector] public int numOfAvailableSkins = 14;
    public Dictionary<int, bool> skinAvailability = new Dictionary<int, bool>();
    public bool doneTutorial;

    public float sfxVolume = 1;
    public float musicVolume = 1;

    public int otherPlayerRP;
    public string otherPlayerName;
    public bool onlineMatch;

    public bool tournamentHost;

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

    private void InitializeSkins()
    {
        for (int i = 0; i < numOfAvailableSkins; i++)
        {
            if(!skinAvailability.ContainsKey(i))
                skinAvailability.Add(i, false);
        }
    }

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
