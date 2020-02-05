using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UnityEngine.UI;

public class AlchaLevels : MonoBehaviour
{
    public Text levelText;
    public BarScript expBar;

    public GameObject addButtons;
    public GameObject loading;

    public Database database;

    private int currentLevel;
    private int currentExp;
    private int startLevel;
    private int startExp;
    private int finalLevel;
    private int finalExp;
    private float expToNextLevel;

    public float currentDonations;
    public Text donationText;

    void Start()
    {
        database = Database.instance;

        SetInfo(database.rows);
        expToNextLevel = ExpMod(currentLevel);
        startLevel = currentLevel;
        startExp = currentExp;

        expBar.fill.fillAmount = expBar.Map(currentExp, expToNextLevel);
        expBar.MaxValue = expToNextLevel;
        expBar.SetValue(currentExp);
    }

    int ExpMod(int level)
    {
        return Mathf.RoundToInt(13 * level + (level * 189.92f));
    }

    public void AddExp(int addedExp, float donation)
    {
        startLevel = currentLevel;
        expToNextLevel = ExpMod(currentLevel);
        int levelsToGain = 0;
        int firstLevelSubtract = (int)(expToNextLevel - currentExp);

        if (addedExp > firstLevelSubtract)
        {
            addedExp -= firstLevelSubtract;
            levelsToGain++;
            currentLevel++;

            bool looping = true;

            while (looping)
            {
                expToNextLevel = ExpMod(currentLevel);

                if (addedExp > expToNextLevel)
                {
                    levelsToGain++;
                    currentLevel++;
                    SendLevelUpdate();
                    addedExp -= Mathf.RoundToInt(expToNextLevel);
                }
                else
                {
                    finalLevel = currentLevel;
                    finalExp = addedExp;
                    looping = false;
                }
            }
            StartCoroutine(LevelUp(levelsToGain));
        }
        else
        {
            currentExp = currentExp + addedExp;
            expBar.SetValue(currentExp);
        }

        string newDonation = (currentDonations + donation).ToString();
        StartCoroutine(SendToDatabase(newDonation));
    }

    IEnumerator SendToDatabase(string newDonations)
    {
        addButtons.SetActive(false);
        loading.SetActive(true);

        Database.AddNewRow("Alchamancer", currentLevel, currentExp, newDonations);

        yield return new WaitForSeconds(5);

        database.Download();
        database.SendAlchamancer(this);
    }

    public void SetInfo(Row[] rows)
    {
        for (int i = 0; i < rows.Length; i++)
        {
            if (rows[i].username == "Alchamancer")
            {
                currentLevel = rows[i].rp;
                currentExp = rows[i].extra;
                levelText.text = currentLevel.ToString();
                currentDonations = float.Parse(rows[i].ID);
                donationText.text = "$" + rows[i].ID;

                if (loading.activeSelf)
                {
                    addButtons.SetActive(true);
                    loading.SetActive(false);
                }
            }
        }
    }

    public IEnumerator LevelUp(int levelsToGain)
    {
        int tempLevel = startLevel;

        for (int i = 0; i < levelsToGain; i++)
        {
            yield return new WaitForSeconds(0.5f);

            expBar.MaxValue = ExpMod(startLevel);
            expBar.SetValue(ExpMod(startLevel));

            yield return new WaitForSeconds(1f);

            tempLevel++;
            levelText.text = currentLevel.ToString();

            yield return new WaitForSeconds(0.5f);

            expBar.fill.fillAmount = expBar.Map(0, ExpMod(currentLevel));
            expBar.MaxValue = ExpMod(currentLevel);
            expBar.SetValue(0);
        }

        yield return new WaitForSeconds(0.5f);

        expBar.SetValue(finalExp);
        currentExp = finalExp;
    }

    public void UpdateLeaderboards()
    {
        //change this to work for mother
        //Database.AddNewRow(saveLoad.playerName, saveLoad.playerLevel, 0, SystemInfo.deviceUniqueIdentifier);
    }

    public void LeaveEarly()
    {
        UpdateLeaderboards();
        SceneSelect.instance.MainMenuButton();
    }

    public void LeaveEarlyToLeaderboards()
    {
        UpdateLeaderboards();
        SceneSelect.instance.LeaderBoard();
    }

    void SendLevelUpdate()
    {
        MailMessage mail = new MailMessage();

        mail.From = new MailAddress("nickthorpe71@gmail.com");
        mail.To.Add("nickthorpe71@gmail.com");
        mail.Subject = "Alchamancer Level - " + currentLevel;
        mail.Body = "Alchamancer just grew to level " + currentLevel;

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
