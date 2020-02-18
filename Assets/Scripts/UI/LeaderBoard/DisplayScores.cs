using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

        StartSetMyScore();

        if (loadingObj.activeSelf && loadingObj != null)
            loadingObj.SetActive(false);
    }

    public void StartSetMyScore()
    {
        StartCoroutine(SetMyScoreRoutine());
    }

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
