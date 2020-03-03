using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Screen that only the developer has access to - allows the input of alchanews and adjusting player scores
/// </summary>
public class AddNewsScreen : MonoBehaviour
{
    public Text news1text; //Field to type in news 1
    public Text news2text; //Field to type in news 2

    public Text username; //Username input field
    public Text amount; //Amount fo Rank Points to increase specific user by

    public GameObject news1Obj;
    public GameObject news2Obj;

    public GameObject resetMenu;

    public Text amountOfExp;

    [SerializeField] TextAsset allUsersFile;
    public List<string> allUsers = new List<string>();

    public List<Row> allRows = new List<Row>();

    private void Start()
    {
        Database.instance.Download();
    }

    /// <summary>
    /// Loads the current news into the news1 field
    /// </summary>
    public void LoadOldNews1()
    {
        news1Obj.gameObject.GetComponent<InputField>().text = Database.instance.rows[1].username;
    }

    /// <summary>
    /// Loads the current news into the news2 field
    /// </summary>
    public void LoadOldNews2()
    {
        news2Obj.gameObject.GetComponent<InputField>().text = Database.instance.rows[2].username;
    }

    /// <summary>
    /// Uploads text in the news1 field to the database replacing the previous news1
    /// </summary>
    public void Upload1()
    {
        Database.AddNewRow("News1", 11000, 0, news1text.text);
    }

    /// <summary>
    /// Uploads text in the news2 field to the database replacing the previous news2
    /// </summary>
    public void Upload2()
    {
        Database.AddNewRow("News2", 11000, 0, news2text.text);
    }

    /// <summary>
    /// Uploads the version number of the instance of the game being run to database replacing current version number
    /// </summary>
    public void UpdateVersion()
    {
        Database.AddNewRow("Version", 11000, 0, Application.version);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            DownloadForBackUp();
    }

    /// <summary>
    /// Opens/Closes the reset leaderboard menu
    /// </summary>
    public void ResetMenu()
    {
        if (resetMenu.activeSelf)
            resetMenu.SetActive(false);
        else if (!resetMenu.activeSelf)
            resetMenu.SetActive(true);
    }

    /// <summary>
    /// Starts the routine to reset the leaderboard
    /// </summary>
    public void ResetLeaderboards()
    {
        StartCoroutine(ResetLeaderboardsRoutine());
    }

    /// <summary>
    /// Resets the entire leaderboard!
    /// </summary>
    public IEnumerator ResetLeaderboardsRoutine()
    {
        Row[] rowStore = Database.instance.rows;

        for (int i = 0; i < rowStore.Length; i++)
        {
            if (rowStore[i].username != "News1" && rowStore[i].username != "News2" && rowStore[i].username != "Version" && rowStore[i].username != "Alchamancer")
                Database.RemoveRow(rowStore[i].username);
        }

        yield return new WaitForSeconds(15);

        for (int i = 0; i < rowStore.Length; i++)
        {
            if (rowStore[i].username != "News1" && rowStore[i].username != "News2" && rowStore[i].username != "Version" && rowStore[i].username != "Alchamancer")
                Database.AddNewRow(rowStore[i].username, 1000, rowStore[i].extra, rowStore[i].ID);
        }

        yield return new WaitForSeconds(10);

        if (resetMenu.activeSelf)
            resetMenu.SetActive(false);
    }

    /// <summary>
    /// Starts a routine to add Rank Points to a specific player
    /// </summary>
    public void AddRP()
    {
        StartCoroutine(AddRPRoutine());
    }

    /// <summary>
    /// Routine that pulls down the user who's username is in the username field
    /// </summary>
    /// <returns></returns>
    public IEnumerator AddRPRoutine()
    {
        WWW www = new WWW(Database.instance.webURL + Database.instance.privateCode + "/pipe-get/" + WWW.EscapeURL(username.text));
        yield return www;

        if (string.IsNullOrEmpty(www.error))
        {
            if(www.text == "")
                AddRPNotFound();
            else
                AddRPFound(www.text);
        }
        else
        {
            print("Error pulling Alchamancer info " + www.error);
        }
    }

    /// <summary>
    /// Adds the amount of Rank Points in the amount field to the user who's username is in the username field
    /// </summary>
    /// <param name="textStream"></param>
    public void AddRPFound(string textStream)
    {
        print(textStream);

        string[] entryInfo = textStream.Split(new char[] { '|' });

        string usernameInfo = entryInfo[0];
        int rpInfo = int.Parse(entryInfo[1]);
        int extraInfo = int.Parse(entryInfo[2]);
        string IDInfo = entryInfo[3];

        Row row = new Row(usernameInfo, rpInfo, extraInfo, IDInfo);

        string usernameForDB = FormatForDatabase(username.text);
        int newAmount;

        newAmount = int.Parse(amount.text) + rpInfo;
        Database.AddNewRow(usernameForDB, newAmount, extraInfo, IDInfo);
    }

