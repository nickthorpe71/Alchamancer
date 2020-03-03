using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

/// <summary>
/// Handles sound for the entire game
/// </summary>
public class SoundManager : MonoBehaviourPunCallbacks
{
    public static SoundManager instance = null; //To allow this object to persist scene to scene

    public GameManager gameManager; //Reference to the game manager in multiplayer matches

    public AudioSource musicSource; //Reference to main music source componenet on SoundManager

    public float volumeMod; //To increase overall volume

    public GameObject prefabSFX; //Instance of a gameoject that is created when SFX are played in game

    public SpellListSO spellList; //List of all spells to create a dictionary of cast sounds
    
    private Dictionary<string, AudioClip> castSoundsDict = new Dictionary<string, AudioClip>(); //Dictionary of cast sounds

    //+ or - 0.5 of the original pitch
    public float lowPitchRange = 0.95f;
    public float highPitchRange = 1.05f;

    //Sounds that need to be stored directly on the SoundManager
    public AudioClip preCastSound;
    public AudioClip counterSound;
    public AudioClip clickSound;
    public AudioClip takeSound;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SetCastDictionary();
    }

    /// <summary>
    /// Populate dictionary of all spell names and their cast sounds - Also including some sounds stored directly on SoundManager
    /// </summary>
    void SetCastDictionary()
    {
        foreach (SpellSO spell in spellList.spells)
        {
            castSoundsDict.Add(spell.spellName , spell.castSound);
        }

        castSoundsDict.Add("preCastSound", preCastSound);
        castSoundsDict.Add("counterSound", counterSound);
        castSoundsDict.Add("takeSound", takeSound);
    }

    /// <summary>
    /// Used to play a button click from other scripts 
    /// </summary>
    public void PlayButtonClick()
    {
        PlaySingle(clickSound, 0.75f);
    }

    /// <summary>
    /// Playes a sound clip at the specified volume percent passed in
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="volume"></param>
    public void PlaySingle(AudioClip clip, float volume)
    {
        //Creates a SFX object to allow sound effects to overlap
        GameObject tempSFX = Instantiate(prefabSFX, transform.position, Quaternion.identity);

        tempSFX.GetComponent<AudioSource>().clip = clip;
        tempSFX.GetComponent<AudioSource>().volume = volume * volumeMod;
        tempSFX.GetComponent<AudioSource>().Play();
    }

    /// <summary>
    /// Playes a sound clip at the specified volume percent passed in to all players in game - the actual RPC call is in the game manager
    /// because there cannot be a PhotonView on persistant objects(this) 
    /// </summary>
    /// <param name="clipName"></param>
    /// <param name="volume"></param>
    public void PlaySinglePublic(string clipName, float volume)
    {
        if(gameManager != null)
            gameManager.PlayPublicSingle(clipName, volume);
    }

    /// <summary>
    /// The game manager passes the info back to this function on the sound manager for both players in game
    /// </summary>
    /// <param name="clipName"></param>
    /// <param name="volume"></param>
    public void RPC_PlaySinglePublic(string clipName, float volume)
    {
        GameObject tempSFX = Instantiate(prefabSFX, transform.position, Quaternion.identity);

        tempSFX.GetComponent<AudioSource>().clip = castSoundsDict[clipName];
        tempSFX.GetComponent<AudioSource>().volume = volume * volumeMod;
        tempSFX.GetComponent<AudioSource>().Play();
    }

    /// <summary>
    /// Play random backing music from the list of clips
    /// </summary>
    /// <param name="tracks"></param>
    public void RandomizeMusic(params AudioClip[] tracks)
    {
        int randomIndex = Random.Range(0, tracks.Length);
        musicSource.clip = tracks[randomIndex];
        musicSource.Play();
    }

    /// <summary>
    /// Play a random SFX from list of clips
    /// </summary>
    /// <param name="clips"></param>
    public void RandomizeSfx (params AudioClip [] clips)
    {
        int randomIndex = Random.Range(0, clips.Length);

        GameObject tempSFX = Instantiate(prefabSFX, transform.position, Quaternion.identity);

        tempSFX.GetComponent<AudioSource>().clip = clips[randomIndex];
        tempSFX.GetComponent<AudioSource>().volume = 1 * volumeMod;
        tempSFX.GetComponent<AudioSource>().Play();
    }
}
