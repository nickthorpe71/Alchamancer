using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates name, stats and skin for AI player
/// </summary>
public class OpponentCreator : MonoBehaviour
{
    public string name;
    public int rp; //Rank Points
    public int skin = 0; 

    private List<char> nextLetters = new List<char>(); //List of letters that can be used after current letter
    private Dictionary<char, List<char>> nextLetterLists = new Dictionary<char, List<char>>(); //Dictionary ehere the key is a letter and the value is a list of all possible letters that could follow

    private List<string> titles = new List<string>() {"the Hammer", "the Axe", "the Sword", "the Blade", "the Sharp", "the Jagged", "the Flayer", "the Slasher", "the Impaler", "the Hunter", "the Slayer", "the Mauler", "the Quick", "the Witch", "the Mad", "the Wraith", "the Shade", "the Dead", "the Unholy", "the Howler", "the Grim", "the Dark", "the Tainted", "the Unclean", "the Hungry", "the Cold", "the Wise", "the Just", "the Agile", "the Angelic", "the Blunt", "the Cowardly", "the Clever", "the Demonic", "the Drunk", "the King"};
    private List<string> prefixes = new List<string>() {"Gloom", "Gray", "Dire", "Black", "Shadow", "Haze", "Wind", "Storm", "Warp", "Night", "Moon", "Star", "Pit", "Fire", "Cold", "Sharp", "Ash", "Blade", "Steel", "Stone", "Rust", "Arcane", "Mold", "Arcane", "Blight", "Plague", "Rot", "Ooze", "Puke", "Snot", "Bile", "Blood", "Pulse", "Gut", "Gore", "Flesh", "Bone", "Spine", "Mind", "Spirit", "Soul", "Wrath", "Grief", "Foul", "Vile", "Sin", "Chaos", "Dread", "Doom", "Bane", "Death", "Viper", "Dragon", "Devil", "Iron", "Angel", "Demon", "Flame", "Rose", "Ice", "Light", "Dark", "Slate", "Shade", "Witch", "Secret", "Blue", "Red", "Green", "Yellow", "White", "Alex", "Vive", "Lylo", "Edward", "Grom", "Sin", "Jana"};
    private List<string> suffixes = new List<string>() { "Touch", "Spell", "Feast", "Wound", "Grin", "Maim", "Hack", "Bite", "Rend", "Burn", "Rip", "Kill", "Call", "Vex", "Jade", "Web", "Beard", "Shield", "Killer", "Razor", "Drinker", "Shifter", "Crawler", "Dancer", "Bender", "Weaver", "Eater", "Widow", "Maggot", "Worm", "Spawn", "Wight", "Grumble", "Growler", "Snarl", "Wolf", "Crow", "Hawk", "Cloud", "Bang", "Head", "Skull", "Brow", "Eye", "Maw", "Tongue", "Fang", "Horn", "Thorn", "Claw", "Fist", "Heart", "Shank", "Skin", "Wing", "Pox", "Fester", "Blister", "Pus", "Slime", "Drool", "Froth", "Sludge", "Venom", "Poison", "Break", "Shard", "Flame", "Maul", "Thirst", "Lust", "Gobbler", "Eagle", "Boar", "Wolf", "Viper", "Wing", "Song", "Hammer", "Axe", "Sword", "Spear", "Shield", "Cow", "Thought", "Dream", "Lust", "Envy", "Sloth", "Pride", "Greed", "Wrath"};

    private void Awake()
    {
        skin = Random.Range(0, SaveLoad.instance.numOfAvailableSkins + 1); //Randomly select a skin for AI

        rp = Random.Range(950, 1010); //Select a random amount fo Rank Points for AI between selected range

        if (skin == 13) //Skin 13 is the Gheist skin 
        { //If selected skin is a Gheist skin
            rp = Random.Range(1500,2300); //Select a random amount fo Rank Points for AI between a higher selected range

            int increaseMod = rp / 50; //Increase mod is used to buff stats of Gheist

            //Raise HP, Attack and Defense by increase mod
            StatsManagerOffline.instance.theirHPMax += increaseMod; 
            StatsManagerOffline.instance.theirHP += increaseMod;
            StatsManagerOffline.instance.theirAttack += increaseMod + 5;
            StatsManagerOffline.instance.theirDefense += increaseMod + 5;
        }
    }

    void Start()
    {
        InitializeLetterLists();

        StructureName();

        //Set name and RP for AI
        StatsManagerOffline.instance.theirName.text = name;
        StatsManagerOffline.instance.theirRP.text = "RP " + rp.ToString();
        GetComponent<PlayerControlOffline>().screenName = name;
    }

