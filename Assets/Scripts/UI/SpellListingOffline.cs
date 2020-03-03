﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;

/// <summary>
/// Multiplayer Version - Sets up UI display or each spell listing
/// </summary>
public class SpellListingOffline : MonoBehaviour
{
    //All possible background colors
    public Color neutral;
    public Color blue;
    public Color green;
    public Color red;
    public Color yellow;
    public Color white;
    public Color black;

    private Dictionary<string, Color> allColors = new Dictionary<string, Color>();

    public Text spellName;
    public RawImage BG;
    public GameObject[] manaIcons = new GameObject[6];
    public Vector2[] slotLocations = new Vector2[6];
    public SpellCasterOffline spellCaster;
    public SpellSO spell;
    public Button infoYesButton;

    public bool active;

    public GameManagerOffline gameManager;

    private void Start()
    {
        gameManager = GameManagerOffline.instance;

        //initialize and populate allColors
        allColors.Add("Neutral", neutral);
        allColors.Add("Blue", blue);
        allColors.Add("Green", green);
        allColors.Add("Red", red);
        allColors.Add("Yellow", yellow);
        allColors.Add("White", white);
        allColors.Add("Black", black);

        //Set positions of mana cost display slots
        if (active)
        {
            slotLocations[0] = new Vector2(-200, 27);
            slotLocations[1] = new Vector2(-160, 27);
            slotLocations[2] = new Vector2(-120, 27);
            slotLocations[3] = new Vector2(-80, 27);
            slotLocations[4] = new Vector2(-40, 27);
            slotLocations[5] = new Vector2(-0, 27);
        }
        else
        {
            slotLocations[0] = new Vector2(-218, 37);
            slotLocations[1] = new Vector2(-166, 37);
            slotLocations[2] = new Vector2(-113, 37);
            slotLocations[3] = new Vector2(-54, 37);
            slotLocations[4] = new Vector2(-1, 37);
            slotLocations[5] = new Vector2(51, 37);
        }
        SetCost();
        SetColor();

    }

    /// <summary>
    /// Sends info fo spell caster when the player clicks a spell listing
    /// </summary>
    public void Click()
    {
        if (gameManager.myTurn)
            spellCaster.SubmitAttack(spell.power, spell, false);
        else
        {
            gameManager.DisplayMessage("It is not your turn.");
        }
    }

    /// <summary>
    /// Sets tooltip to be active or inactive depending on current state
    /// </summary>
    public void Info()
    {
        if (active)
        {
            GameManagerOffline.instance.currentSpell = spell;
            GameManagerOffline.instance.DisplayToolTip(spell.spellName, spell.toolTip, spell.power.ToString());
            CheckCost();
        }
        else
        {
            SpellScreen.instance.DisplayToolTip(spell.spellName, spell.toolTip, spell.power.ToString());
        }
    }

    /// <summary>
    /// Checks the cost of the spell for this listing and determines whether the player has enough mana to cast it
    /// </summary>
    public void CheckCost()
    {
        //this is acts as start becuase it is called before the object becomes active
        infoYesButton = GameManagerOffline.instance.infoYesButton;
        TerraformOffline terraScript = GameManagerOffline.instance.myPlayer.GetComponentInChildren<TerraformOffline>();

        if (terraScript.waterCount >= spell.blueCost && terraScript.plantCount >= spell.greenCost &&
           terraScript.fireCount >= spell.redCost && terraScript.rockCount >= spell.yellowCost &&
           terraScript.lifeCount >= spell.whiteCost && terraScript.deathCount >= spell.blackCost)
        {
            GetComponent<Button>().interactable = true;
            infoYesButton.GetComponent<Button>().interactable = true;
        }
        else
        {
            GetComponent<Button>().interactable = false;
            infoYesButton.GetComponent<Button>().interactable = false;
        }

    }

    /// <summary>
    /// Set background UI color
    /// </summary>
    private void SetColor()
    {
        BG.color = allColors[spell.color];
    }

    /// <summary>
    /// Use cost of spell to set mana cost images
    /// </summary>
    private void SetCost()
    {
        int blueTemp = spell.blueCost;
        int greenTemp = spell.greenCost;
        int redTemp = spell.redCost;
        int yellowTemp = spell.yellowCost;
        int whiteTemp = spell.whiteCost;
        int blackTemp = spell.blackCost;

        for (int i = 0; i < spell.totalcost; i++)
        {
            if (blueTemp > 0)
            {
                InstantiateMana(manaIcons[0], slotLocations[i]);
                blueTemp--;
            }
            else if(greenTemp > 0)
            {
                InstantiateMana(manaIcons[1], slotLocations[i]);
                greenTemp--;
            }
            else if (redTemp > 0)
            {
                InstantiateMana(manaIcons[2], slotLocations[i]);
                redTemp--;
            }
            else if (yellowTemp > 0)
            {
                InstantiateMana(manaIcons[3], slotLocations[i]);
                yellowTemp--;
            }
            else if (whiteTemp > 0)
            {
                InstantiateMana(manaIcons[4], slotLocations[i]);
                whiteTemp--;
            }
            else if (blackTemp > 0)
            {
                InstantiateMana(manaIcons[5], slotLocations[i]);
                blackTemp--;
            }
        }
    }

    /// <summary>
    /// Populates mana cost images
    /// </summary>
    /// <param name="manaImage"></param>
    /// <param name="slotPos"></param>
    private void InstantiateMana(GameObject manaImage, Vector2 slotPos)
    {
        GameObject temp = Instantiate(manaImage, slotPos, Quaternion.identity);
        temp.transform.SetParent(this.gameObject.transform, false);
        temp.transform.localPosition = slotPos;

        if (!active)
            temp.GetComponent<RectTransform>().sizeDelta *= 1.25f;
    }
}
