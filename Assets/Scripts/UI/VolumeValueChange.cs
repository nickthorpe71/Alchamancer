using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeValueChange : MonoBehaviour
{
    public AudioSource audioSorc;

    public AudioClip[] testClips;

    public Slider sfxSlider;
    public Slider musicSlider;

    private float musicVolume = 1f;
    private float sfxMod = 1f;

    private SaveLoad saveLoad;
    private SoundManager soundManager;

    void Start()
    {
        saveLoad = SaveLoad.instance;
        soundManager = SoundManager.instance;

        sfxSlider.value = saveLoad.sfxVolume;
        musicSlider.value = saveLoad.musicVolume;

        audioSorc = soundManager.musicSource;
    }

    void Update()
    {
        audioSorc.volume = musicVolume;
        soundManager.volumeMod = sfxMod;
    }

    public void SetMusicVolume(float vol)
    {
        musicVolume = vol;
        //audioSorc.volume = musicVolume;
    }

    public void SetSFXVolume(float vol)
    {
        sfxMod = vol;
    }

    public void SFXTest()
    {
        soundManager.RandomizeSfx(testClips);
    }

    public void SaveSettings()
    {
        soundManager.PlayButtonClick();

        saveLoad.sfxVolume = sfxMod;
        saveLoad.musicVolume = musicVolume;
        saveLoad.Save();
    }
}
