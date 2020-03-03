using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Main script for Code Scene which is primarily used to get to add news scene
/// </summary>
public class PasswordScreen : MonoBehaviour
{
    [SerializeField]
    private Text enteredCode;

    private SaveLoad saveLoad;
    private Carry carry;

    public GameObject message;

    public AudioClip menuMusic;

    void Start()
    {
        saveLoad = SaveLoad.instance;
        carry = Carry.instance;

        SoundManager.instance.musicSource.clip = menuMusic;
        SoundManager.instance.musicSource.Play();
    }

    /// <summary>
    /// Function that is run by clicking the } button
    /// </summary>
    public void OnClickEnterCode()
    {
        if(enteredCode.text == "aIAlive = true;")
        {
            carry.aIAcvive = true;
            StartCoroutine(DisplayMessage("You've done it!"));
        }

        else if (enteredCode.text == "aIAlive = false;")
        {
            carry.aIAcvive = false;
            StartCoroutine(DisplayMessage("You've done it!"));
        }

        else if (enteredCode.text == "dwarfInTheFlask")
        {

            StartCoroutine(DisplayMessage("Hey Andrew! ;)"));
        }

        else if (enteredCode.text == "AddNews()")
        {

            StartCoroutine(DisplayMessage("Entering News"));
            GetComponent<SceneSelect>().AddNews();
        }

        else if (enteredCode.text == "Donate()")
        {
            StartCoroutine(DisplayMessage("Entering Donations"));
            GetComponent<SceneSelect>().Donate();
        }

        else
        {
            StartCoroutine(DisplayMessage("NullReferenceException"));
        }


    }

    /// <summary>
    /// Displays the input string on screen
    /// </summary>
    /// <param name="messageText"></param>
    /// <returns></returns>
    public IEnumerator DisplayMessage(string messageText)
    {
        message.GetComponent<Text>().text = messageText;
        message.SetActive(true);
        yield return new WaitForSeconds(4);
        message.SetActive(false);
    }
    
}