    /// <summary>
    /// Randomly selects a name structure for the AI
    /// </summary>
    void StructureName()
    {
        int structInt = Random.Range(0, 5);

        int titleInt = Random.Range(0, titles.Count);
        int prefixInt = Random.Range(0, prefixes.Count);
        int suffixInt = Random.Range(0, suffixes.Count);

        if(structInt == 0)
            name = prefixes[prefixInt] + " " + titles[titleInt];
        else if (structInt == 1)
            name = BuildRandomName() + " " + prefixes[prefixInt] + " " + suffixes[suffixInt];
        else if (structInt == 2)
            name = BuildRandomName() + " " + titles[titleInt];
		else if (structInt == 3)
			name = prefixes[prefixInt] + " " + suffixes[suffixInt];
		else if (structInt == 4 || structInt == 5)
            name = BuildRandomName();
        

        if (skin == 13)
            name = "Gheist " + BuildRandomName();
    }

    /// <summary>
    /// Builds a name with no preexisting words - built letter by letter with specified randomness so the name is readable but completely made up
    /// </summary>
    string BuildRandomName()
    {
        char lastLetter = '1';
        char nextLetter;

        string _name = "";

        int length = Random.Range(3, 8);

        for (int i = 0; i < length; i++)
        {
            nextLetters = nextLetterLists[lastLetter];

            nextLetter = nextLetters[Random.Range(0, nextLetters.Count)];

            if (i == 0)
                nextLetter = char.ToUpper(nextLetter);

            _name += nextLetter;

            lastLetter = char.ToLower(nextLetter);
        }

        return _name;
    }

