using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

/// <summary>
/// Singleplayer Version - Tracks field color and changes environments
/// </summary>
public class AreaColorManagerOffline : MonoBehaviourPunCallbacks
{
    public static AreaColorManagerOffline instance; //Allows easy access to this script from other scripts in the scene

    public GameObject[] environments; //Array to hold all environment game objects

    public Color neutral;
    public Color blue;
    public Color green;
    public Color red;
    public Color yellow;
    public Color white;
    public Color black;

    [HideInInspector] public string currentAreaColor = "Neutral";

    //Images that display field colors
    public Image MainColor;
    public Image orb1;
    public Image orb2;

    private string[] currentColors = new string[2]; //Array of which colors are active
    private Dictionary<int, string> allColorStrings = new Dictionary<int, string>(); //Dictionary of all possible color names - Keyed by int
    private Dictionary<string, Color> allColors = new Dictionary<string, Color>(); //Dictionary of all collors - Keyed by string name

    private bool firstTurn = true; //bool to stop the field from changing color on the first turn

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        //Populate allColorStrings dictionary
        allColorStrings.Add(0, "Blue");
        allColorStrings.Add(1, "Green");
        allColorStrings.Add(2, "Red");
        allColorStrings.Add(3, "Yellow");
        allColorStrings.Add(4, "White");
        allColorStrings.Add(5, "Black");

        //Populate allColors dictionary
        allColors.Add("Neutral", neutral);
        allColors.Add("Blue", blue);
        allColors.Add("Green", green);
        allColors.Add("Red", red);
        allColors.Add("Yellow", yellow);
        allColors.Add("White", white);
        allColors.Add("Black", black);

        //Randomizes the current field colors
        RandomizeColors();

        //Set the field color to neutral at start of the match
        MainColor.color = neutral;
        currentAreaColor = "Neutral";
    }

    /// <summary>
    /// Randomizes the colors on the field
    /// </summary>
    private void RandomizeColors()
    {
        for (int i = 0; i < 2; i++)
        {
            int temp = Random.Range(0, 6);
            currentColors[i] = allColorStrings[temp];
        }

        UpdateColorImages();
    }

    /// <summary>
    /// Moves the colors right allowing a new color to be added
    /// </summary>
    /// <param name="newColor"></param>
    public void Cycle(string newColor)
    {
        currentColors[1] = currentColors[0];
        currentColors[0] = newColor;

        UpdateColorImages();
    }

    /// <summary>
    /// Updates the display images with the currenyColor array
    /// </summary>
    private void UpdateColorImages()
    {
        orb1.color = allColors[currentColors[0]];
        orb2.color = allColors[currentColors[1]];

        CheckMainColor();
    }

    /// <summary>
    /// Checks to see if the two sub colors match and if so changes the field color to that color
    /// </summary>
    private void CheckMainColor()
    {
        string tempColor = currentColors[0];

        if(currentColors[1] == tempColor && tempColor != currentAreaColor && !firstTurn)
        {
            MainColor.color = allColors[tempColor];
            currentAreaColor = tempColor;
            SoundManager.instance.PlaySinglePublic("preCastSound", 1);

            EnvironmentsOff();

            switch (tempColor)
            {
                case "Blue":
                    GameManagerOffline.instance.DisplayMessage("Water and ice enclose the battlefield while rain pours down. Blue spells will now be twice as effective and Red spells half as effective.");
                    environments[1].SetActive(true);
                    break;

                case "Green":
                    GameManagerOffline.instance.DisplayMessage("All manner of flora have beset the field. Green spells will now be twice as effective and Yellow spells half as effective.");
                    environments[2].SetActive(true);
                    break;

                case "Red":
                    GameManagerOffline.instance.DisplayMessage("The temperature has piqued surrounding the field in magma. Red spells will now be twice as effective and Blue spells half as effective.");
                    environments[3].SetActive(true);
                    break;

                case "Yellow":
                    GameManagerOffline.instance.DisplayMessage("The battlefield has become arid and bouldered. Yellow spells will now be twice as effective and Green spells half as effective.");
                    environments[4].SetActive(true);
                    break;

                case "White":
                    GameManagerOffline.instance.DisplayMessage("Heavenly light envelopes the field. White spells will now be twice as effective and Black spells half as effective.");
                    environments[5].SetActive(true);
                    break;

                case "Black":
                    GameManagerOffline.instance.DisplayMessage("The field has fallen into an eerie darkness. Black spells will now be twice as effective and White spells half as effective.");
                    environments[6].SetActive(true);
                    break;
            }

            GameManagerOffline.instance.CameraShake();
        }

        firstTurn = false;
    }

    /// <summary>
    /// Deactivates all environment Game Objects
    /// </summary>
    void EnvironmentsOff()
    {
        for (int i = 0; i < environments.Length; i++)
        {
            environments[i].SetActive(false);
        }
    }

}
