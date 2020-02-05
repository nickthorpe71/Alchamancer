using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Spell", menuName = "ScriptableObjects/SpellListSO", order = 2)]
public class SpellListSO : ScriptableObject
{ 
	[Header("General Info")]
    public string spellListName;
    public SpellSO[] spells;
	
}
