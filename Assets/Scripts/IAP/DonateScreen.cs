using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UnityEngine.UI;

/// <summary>
/// Deals with visuals on Donation Screen and sending donation updates by email
/// </summary>
public class DonateScreen : MonoBehaviour
{
    //To allow this sctipt to be referenced by other scrips in this scene without being saved in a variable
    public static DonateScreen instance;

    //Different images taht can be displayed on the donation screen
    public GameObject alchaMother;
    public GameObject flames;
    public GameObject[] nicks;

    public GameObject messageBox; //Game object that is turned off and on with messages
    private int messageInt; //Determines which message is being displayed
    public Text messageText; //Text object that is filled with different messagees

    //All messages that can be read on donation screen
    private string message0 = "Hello, my name is Nick and I'm a solo independant developer. I decided to create a place where players can donate.";
    private string message1 = "You can buy exp which will be added to Alchamancer's exp bar. When Alchamancer gains a level I will add a new feature such as a new character skin or game mode.";
    private string message2 = "Essentially, instead of traditional monetization, this game will grow by the contribution of the people who play it and it will grow for everyone equally. \nThanks again for playing!";
    private string[] messages = { "", "", "" };

    private string thanks1 = "Thank you! \n\nEvery donation means a lot and gets us closer to another feature addition.";
    private string thanks5 = "Thank you very much! \n\nYour support means a lot to me and to everyone else who plays Alchamancer.";
    private string thanks25 = "Wowwwwwww!!! \nThank you for the support!! \n\nThe flames of generosity burn!";
    private string thanks100 = "You must be the most generous person in the world!!!!!!!!!! \n\nFor your great contribution behold the Alchamother!";
    private string[] thanksMessages = { "", "", "", "" };

    //Reference to AlchaLevels script
    private AlchaLevels alchaLevels;

    //Background music for the scene
    public AudioClip sceneTrack;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        //Plays the background music at a randomized pitch
        SoundManager.instance.RandomizeMusic(sceneTrack);

        //Initialize AlchaLevels script
        alchaLevels = GetComponent<AlchaLevels>();

        //Load all message arrays
        messages[0] = message0;
        messages[1] = message1;
        messages[2] = message2;

        thanksMessages[0] = thanks1;
        thanksMessages[1] = thanks5;
        thanksMessages[2] = thanks25;
        thanksMessages[3] = thanks100;
    }

    /// <summary>
    /// Called when the user donates - Sets appropriate messages and images to reflect their donation
    /// </summary>
    /// <param name="thanks"></param>
    /// <param name="exp"></param>
    /// <param name="amount"></param>
    /// <param name="nick"></param>
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

    /// <summary>
    /// Displays message from the thanksMessages array using thanksInt that is passed through
    /// </summary>
    /// <param name="thanksInt"></param>
    public void Thanks(int thanksInt)
    {
        messageInt = 2;
        messageBox.SetActive(true);
        messageText.text = thanksMessages[thanksInt];
    }

    /// <summary>
    /// Called when the talk button is called - Starts messages at the first message 
    /// </summary>
    public void Talk ()
    {
        messageInt = 0;
        messageBox.SetActive(true);
        messageText.text = messages[messageInt];
    }

    /// <summary>
    /// When user taps on the current message this function takes them to the next message
    /// </summary>
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

    /// <summary>
    /// Sets all images of Nick character to inactive
    /// </summary>
    void DeactivateNicks()
    {
        for (int i = 0; i < nicks.Length; i++)
        {
            nicks[i].SetActive(false);
        }
    }

    /// <summary>
    /// Sends me an email whenever someone donates with their username and the amount donated
    /// </summary>
    /// <param name="amount"></param>
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
