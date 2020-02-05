using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UnityEngine.UI;

public class LoggedIn : MonoBehaviour
{
    
    void Start()
    {
        Send();
    }

    public void Send()
    {
        MailMessage mail = new MailMessage();

        mail.From = new MailAddress("nickthorpe71@gmail.com");
        mail.To.Add("nickthorpe71@gmail.com");
        mail.Subject = SaveLoad.instance.playerName + " Logged In! " + DateTime.Now.ToString();
        mail.Body = SaveLoad.instance.playerName + " logged in at " + DateTime.Now.ToString();

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
