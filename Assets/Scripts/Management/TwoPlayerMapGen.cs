using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// Contains different methods for creating a string to generate starting map
/// </summary>
public class TwoPlayerMapGen : MonoBehaviourPunCallbacks
{
    public static TwoPlayerMapGen instance; //Allows this scrips to be easily accessed by other scripts in the scene

    public string finalString; //The string that will be used to create the map

    public LevelSO random2Plevel; //Used to transfer the string

    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// Called to create a LevelSO - currently purely to generate a starting string
    /// </summary>
    public LevelSO MapGen()
    {
        random2Plevel = new LevelSO();

        random2Plevel.startString = MapStringSemetricalRandom(); //Generate teh starting string

        return random2Plevel;
    }

    /// <summary>
    /// Creates a starting map string that is completely random
    /// </summary>
    /// <returns></returns>
    public string MapStringFullRandom()
    {
        finalString = "";
        string lastTemp = "0";
        string temp = "0";


        for (int i = 0; i < 49; i++)
        {
            lastTemp = temp;

            int roll = UnityEngine.Random.Range(1, 100);

            if (roll <= 95)
            {
                if (temp == lastTemp)
                {
                    while (temp == lastTemp)
                        temp = UnityEngine.Random.Range(1, 8).ToString();
                }
            }
            else
            {
                temp = "1";
            }

            if (i == 0)
            {
                finalString += temp;
            }
            else if (i == 21 || i == 22 || i == 27 || i == 26)
            {
                finalString += ",1";
            }
            else
            {
                finalString += "," + temp;
            }
        }

        return finalString;
    }

    /// <summary>
    /// Creates a starting map string that is random but semetrical 
    /// </summary>
    public string MapStringSemetricalRandom()
    {
        finalString = "";
        string one = "0";
        string two = "0";
        string three = "0";
        string four = "0";


        for (int i = 0; i < 7; i++)
        {
            one = UnityEngine.Random.Range(2, 8).ToString();
            two = UnityEngine.Random.Range(2, 8).ToString();
            three = UnityEngine.Random.Range(2, 8).ToString();
            four = UnityEngine.Random.Range(2, 8).ToString();

            if (i == 0)
            {
                finalString += one + "," + two + "," + three + "," + four + "," + three + "," + two + "," + one;
            }
            else if (i == 3)
            {
                finalString += ",1,1,1,8,1,1,1"; //makes the middle row clear except a blocker in the middle 
            }
            else
            {
                finalString += "," + one + "," + two + "," + three + "," + four + "," + three + "," + two + "," + one;
            }
        }

        return finalString;
    }


}
