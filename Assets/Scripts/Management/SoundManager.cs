using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class SoundManager : MonoBehaviourPunCallbacks
{
    public AudioSource musicSource;

    public GameManager gameManager;

    public float volumeMod;

    public GameObject prefabSFX;

    public static SoundManager instance = null;

    public SpellListSO spellList;
    //private List<AudioClip> castSoundList = new List<AudioClip>();
    private Dictionary<string, AudioClip> castSoundsDict = new Dictionary<string, AudioClip>();

    //+ or - 0.5 of the original pitch
    public float lowPitchRange = 0.95f;
    public float highPitchRange = 1.05f;

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

    public void PlayButtonClick()
    {
        PlaySingle(clickSound, 0.75f);
    }

    public void PlaySingle(AudioClip clip, float volume)
    {
        GameObject tempSFX = Instantiate(prefabSFX, transform.position, Quaternion.identity);

        tempSFX.GetComponent<AudioSource>().clip = clip;
        tempSFX.GetComponent<AudioSource>().volume = volume * volumeMod;
        tempSFX.GetComponent<AudioSource>().Play();
    }
    
    public void PlaySinglePublic(string clipName, float volume)
    {
        if(gameManager != null)
            gameManager.PlayPublicSingle(clipName, volume);
    }

    public void RPC_PlaySinglePublic(string clipName, float volume)
    {
        GameObject tempSFX = Instantiate(prefabSFX, transform.position, Quaternion.identity);

        tempSFX.GetComponent<AudioSource>().clip = castSoundsDict[clipName];
        tempSFX.GetComponent<AudioSource>().volume = volume * volumeMod;
        tempSFX.GetComponent<AudioSource>().Play();
    }

    public void RandomizeMusic(params AudioClip[] tracks)
    {
        int randomIndex = Random.Range(0, tracks.Length);
        musicSource.clip = tracks[randomIndex];
        musicSource.Play();
    }

    //params allows you to send any number of a specific variable (in this case AudioClip)
    //through as paramaters as long as they are separated by a comma.
    public void RandomizeSfx (params AudioClip [] clips)
    {
        int randomIndex = Random.Range(0, clips.Length);

        GameObject tempSFX = Instantiate(prefabSFX, transform.position, Quaternion.identity);

        tempSFX.GetComponent<AudioSource>().clip = clips[randomIndex];
        tempSFX.GetComponent<AudioSource>().volume = 1 * volumeMod;
        tempSFX.GetComponent<AudioSource>().Play();
    }
}
