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

                    if (rows[i].username == SaveLoad.instance.playerName)
                        myScore.Initialize(rows[i].rp, rows[i].username);
                }
            }
        }

        if (loadingObj.activeSelf && loadingObj != null)
            loadingObj.SetActive(false);

        
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
