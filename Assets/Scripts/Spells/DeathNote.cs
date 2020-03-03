using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to destroy a game object after a specified number of seconds
/// </summary>
public class DeathNote : MonoBehaviour
{
    [Header ("Death Note")]
    public float timeBeforeDeath;

    void Start()
    {
        Destroy(this.gameObject, timeBeforeDeath);
    }

}
