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

    public void OnDownloadNames (Row[] rows)
    {
        bool isBadWord = badWords.Any(newName.text.ToLower().Contains);

        bool nameIsAvailable = true;

        string dataFormatNameOld = FormatForDatabase(SaveLoad.instance.playerName);
        string dataFormatNameNew = FormatForDatabase(newName.text);

        for (int i = 0; i < rows.Length; i++)
        {
            if (dataFormatNameNew == rows[i].username)
                nameIsAvailable = false;
        }

        if (nameIsAvailable)
        {
            if (newName.text.Length >= 2)
            {
                if (newName.text.Length <= 15)
                {
                    if (isBadWord || newName.text == "P1" || newName.text == "P2")
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
                        int extra = 0;

                        for (int i = 0; i < rows.Length; i++)
                        {
                            if (dataFormatNameOld == rows[i].username)
                            {
                                rp = rows[i].rp;
                                if (rp == 0)
                                    rp = 1000;
                                extra = rows[i].extra;
                            }
                        }

                        if (!isNewGame)
                        {
                            Database.RemoveRow(dataFormatNameOld);
                        }

                        Database.AddNewRow(newName.text, rp, extra, SystemInfo.deviceUniqueIdentifier);
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

    public void OnClickChangeName()
    {
        database.SendNames(this);
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
