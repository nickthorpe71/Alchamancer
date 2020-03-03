using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Configured each individual leaderboard listing
/// </summary>
public class ScoreListing : MonoBehaviour
{
    public Text rpText;
    public Text nameText;

    public Color eldar;
    public Color archsage;
    public Color sage;
    public Color scribe;
    public Color acolyte;


    public Image BG;

    /// <summary>
    /// Sets name, rank points and background color UI display for this score listing
    /// </summary>
    /// <param name="_rp"></param>
    /// <param name="_name"></param>
    public void Initialize(int _rp, string _name)
    {
        rpText.text = _rp.ToString();

        string newName = "";

        //remove + from database names
        for (int i = 0; i < _name.Length; i++)
        {

            if (_name[i] == '+')
                newName += " ";
            else
                newName += _name[i];
        }

        nameText.text = newName;

        if (_rp > 4999)
            BG.color = eldar;
        else if (_rp < 5000 && _rp > 3499)
            BG.color = archsage;
        else if (_rp < 3500 && _rp > 2499)
            BG.color = sage;
        else if (_rp < 2500 && _rp > 1699)
            BG.color = scribe;
        else if (_rp < 1700)
            BG.color = acolyte;
    }
}