    /// <summary>
    /// If the username in the username field is not found in the database this function adds them as a new user
    /// </summary>
    public void AddRPNotFound()
    {
        string usernameForDB = FormatForDatabase(username.text);

        Database.AddNewRow(usernameForDB, int.Parse(amount.text), 0, "Fresh");
    }

    /// <summary>
    /// Starts a routine to add exp to the alchamancer row
    /// </summary>
    public void AddAlchaExpRP()
    {
        StartCoroutine(AddAlchaExpRoutine());
    }

    /// <summary>
    /// Adds exp to the alchamancer row part 1
    /// </summary>
    /// <returns></returns>
    public IEnumerator AddAlchaExpRoutine()
    {
        WWW www = new WWW(Database.instance.webURL + Database.instance.privateCode + "/pipe-get/" + WWW.EscapeURL("Alchamancer"));
        yield return www;

        if (string.IsNullOrEmpty(www.error))
        {
            AddAlchaExpFinal(www.text);
        }
        else
            print("Error pulling Alchamancer info " + www.error);
    }

    /// <summary>
    /// Adds exp to the alchamancer row part 2
    /// </summary>
    /// <param name="textStream"></param>
    public void AddAlchaExpFinal(string textStream)
    {
        string[] entryInfo = textStream.Split(new char[] { '|' });

        string usernameInfo = entryInfo[0];
        int rpInfo = int.Parse(entryInfo[1]);
        int extraInfo = int.Parse(entryInfo[2]);
        string IDInfo = entryInfo[3];

        Row row = new Row(usernameInfo, rpInfo, extraInfo, IDInfo);

        int newAmount = int.Parse(amountOfExp.text) + extraInfo;

        Database.RemoveRow("Alchamancer");
        Database.AddNewRow("Alchamancer", rpInfo, newAmount, IDInfo);
    }

    /// <summary>
    /// Subs out spaces for + before removing row from database
    /// </summary>
    /// <param name="_username"></param>
    /// <returns></returns>
    private string FormatForDatabase(string _username)
    {
        string dataFormat = "";

        for (int i = 0; i < _username.Length; i++)
        {
            if (_username[i] == ' ')
                dataFormat += "+";
            else
                dataFormat += _username[i];
        }
        return dataFormat;
    }

    /// <summary>
    /// Starts the process to back up the databse
    /// </summary>
    public void DownloadForBackUp()
    {
        allUsers = new List<string>();

        string[] lines = allUsersFile.text.Split('\n');

        foreach (string line in lines)
        {
            StartCoroutine(DownloadForBackupRoutine(line));
        }
    }

    /// <summary>
    /// starts pulling info from database for backup
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    IEnumerator DownloadForBackupRoutine(string username)
    {
        int startInt = 49;

        WWW www = new WWW(Database.instance.webURL + Database.instance.publicCode + "/pipe-get/" + username);
        yield return www;

        if (string.IsNullOrEmpty(www.error))
        {
            FormatScores(www.text);
        }
        else
        {
            print("Error downloading " + www.error);
        }
    }

    /// <summary>
    /// Formats databse for backup
    /// </summary>
    /// <param name="textStream"></param>
    public void FormatScores(string textStream)
    {
        string[] entries = textStream.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < entries.Length; i++)
        {
            string[] entryInfo = entries[i].Split(new char[] { '|' });
            string username = entryInfo[0];
            int rp = int.Parse(entryInfo[1]);
            int extra = int.Parse(entryInfo[2]);
            string ID = entryInfo[3];

            Row temp = new Row(username, rp, extra, ID);

            allRows.Add(temp);

            print(username + " | " + rp + " | " + extra + " | " + ID);
        }
    }

    /// <summary>
    /// Sends database to my email as a back up
    /// </summary>
    public void Backup()
    {
        string allRowsString = "";

        for (int i = 0; i < allRows.Count; i++)
        {
            string toAdd = allRows[i].username + ":" + allRows[i].rp.ToString() + ":" + allRows[i].extra.ToString() + ":" + allRows[i].ID + "\n";
            allRowsString += toAdd;
        }

        MailMessage mail = new MailMessage();

        mail.From = new MailAddress("nickthorpe71@gmail.com");
        mail.To.Add("nickthorpe71@gmail.com");
        mail.Subject = "Alchamancer Database Backup " + DateTime.Now.ToString();
        mail.Body = "Here is the databse as of " + DateTime.Now.ToString() + ". \n\n\n" + allRowsString;

        SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");
        smtpServer.Port = 587;
        smtpServer.Credentials = new NetworkCredential("nickthorpe71@gmail.com", "P880-331w") as ICredentialsByHost;
        smtpServer.EnableSsl = true;
        ServicePointManager.ServerCertificateValidationCallback =
            delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            { return true; };
        smtpServer.Send(mail);
        Debug.Log("success");
    }
}
