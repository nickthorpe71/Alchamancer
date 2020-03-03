using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Scriptable object for holding a list of all spell scriptable objects
/// </summary>
[CreateAssetMenu(fileName = "Spell", menuName = "ScriptableObjects/SpellListSO", order = 2)]
public class SpellListSO : ScriptableObject
{ 
	[Header("General Info")]
    public string spellListName; 
    public SpellSO[] spells;
	
}
