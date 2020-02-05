using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellManagerOffline : MonoBehaviour
{
    public static CellManagerOffline instance;

    [Header("Elements")]
    public Dictionary<Vector3, GameObject> masterGrid = new Dictionary<Vector3, GameObject>();
    public string startString;
    private string realString = "4,3,6,2,6,3,4,5,2,7,3,7,2,5,7,6,5,4,5,6,7,1,1,1,8,1,1,1,2,3,4,5,4,3,2,4,7,2,6,2,7,4,5,6,3,7,3,6,5";
    public GameObject[] startMaterials;
    private int startMatInt = 0;
    private List<GameObject> elements = new List<GameObject>();
    public GameObject dirt;
    public GameObject water;
    public GameObject plant;
    public GameObject fire;
    public GameObject rock;
    public GameObject life;
    public GameObject death;
    public GameObject dragonVein;

    [Header("Positioning")]
    private Vector3 spawnVec;
    private int xInt;
    private int yInt;

    [Header("Management")]
    public GameObject player;
    public bool canWin = true;
    public GameObject spawned;
    public int currentInt;
    public SpellCasterOffline spellCaster;
    private GameManagerOffline gameManager;
    private Carry carryScript;


    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        gameManager = GameManagerOffline.instance;
        carryScript = Carry.instance;

        currentInt = 0;

        elements.Add(water);
        elements.Add(plant);
        elements.Add(fire);
        elements.Add(rock);
        elements.Add(life);
        elements.Add(death);

        //startString = carryScript.levelString;
        startString = realString;
        FillByCode();
    }

    public void SpawnRandom()
    {
        for (int i = 0; i < 100; i++)
        {
            Vector3 randomSpawnPoint = new Vector3(Random.Range(1, 8), Random.Range(1, 8), 0);

            if (masterGrid[randomSpawnPoint].tag == "Dirt" && masterGrid[randomSpawnPoint].tag != "DragonVein"
                && randomSpawnPoint != gameManager.myPlayer.transform.position && randomSpawnPoint != gameManager.theirPlayer.transform.position)
            {
                GameObject old = masterGrid[randomSpawnPoint];
                GameObject replacement = elements[Random.Range(0, 6)];
                Replace(randomSpawnPoint, replacement);
                return;
            }
        }
    }

    public void CheckIfTapped()
    {
        bool tapped = true;

        int y = 0;

        for (int i = 0; i < 7; i++)
        {
            y++;
            int x = 0;

            for (int n = 0; n < 7; n++)
            {
                x++;

                Vector3 masterVec = new Vector3(x, y, 0);

                if (masterGrid[masterVec] != dirt)
                    tapped = false;
            }
        }

        if (tapped)
            FillByCode();

        print("tapped = " + tapped);

    }

    public void Replace(Vector3 pos, GameObject replacement)
    {
        GameObject old = masterGrid[pos];

        GameObject newObj = Instantiate(replacement, pos, Quaternion.identity);

        masterGrid[pos] = newObj;
        DestroyImmediate(old, true);
    }

    public void FillByCode()
    {
        string[] temp;

        temp = startString.Split(',');

        for (int i = 0; i < 49; i++)
        {
            if (temp[i] == "1")
                startMaterials[i] = dirt;
            if (temp[i] == "2")
                startMaterials[i] = water;
            if (temp[i] == "3")
                startMaterials[i] = plant;
            if (temp[i] == "4")
                startMaterials[i] = fire;
            if (temp[i] == "5")
                startMaterials[i] = rock;
            if (temp[i] == "6")
                startMaterials[i] = life;
            if (temp[i] == "7")
                startMaterials[i] = death;
            if (temp[i] == "8")
                startMaterials[i] = dragonVein;

        }

        startMatInt = 0;
        yInt = 0;

        for (int i = 0; i < 7; i++)
        {
            xInt = 0;
            yInt++;

            spawnVec = new Vector2(xInt, yInt);

            for (int n = 0; n < 7; n++)
            {
                xInt++;

                spawnVec = new Vector3(xInt, yInt, 0);
                Vector3 p1Vec = new Vector3(1, 4, 0);
                Vector3 p2Vec = new Vector3(7, 4, 0);

                if (spawnVec != p1Vec && spawnVec != p2Vec)
                    spawned = Instantiate(startMaterials[startMatInt], spawnVec, transform.rotation);
                else
                    spawned = Instantiate(dirt, spawnVec, transform.rotation);

                startMatInt++;

                currentInt = yInt * 10 + xInt;

                masterGrid.Add(spawnVec, spawned);
            }
        }

        Replace(gameManager.myPlayer.transform.position, dirt);
        Replace(gameManager.theirPlayer.transform.position, dirt);
    }

}
