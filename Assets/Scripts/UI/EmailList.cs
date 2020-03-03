using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UnityEngine.UI;

/// <summary>
/// Allows players to send their email to me and therefore join the mailing list
/// </summary>
public class EmailList : MonoBehaviour
{
    public Text email;
    public GameObject saveIndicator;

    public void AddEmail()
    {
        MailMessage mail = new MailMessage();

        mail.From = new MailAddress("nickthorpe71@gmail.com");
        mail.To.Add("nickthorpe71@gmail.com");
        mail.Subject = "New Mailing List Sub " + DateTime.Now.ToString();
        mail.Body = SaveLoad.instance.playerName + " subscribed to the mailing list: " + email.text;

        SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");
        smtpServer.Port = 587;
        smtpServer.Credentials = new NetworkCredential("nickthorpe71@gmail.com", "P880-331w") as ICredentialsByHost;
        smtpServer.EnableSsl = true;
        ServicePointManager.ServerCertificateValidationCallback =
            delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            { return true; };
        smtpServer.Send(mail);

        StartCoroutine(Saved());
    }

    IEnumerator Saved()
    {
        yield return new WaitForSeconds(4);
        saveIndicator.SetActive(true);
        yield return new WaitForSeconds(8);
        saveIndicator.SetActive(false);
    }
}
