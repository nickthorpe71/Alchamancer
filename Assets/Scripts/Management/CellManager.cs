using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class CellManager : MonoBehaviourPunCallbacks, IInRoomCallbacks
{
    public static CellManager instance;

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
    public SpellCaster spellCaster;
    private GameManager gameManager;
    private Carry carryScript;


    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        gameManager = GameManager.instance;
        carryScript = Carry.instance;

        currentInt = 0;

        elements.Add(water);
        elements.Add(plant);
        elements.Add(fire);
        elements.Add(rock);
        elements.Add(life);
        elements.Add(death);

        /*if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                startString = carryScript.levelString;
                FillByCode();
            }
        }*/

        startString = realString;
        FillByCode();
    }

    public void SpawnRandom()
    {
        for (int i = 0; i < 100; i++)
        {
            Vector3 randomSpawnPoint = new Vector3(Random.Range(1, 8), Random.Range(1, 8), 0);

            if (masterGrid[randomSpawnPoint].tag == "Dirt" && randomSpawnPoint != gameManager.myPlayer.transform.position
                && masterGrid[randomSpawnPoint].tag != "DragonVein" && randomSpawnPoint != gameManager.theirPlayer.transform.position)
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
            base.photonView.RPC("RPC_FillByCode", RpcTarget.All);
    }

    public void Replace(Vector3 pos, GameObject replacement)
    {
        GameObject old = masterGrid[pos];

        GameObject newObj = Instantiate(replacement, pos, Quaternion.identity);

        masterGrid[pos] = newObj;
        DestroyImmediate(old, true);

        string replaceString = newObj.tag;

        base.photonView.RPC("RPC_OtherReplace", RpcTarget.OthersBuffered, pos, replaceString);
    }

    [PunRPC]
    private void RPC_OtherReplace(Vector3 pos, string replaceString)
    {
        GameObject old = masterGrid[pos];

        GameObject newObj = new GameObject();

        if (replaceString == "Dirt")
            newObj = Instantiate(dirt, pos, Quaternion.identity);
        if (replaceString == "Water")
            newObj = Instantiate(water, pos, Quaternion.identity);
        if (replaceString == "Plant")
            newObj = Instantiate(plant, pos, Quaternion.identity);
        if (replaceString == "Fire")
            newObj = Instantiate(fire, pos, Quaternion.identity);
        if (replaceString == "Rock")
            newObj = Instantiate(rock, pos, Quaternion.identity);
        if (replaceString == "Life")
            newObj = Instantiate(life, pos, Quaternion.identity);
        if (replaceString == "Death")
            newObj = Instantiate(death, pos, Quaternion.identity);

        masterGrid[pos] = newObj;
        DestroyImmediate(old, true);
    }

    public void FillByCode()
    {
        //create array for string
        string[] temp;
        //fill the array with component from data string by spltting at commas
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
    }

    void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.AddCallbackTarget(this);
        SceneManager.sceneLoaded += OnSceneFinishedLoading;
    }

    void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.RemoveCallbackTarget(this);
        SceneManager.sceneLoaded -= OnSceneFinishedLoading;
    }

    void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        Carry.instance.currentScene = scene.buildIndex;
        if (Carry.instance.currentScene == Carry.instance.levelScene)
        {
            PhotonNetwork.Instantiate("NetworkPlayer", new Vector2(0, 0), Quaternion.identity);

            /*if(PhotonNetwork.IsMasterClient)
                base.photonView.RPC("RPC_PlayerJoined", RpcTarget.Others, Carry.instance.levelString);*/
        }
    }

    [PunRPC]
    private void RPC_FillByCode()
    {
        FillByCode();
    }

    //This receives the RPC
    [PunRPC]
    private void RPC_PlayerJoined(string LevelString)
    {
        if (!gameManager.gameStarted)
        {
            carryScript.levelString = LevelString;
            currentInt = 0;
            startString = LevelString;
            FillByCode();
        }
    }
}
