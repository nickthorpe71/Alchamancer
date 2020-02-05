using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Database : MonoBehaviour
{
    const string privateCode = "XpXa0oOgz0Cn31iCgvP6zww_IStnP3Q0Sn-CyW1Iv43A";
    const string publicCode = "5debdc7fb5622f683c282d00";
    const string webURL = "https://www.dreamlo.com/lb/";

    public static Database instance;
    public bool downloadComplete;

    public Row[] rows;
    public string[] newsArr;
    public string currentVersion;
    public string totalDonation;

    private void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        Download();
    }

    private void Start()
    {
        newsArr = new string[2];
        newsArr[0] = "";
        newsArr[1] = "";
    }

    public static void AddNewRow(string username, int rp, int extra, string ID)
    {
        instance.StartCoroutine(instance.UploadNewRow(username, rp, extra, ID));
    }

    IEnumerator UploadNewRow(string username, int rp, int extra, string ID)
    {
        WWW www = new WWW(webURL + privateCode + "/add/" + WWW.EscapeURL(username) + "/" + rp + "/" + extra + "/" + ID);
        yield return www;

        if (string.IsNullOrEmpty(www.error))
        {
            print("Upload Successful");
        }
        else
            print("Error uploading " + www.error);
    }

    public static void RemoveRow(string username)
    {
        instance.StartCoroutine(instance.RemoveRowByUsername(username));
    }

    IEnumerator RemoveRowByUsername(string username)
    {
        WWW www = new WWW(webURL + privateCode + "/delete/" + WWW.EscapeURL(username));
        yield return www;

        if (string.IsNullOrEmpty(www.error))
        {
            print("Removal Successful");
        }
        else
            print("Error removing " + www.error);
    }

    public static void ClearAll()
    {
        WWW www = new WWW(webURL + privateCode + "/clear");
    }

    public void Download()
    {
        downloadComplete = false;
        StartCoroutine(DownloadFromDatabase());
    }

    IEnumerator DownloadFromDatabase()
    {
        WWW www = new WWW(webURL + privateCode + "/pipe/");
        yield return www;

        if (string.IsNullOrEmpty(www.error))
        {
            FormatScores(www.text);
        }
        else
            print("Error downloading " + www.error);

    }

    public void FormatScores(string textStream)
    {
        string[] entries = textStream.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        rows = new Row[entries.Length];

        for (int i = 0; i < entries.Length; i++)
        {
            string[] entryInfo = entries[i].Split(new char[] { '|' });
            string username = entryInfo[0];
            int rp = int.Parse(entryInfo[1]);
            int extra = int.Parse(entryInfo[2]);
            string ID = entryInfo[3];

            rows[i] = new Row(username, rp, extra, ID);

            Scene currentScene = SceneManager.GetActiveScene();

            if(currentScene.name == "LeaderBoard")
                print(rows[i].username + " : " + rows[i].rp + " : " + extra + " : " + ID);

            if (username == "News1")
                newsArr[0] = ID;

            if (username == "News2")
                newsArr[1] = ID;

            if (username == "Version")
                currentVersion = ID;

            if (username == "Alchamancer")
                totalDonation = ID;
        }

        downloadComplete = true;
    }

    public void SendScores(DisplayScores target)
    {
        StartCoroutine(CheckDownloadScores(target));
    }

    IEnumerator CheckDownloadScores(DisplayScores target)
    {
        while (!downloadComplete)
            yield return null;

        if(target!= null)
            target.OnDownloadedScores(rows);
    }

    public void SendNames(ChangeName target)
    {
        StartCoroutine(CheckDownloadNames(target));
    }

    IEnumerator CheckDownloadNames(ChangeName target)
    {
        while (!downloadComplete)
            yield return null;

        if (target != null)
            target.OnDownloadNames(rows);
    }

    public void SendAlchamancer(AlchaLevels target)
    {
        StartCoroutine(CheckDownloadAlchamancer(target));
    }

    IEnumerator CheckDownloadAlchamancer(AlchaLevels target)
    {
        while (!downloadComplete)
            yield return null;

        if (target != null)
            target.SetInfo(rows);
    }

    public void SendNewsAndVersion(MainMenu target)
    {
        StartCoroutine(CheckDownloadNewsVersion(target));
    }

    IEnumerator CheckDownloadNewsVersion(MainMenu target)
    {
        while (!downloadComplete)
            yield return null;

        if (target != null)
            target.OnDownloadNews(rows);
    }

    public string FormatUsername(string username)
    {
        string dataFormat = "";

        for (int i = 0; i < username.Length; i++)
        {
            if (username[i] == ' ')
                dataFormat += "+";
            else
                dataFormat += username[i];
        }
        return dataFormat;
    }
}



public struct Row
{
    public string username;
    public int rp;
    public int extra;
    public string ID;

    public Row (string _username, int _rp, int _extra, string _ID)
    {
        username = _username;
        rp = _rp;
        extra = _extra;
        ID = _ID;
    }
}

