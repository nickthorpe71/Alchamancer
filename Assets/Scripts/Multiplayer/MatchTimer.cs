using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Allows the tournament host to activate a timer that counts down from a set time and end the game at the end of that count down
/// </summary>
public class MatchTimer : MonoBehaviour
{
    public Text timeText;
    private float minutes = 1;
    private float seconds = 30;
    public bool matchOver;

    private void Start()
    {
        timeText.text = "1:30";
        InvokeRepeating("TimeCheck", 2, 1);
    }

    /// <summary>
    /// Removes a second from current time and checks if time has run out
    /// </summary>
    void TimeCheck()
    {
        if (!matchOver)
        {
            seconds--;
            timeText.text = TimeConvert();

            if (minutes == 0 && seconds < 10)
                timeText.color = Color.red;

            if (minutes <= 0 && seconds <= 0)
            {
                if(!SaveLoad.instance.tournamentHost)
                    CheckResults();
            }

        }
    }

    /// <summary>
    /// Converts total minutes and seconds to 0:00 format string
    /// </summary>
    string TimeConvert()
    {
        if (seconds <= 0)
        {
            if (minutes > 0)
            {
                minutes--;
                seconds = 59;
            }
            else
            {
                seconds = 0;
            }
        }

        string newTime;

        if(seconds < 10)
            newTime = minutes + ":0" + seconds;
        else
            newTime = minutes + ":" + seconds;

        return newTime;
    }

    /// <summary>
    /// When time has run out this checks if the local player has less health - if so it triggers a loss for them
    /// </summary>
    public void CheckResults()
    {
        matchOver = true;

        GameManager.instance.playerControl.canMove = false;

        if(StatsManager.instance.myHP <= StatsManager.instance.theirHP)
        {
            GameManager.instance.GameOver();
        }
    }
}
