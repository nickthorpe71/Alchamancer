using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UnityEngine.UI;

public class AddNewsScreen : MonoBehaviour
{
    public Text news1text;
    public Text news2text;

    public Text username;
    public Text amount;

    public GameObject news1Obj;
    public GameObject news2Obj;

    public GameObject resetMenu;

    public Text amountOfExp;

    private void Start()
    {
        Database.instance.Download();
    }

    public void LoadOldNews1()
    {
        //need to replace these two with single search
        //news1Obj.gameObject.GetComponent<InputField>().text = Database.instance.newsArr[0];
    }
    public void LoadOldNews2()
    {
        //news2Obj.gameObject.GetComponent<InputField>().text = Database.instance.newsArr[1];
    }

    public void Upload1()
    {
        Database.AddNewRow("News1", 11000, 0, news1text.text);
    }

    public void Upload2()
    {
        Database.AddNewRow("News2", 11000, 0, news2text.text);
    }

    public void UpdateVersion()
    {
        Database.AddNewRow("Version", 11000, 0, Application.version);
    }

    public void ResetMenu()
    {
        if (resetMenu.activeSelf)
            resetMenu.SetActive(false);
        else if (!resetMenu.activeSelf)
            resetMenu.SetActive(true);
    }

    public void ResetLeaderboards()
    {
        StartCoroutine(ResetLeaderboardsRoutine());
    }

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

    public void AddRP()
    {
        StartCoroutine(AddRPRoutine());
    }

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

    public void AddRPNotFound()
    {
        string usernameForDB = FormatForDatabase(username.text);

        Database.AddNewRow(usernameForDB, int.Parse(amount.text), 0, "Fresh");
    }

    public void AddAlchaExpRP()
    {
        StartCoroutine(AddAlchaExpRoutine());
    }

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

    //subs out spaces for + before removing row from database
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

    public void Backup()
    {
        string allRows = "";
        Row[] rows = Database.instance.rows;

        for (int i = 0; i < rows.Length; i++)
        {
            string toAdd = rows[i].username + ":" + rows[i].rp.ToString() + ":" + rows[i].extra.ToString() + ":" + rows[i].ID + "\n";
            allRows += toAdd;
        }

        MailMessage mail = new MailMessage();

        mail.From = new MailAddress("nickthorpe71@gmail.com");
        mail.To.Add("nickthorpe71@gmail.com");
        mail.Subject = "Alchamancer Database Backup " + DateTime.Now.ToString();
        mail.Body = "Here is the databse as of " + DateTime.Now.ToString() + ". \n\n\n" + allRows;

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
