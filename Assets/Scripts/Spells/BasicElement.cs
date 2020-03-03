using System.Collections;
using Photon.Pun;
using UnityEngine;

/// <summary>
/// Script placed on each element prefab to trigger their sound effect
/// </summary>
public class BasicElement : MonoBehaviour
{
    public AudioClip sound;

    private void Start()
    {
        if (Time.timeSinceLevelLoad > 0.5f)
            SoundManager.instance.PlaySingle(sound, 0.75f);
        else
            SoundManager.instance.PlaySingle(sound, 0.1f);
    }
}
