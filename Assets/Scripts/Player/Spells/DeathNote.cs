using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathNote : MonoBehaviour
{
    [Header ("Death Note")]
    public float timeBeforeDeath;

    void Start()
    {
        Destroy(this.gameObject, timeBeforeDeath);
    }

}
