using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main script for 
/// </summary>
public class TutorialScreen : MonoBehaviour
{
    public GameObject prevButton;
    public GameObject nextButton;
    public GameObject finishButton;

    public List<GameObject> slides = new List<GameObject>(); //Array holding all tutorial slides

    private int currentSlide;

    /// <summary>
    /// Moves the to next slide in the slides array
    /// </summary>
    public void NextButton()
    {
        if(!prevButton.activeSelf)
            prevButton.SetActive(true);

        slides[currentSlide].SetActive(false);
        currentSlide++;
        slides[currentSlide].SetActive(true);

        if (currentSlide == slides.Count - 1)
        {
            nextButton.SetActive(false);
            finishButton.SetActive(true);
        }
        else
            finishButton.SetActive(false);

        SoundManager.instance.PlayButtonClick();
    }

    /// <summary>
    /// Moves the to previous slide in the slides array
    /// </summary>
    public void PrevButton()
    {
        finishButton.SetActive(false);

        if (!nextButton.activeSelf)
            nextButton.SetActive(true);

        slides[currentSlide].SetActive(false);
        currentSlide--;
        slides[currentSlide].SetActive(true);

        if (currentSlide == 0)
            prevButton.SetActive(false);

        SoundManager.instance.PlayButtonClick();
    }

    /// <summary>
    /// Sets the dontTutorial bool and saves to local data
    /// </summary>
    public void TutorialComplete()
    {
        SaveLoad.instance.doneTutorial = true;
        SaveLoad.instance.Save();
    }
}
