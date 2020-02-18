using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class ChangeName : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private Text newName;
    [SerializeField]
    private Text currentName;

    public bool isNewGame;
    public NewProfileScreen newScreen;

    List<string> badWords;
    [SerializeField] TextAsset badWordsFile;

    public Database database;

    public GameObject savedIndicator;
    public GameObject notSavedIndicator;

    private void Awake()
    {
        badWords = new List<string>();

        string[] lines = badWordsFile.text.Split(',');

        foreach (string line in lines)
        {
            string newLine = line.Substring(1, line.Length - 1);

            badWords.Add(newLine);
        }

        database = Database.instance;
    }

    //subs out spaces for + before removing row from database
    private string FormatForDatabase(string username)
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

    public void OnClickChangeName()
    {
        StartCoroutine(ChangeNameRoutine());
    }

    public IEnumerator ChangeNameRoutine()
    {
        string dataFormatNameOld = FormatForDatabase(SaveLoad.instance.playerName);

        WWW www = new WWW(Database.instance.webURL + Database.instance.privateCode + "/pipe-get/" + WWW.EscapeURL(newName.text));
        yield return www;

        if (string.IsNullOrEmpty(www.error))
        {
            print(www.text);

            if (www.text == "")
            {
                WWW www2 = new WWW(Database.instance.webURL + Database.instance.privateCode + "/pipe-get/" + WWW.EscapeURL(dataFormatNameOld));
                yield return www2;
                ChangeNameFinal(www2.text, true);
            }
            else
                ChangeNameFinal(www.text, false);
        }
        else
        {
            print("Error pulling Alchamancer info " + www.error);
        }
    }

    public void ChangeNameFinal(string textStream, bool isAvailable)
    {
        bool isBadWord = badWords.Any(newName.text.ToLower().Contains);

        string dataFormatNameOld = FormatForDatabase(SaveLoad.instance.playerName);
        string dataFormatNameNew = FormatForDatabase(newName.text);

        if (isAvailable)
        {
            string[] entryInfo = textStream.Split(new char[] { '|' });

            string usernameInfo = entryInfo[0];
            int rpInfo = int.Parse(entryInfo[1]);
            int extraInfo = int.Parse(entryInfo[2]);
            string IDInfo = entryInfo[3];

            Row row = new Row(usernameInfo, rpInfo, extraInfo, IDInfo);

            if (newName.text.Length >= 2)
            {
                if (newName.text.Length <= 15)
                {
                    if (isBadWord)
                    {
                        currentName.text = "That name may offend some users.";

                        if (notSavedIndicator != null)
                            StartCoroutine(NotSavedNameRoutine());
                    }
                    else
                    {
                        if (savedIndicator != null)
                        {
                            StartCoroutine(SavedNameRoutine());
                        }

                        int rp = 1000;

                        if (dataFormatNameOld == row.username)
                        {
                            rp = row.rp;
                            if (rp == 0)
                                rp = 1000;
                        }

                        if (!isNewGame)
                        {
                            Database.RemoveRow(dataFormatNameOld);
                        }

                        Database.AddNewRow(newName.text, rp, row.extra, SystemInfo.deviceUniqueIdentifier);
                        currentName.text = newName.text;
                        SaveLoad.instance.playerName = newName.text;
                        SaveLoad.instance.Save();

                        if (isNewGame)
                        {
                            newScreen.SwitchToAppearance();
                        }
                    }
                }
                else
                {
                    currentName.text = "That name is too long.";

                    if (notSavedIndicator != null)
                        StartCoroutine(NotSavedNameRoutine());
                }
            }
            else
            {
                currentName.text = "That name is too short.";

                if (notSavedIndicator != null)
                    StartCoroutine(NotSavedNameRoutine());
            }
        }
        else
        {
            currentName.text = "That name is already in use.";

            if (notSavedIndicator != null)
                StartCoroutine(NotSavedNameRoutine());
        }
    }

    public IEnumerator SavedNameRoutine()
    {
        savedIndicator.SetActive(true);
        yield return new WaitForSeconds(2);
        savedIndicator.SetActive(false);
    }

    public IEnumerator NotSavedNameRoutine()
    {
        notSavedIndicator.SetActive(true);
        yield return new WaitForSeconds(2);
        notSavedIndicator.SetActive(false);
    }
}
