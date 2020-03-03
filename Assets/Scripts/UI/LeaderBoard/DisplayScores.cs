using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays all score UI on leaderboard scene
/// </summary>
public class DisplayScores : MonoBehaviour
{
    public ScoreListing scoreListing;

    public ScoreListing myScore;
    public Transform content;
    private List<GameObject> scoreListings = new List<GameObject>();
    Database database;

    public GameObject loadingObj;

    private void Start()
    {
        database = Database.instance;
        StartCoroutine(RefreshScores());
    }

    /// <summary>
    /// Attempts to download top 100 scores from database
    /// </summary>
    /// <param name="rows"></param>
    public void OnDownloadedScores(Row[] rows)
    {
        for (int i = 0; i < rows.Length; i++)
        {
            if (rows[i].username != "News1" && rows[i].username != "News2" && rows[i].username != "Version" && rows[i].username != "Alchamancer" && rows[i].username != "Gaspar" && rows[i].username != "Lost+One")
            {
                ScoreListing listing = Instantiate(scoreListing, content);
                scoreListings.Add(listing.gameObject);

                if (listing != null)
                {
                    listing.Initialize(rows[i].rp, rows[i].username);
                }
            }
        }

        StartCoroutine(SetMyScoreRoutine());

        if (loadingObj.activeSelf && loadingObj != null)
            loadingObj.SetActive(false);
    }

    /// <summary>
    /// Attempts to download the score for the local player
    /// </summary>
    /// <returns></returns>
    public IEnumerator SetMyScoreRoutine()
    {
        WWW www = new WWW(database.webURL + database.privateCode + "/pipe-get/" + WWW.EscapeURL(SaveLoad.instance.playerName));
        yield return www;

        if (string.IsNullOrEmpty(www.error))
        {
            SetMyScore(www.text);
        }
        else
            print("Error pulling Alchamancer info " + www.error);
    }

    /// <summary>
    /// Sets UI display for local players score
    /// </summary>
    /// <param name="textStream"></param>
    public void SetMyScore(string textStream)
    {
        string[] entryInfo = textStream.Split(new char[] { '|' });

        string username = entryInfo[0];
        int rp = int.Parse(entryInfo[1]);
        int extra = int.Parse(entryInfo[2]);
        string ID = entryInfo[3];

        Row row = new Row(username, rp, extra, ID);

        myScore.Initialize(rp, username);
    }

    /// <summary>
    /// Downloads and updates leaderboard listing
    /// </summary>
    /// <returns></returns>
    IEnumerator RefreshScores()
    {
        while(true)
        {
            for (int i = 0; i < scoreListings.Count; i++)
            {
                Destroy(scoreListings[i]);
            }

            scoreListings.Clear();

            database.Download();
            database.SendScores(this);
            yield return new WaitForSeconds(30);

        }
    }
}