    /// <summary>
    /// List of all letters that could follow each letter of the alphabet
    /// </summary>
    void InitializeLetterLists()
    {
        //Some letters occur multiple times in a single list to increase their chance of being placed

        //1 is used as the first letter in a name
        List<char> allLetters = new List<char>() {'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'y', 'z', 'a', 'e', 'i', 'o', 'u', 'b', 'c', 'd', 'f', 'g', 'h', 'l', 'm', 'n', 'p', 'r', 's', 't', 'a', 'e', 'o', 'u', 'b', 'c', 'd', 'f', 'g', 'h', 'l', 'm', 'n', 'p','r', 's', 't', 'a', 'e', 'o', 'u' };
        nextLetterLists.Add('1', allLetters);

        List<char> aLetters = new List<char>() { 'b', 'c', 'd', 'f', 'g', 'h', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'x', 'y', 'z', 'e', 'b', 'c', 'd', 'f', 'g', 'h', 'k', 'l', 'm', 'n', 'p', 'r', 's', 't', 'e', 'b', 'c', 'd', 'f', 'g', 'h', 'l', 'm', 'n', 'p', 'r', 's', 't', 'e' };
        nextLetterLists.Add('a', aLetters);

        List<char> bLetters = new List<char>() { 'l', 'r', 'a', 'e', 'i', 'o', 'u' };
        nextLetterLists.Add('b', bLetters);

        List<char> cLetters = new List<char>() { 'h', 'k', 'l', 'r', 'a', 'e', 'i', 'o', 'u', 'h', 'k', 'r', 'a', 'e','o',};
        nextLetterLists.Add('c', cLetters);

        List<char> dLetters = new List<char>() { 'r', 'y', 'a', 'e', 'i', 'o', 'u', 'r', 'a', 'e', 'i', 'o', 'u' };
        nextLetterLists.Add('d', dLetters);

        List<char> eLetters = new List<char>() { 'b', 'c', 'd', 'f', 'g', 'h', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'x', 'y', 'z', 'a', 'b', 'c', 'd', 'f', 'g', 'l', 'm', 'n', 'p', 'r', 's', 't' };
        nextLetterLists.Add('e', eLetters);

        List<char> fLetters = new List<char>() { 'l', 'r', 'a', 'e', 'i', 'o', 'u' };
        nextLetterLists.Add('f', fLetters);

        List<char> gLetters = new List<char>() { 'l', 'r', 'a', 'e', 'i', 'o', 'u', 'a', 'e', 'i', 'o', 'u' };
        nextLetterLists.Add('g', gLetters);

        List<char> hLetters = new List<char>() { 'y', 'a', 'e', 'i', 'o', 'u', 'a', 'e', 'i', 'o', 'u' };
        nextLetterLists.Add('h', hLetters);

        List<char> iLetters = new List<char>() { 'b', 'c', 'd', 'f', 'g', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'x', 'z', 'e', 'o', 'b', 'c', 'd', 'f', 'g', 'l', 'm', 'n', 'p', 'r', 's', 't', 'e', 'b', 'c', 'd', 'f', 'g', 'l', 'm', 'n', 'p', 'r', 's', 't' };
        nextLetterLists.Add('i', iLetters);

        List<char> jLetters = new List<char>() { 'a', 'e', 'i', 'o', 'u', 'a', 'e', 'o',};
        nextLetterLists.Add('j', jLetters);

        List<char> kLetters = new List<char>() { 'l', 'r', 'a', 'e', 'i', 'o', 'u', 'a', 'e', 'i', 'o', 'u' };
        nextLetterLists.Add('k', kLetters);

        List<char> lLetters = new List<char>() { 'y', 'a', 'e', 'i', 'o', 'u' };
        nextLetterLists.Add('l', lLetters);

        List<char> mLetters = new List<char>() { 'y', 'a', 'e', 'i', 'o', 'u', 'a', 'e', 'i', 'o', 'u' };
        nextLetterLists.Add('m', mLetters);

        List<char> nLetters = new List<char>() { 'a', 'e', 'i', 'o', 'u', 'a', 'e', 'o', 'u' };
        nextLetterLists.Add('n', nLetters);

        List<char> oLetters = new List<char>() { 'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'r', 's', 't', 'v', 'x', 'z', 'o', 'u', 'b', 'c', 'd', 'f', 'g', 'h', 'k', 'l', 'm', 'n', 'p', 'r', 's', 't', 'o', 'b', 'c', 'd', 'f', 'g', 'h', 'k', 'l', 'm', 'n', 'p', 'r', 's', 't' };
        nextLetterLists.Add('o', oLetters);

        List<char> pLetters = new List<char>() { 'h', 'l', 'r', 'y', 'a', 'e', 'i', 'o', 'u', 'a', 'e', 'o', 'u' };
        nextLetterLists.Add('p', pLetters);

        List<char> qLetters = new List<char>() { 'u' };
        nextLetterLists.Add('q', qLetters);

        List<char> rLetters = new List<char>() { 'y', 'a', 'e', 'i', 'o', 'u', 'a', 'e', 'i', 'o', 'u' };
        nextLetterLists.Add('r', rLetters);

        List<char> sLetters = new List<char>() { 'c', 'h', 'l', 'm', 'n', 'r', 't', 'y', 'a', 'e', 'i', 'o', 'u', 'h', 'l', 'm', 'n', 'r', 't', 'y', 'a', 'e', 'i', 'o', 'u', 'h', 'l', 'm', 'n', 'r', 't', 'y', 'a', 'e', 'i', 'o', 'u' };
        nextLetterLists.Add('s', sLetters);

        List<char> tLetters = new List<char>() { 'h', 'r', 'a', 'e', 'i', 'o', 'u', 'h', 'a', 'e', 'i', 'o', 'u' };
        nextLetterLists.Add('t', tLetters);

        List<char> uLetters = new List<char>() { 'b', 'c', 'd', 'f', 'g', 'k', 'l', 'm', 'n', 'p', 'r', 's', 't', 'v', 'x', 'z', 'b', 'c', 'd', 'f', 'g', 'k', 'l', 'm', 'n', 'p', 'r', 's', 't', 'b', 'c', 'd', 'f', 'g', 'k', 'l', 'm', 'n', 'p', 'r', 's', 't' };
        nextLetterLists.Add('u', uLetters);

        List<char> vLetters = new List<char>() { 'l', 'a', 'e', 'i', 'o', 'u', 'a', 'e', 'i', 'o', 'u' };
        nextLetterLists.Add('v', vLetters);

        List<char> wLetters = new List<char>() { 'h', 'r', 'y', 'a', 'e', 'i', 'o', 'u', 'a', 'e', 'i', 'o', 'u' };
        nextLetterLists.Add('w', wLetters);

        List<char> xLetters = new List<char>() { 'a', 'i', 'o', 'u' };
        nextLetterLists.Add('x', xLetters);

        List<char> yLetters = new List<char>() { 'a', 'e', 'i', 'o', 'u', 'a', 'e', 'o', 'u' };
        nextLetterLists.Add('y', yLetters);

        List<char> zLetters = new List<char>() { 'a', 'e', 'i', 'o', 'u', 'a', 'e', 'o', 'u' };
        nextLetterLists.Add('z', zLetters);
    }
}
