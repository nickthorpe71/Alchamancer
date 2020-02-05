using UnityEngine;

[CreateAssetMenu(fileName = "Spell", menuName = "ScriptableObjects/SpellSO", order = 1)]
public class SpellSO : ScriptableObject
{ 
	[Header("General Info")]
    public string spellName;
	public string color;
	public string category;
	public int power;
	public string techType;
    public AudioClip castSound;

    [Header("Techs")]
    public int poisionChance;
    public int burnChance;
    public bool removeDot;
    public string changeLandscape;
    public int healAmount;
    public int myAttkInt;
    public int theirAttkInt;
    public int myDefInt;
    public int theirDefInt;
    public bool stealHealth;
    public bool counter;
    public bool spiritWalk;
    public bool eatMana;

    [Header("Mana Cost")]
	public int blueCost;
	public int greenCost;
	public int redCost;
	public int yellowCost;
	public int whiteCost;
	public int blackCost;
    public int totalcost;

	[Header("Text")]
	public string toolTip;

    [Header("Animations")]
    public bool hasProjectile;
    public bool hasTheirLocationEffect;
    public bool hasMyLocationEffect;
    public string projectile;
	public string theirLocationEffect;
	public string myLocationEffect;
    public bool screenShake;

}
