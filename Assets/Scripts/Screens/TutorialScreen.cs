using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialScreen : MonoBehaviour
{
    public GameObject prevButton;
    public GameObject nextButton;
    public GameObject finishButton;

    public List<GameObject> slides = new List<GameObject>();

    private int currentSlide;

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

    public void TutorialComplete()
    {
        SaveLoad.instance.doneTutorial = true;
        SaveLoad.instance.Save();
    }
}
