using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Main script for the Spel Library Scene 
/// </summary>
public class SpellScreen : MonoBehaviour
{
    public static SpellScreen instance;

    [Header("Spells")]
    public SpellListSO spellLibrary;
    public SpellSO[] spells;
    public SpellListing spellListing;
    public Transform content;
    public List<GameObject> spellButtons = new List<GameObject>();

    public Text toolText;
    public Text toolName;
    public Text toolPow;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        spells = spellLibrary.spells;
        PopulateSpellList();
        DisplayToolTip("", "Tap the i icon for information about a spell" , "-");
    }

    /// <summary>
    /// Populates the UI list of spells
    /// </summary>
    private void PopulateSpellList()
    {
        for (int i = 0; i < spells.Length; i++)
        {
            SpellListing listing = Instantiate(spellListing, content);
            if (listing != null)
            {
                spellButtons.Add(listing.gameObject);

                listing.spellName.text = spells[i].spellName;
                listing.spell = spells[i];
            }
        }
    }

    /// <summary>
    /// Function that displays the tooltip for each spell
    /// </summary>
    /// <param name="name"></param>
    /// <param name="message"></param>
    /// <param name="pow"></param>
    public void DisplayToolTip(string name, string message, string pow)
    {
        toolName.text = name;
        toolText.text = message;
        toolPow.text = "Pow: " + pow;
    }

}
