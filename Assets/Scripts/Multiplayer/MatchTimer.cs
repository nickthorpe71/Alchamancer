using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchTimer : MonoBehaviour
{
    public Text timeText;
    private float minutes = 1;
    private float seconds = 0;
    public bool matchOver;

    private void Start()
    {
        timeText.text = "7:00";
        InvokeRepeating("TimeCheck", 5, 1);
    }

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
