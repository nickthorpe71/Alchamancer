﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main script for New Profile scene
/// </summary>
public class NewProfileScreen : MonoBehaviour
{
    public GameObject nameObj;
    public GameObject appearanceObj;

    void Start()
    {
        nameObj.SetActive(true);
        appearanceObj.SetActive(false);
    }

    public void SwitchToAppearance()
    {
        nameObj.SetActive(false);
        appearanceObj.SetActive(true);
    }
}
