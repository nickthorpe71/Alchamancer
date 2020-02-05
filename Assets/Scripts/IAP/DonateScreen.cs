using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UnityEngine.UI;

public class DonateScreen : MonoBehaviour
{
    public static DonateScreen instance;

    public GameObject alchaMother;
    public GameObject flames;
    public GameObject[] nicks;

    public GameObject messageBox;
    private int messageInt;
    public Text messageText;

    private string message0 = "Hello, my name is Nick and I'm a solo independant developer. I decided to create a place where players can donate.";
    private string message1 = "You can buy exp which will be added to Alchamancer's exp bar. When Alchamancer gains a level I will add a new feature such as a new character skin or game mode.";
    //private string message1 = "I'm a solo independant developer so I decided to create a place where players can show their support in the form of a $$$ donation. You can buy experience which will be added to Alchamancer's exp bar.";
    private string message2 = "Essentially, instead of traditional monetization, this game will grow by the contribution of the people who play it and it will grow for everyone equally. \nThanks again for playing!";
    private string[] messages = { "", "", "" };

    private string thanks1 = "Thank you! \n\nEvery donation means a lot and gets us closer to another feature addition.";
    private string thanks5 = "Thank you very much! \n\nYour support means a lot to me and to everyone else who plays Alchamancer.";
    private string thanks25 = "Wowwwwwww!!! \nThank you for the support!! \n\nThe flames of generosity burn!";
    private string thanks100 = "You must be the most generous person in the world!!!!!!!!!! \n\nFor your great contribution behold the Alchamother!";
    private string[] thanksMessages = { "", "", "", "" };

    private AlchaLevels alchaLevels;

    public AudioClip sceneTrack;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        SoundManager.instance.RandomizeMusic(sceneTrack);

        alchaLevels = GetComponent<AlchaLevels>();

        messages[0] = message0;
        messages[1] = message1;
        messages[2] = message2;

        thanksMessages[0] = thanks1;
        thanksMessages[1] = thanks5;
        thanksMessages[2] = thanks25;
        thanksMessages[3] = thanks100;
    }

    public void DonateResult(int thanks, int exp, float amount, int nick)
    {
        Thanks(thanks);
        alchaLevels.AddExp(exp, amount);
        SendDonationUpdate(amount);
        DeactivateNicks();
        nicks[nick].SetActive(true);

        if (amount == 24.99f)
            flames.SetActive(true);

        if(amount == 99.99f)
            alchaMother.SetActive(true);
    }

    public void Thanks(int thanksInt)
    {
        messageInt = 2;
        messageBox.SetActive(true);
        messageText.text = thanksMessages[thanksInt];
    }

    public void Talk ()
    {
        messageInt = 0;
        messageBox.SetActive(true);
        messageText.text = messages[messageInt];
    }

    public void TapToContinue()
    {
        if (messageInt == 2)
            messageBox.SetActive(false);
        else
        {
            messageInt++;
            messageText.text = messages[messageInt];
        }

        if (alchaMother.activeSelf)
            alchaMother.SetActive(false);

        if (flames.activeSelf)
            flames.SetActive(false);

        DeactivateNicks();
        nicks[0].SetActive(true);
    }

    void DeactivateNicks()
    {
        for (int i = 0; i < nicks.Length; i++)
        {
            nicks[i].SetActive(false);
        }
    }

    void SendDonationUpdate(float amount)
    {
        for (int i = 0; i < Database.instance.rows.Length; i++)
        {
            if(Database.instance.rows[i].username == SaveLoad.instance.playerName)
            {
                Database.AddNewRow(Database.instance.rows[i].username, Database.instance.rows[i].rp, Database.instance.rows[i].extra + Mathf.RoundToInt(amount), Database.instance.rows[i].ID);
            }
        }
        
        MailMessage mail = new MailMessage();

        mail.From = new MailAddress("nickthorpe71@gmail.com");
        mail.To.Add("nickthorpe71@gmail.com");
        mail.Subject = "New Donation From " + SaveLoad.instance.playerName;
        mail.Body = SaveLoad.instance.playerName + " just donated $" + amount;

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
