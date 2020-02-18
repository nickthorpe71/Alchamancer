using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class AreaColorManager : MonoBehaviourPunCallbacks
{
    public static AreaColorManager instance;

    public GameObject[] environments;

    public Color neutral;
    public Color blue;
    public Color green;
    public Color red;
    public Color yellow;
    public Color white;
    public Color black;

    [HideInInspector] public string currentAreaColor = "Neutral";

    public Image MainColor;
    public Image orb1;
    public Image orb2;

    private string[] currentColors = new string[3];
    private Dictionary<int, string> allColorStrings = new Dictionary<int, string>();
    private Dictionary<string, Color> allColors = new Dictionary<string, Color>();

    private bool firstTurn = true;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        allColorStrings.Add(0, "Blue");
        allColorStrings.Add(1, "Green");
        allColorStrings.Add(2, "Red");
        allColorStrings.Add(3, "Yellow");
        allColorStrings.Add(4, "White");
        allColorStrings.Add(5, "Black");

        allColors.Add("Neutral", neutral);
        allColors.Add("Blue", blue);
        allColors.Add("Green", green);
        allColors.Add("Red", red);
        allColors.Add("Yellow", yellow);
        allColors.Add("White", white);
        allColors.Add("Black", black);

        if (PhotonNetwork.IsMasterClient)
            RandomizeColors();

        MainColor.color = neutral;
        currentAreaColor = "Neutral";
    }

    private void RandomizeColors()
    {
        for (int i = 0; i < 2; i++)
        {
            int temp = Random.Range(0, 6);
            currentColors[i] = allColorStrings[temp];
        }

        UpdateColorImages();

        base.photonView.RPC("RPC_RandomizeColors", RpcTarget.Others, currentColors[0], currentColors[1]);
    }

    [PunRPC]
    private void RPC_RandomizeColors(string color0, string color1)
    {
        currentColors[0] = color0;
        currentColors[1] = color1;

        UpdateColorImages();
    }

    public void Cycle(string newColor)
    {
        base.photonView.RPC("RPC_Cycle", RpcTarget.All, newColor);
    }

    [PunRPC]
    public void RPC_Cycle(string newColor)
    {
        currentColors[1] = currentColors[0];
        currentColors[0] = newColor;

        UpdateColorImages();
    }

    private void UpdateColorImages()
    {
        orb1.color = allColors[currentColors[0]];
        orb2.color = allColors[currentColors[1]];

        CheckMainColor();
    }

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
                    GameManager.instance.DisplayPublicMessage("Water and ice enclose the battlefield while rain pours down. Blue spells will now be twice as effective and Red spells half as effective.");
                    environments[1].SetActive(true);
                    break;

                case "Green":
                    GameManager.instance.DisplayPublicMessage("All manner of flora have beset the field. Green spells will now be twice as effective and Yellow spells half as effective.");
                    environments[2].SetActive(true);
                    break;

                case "Red":
                    GameManager.instance.DisplayPublicMessage("The temperature has piqued surrounding the field in magma. Red spells will now be twice as effective and Blue spells half as effective.");
                    environments[3].SetActive(true);
                    break;

                case "Yellow":
                    GameManager.instance.DisplayPublicMessage("The battlefield has become arid and bouldered. Yellow spells will now be twice as effective and Green spells half as effective.");
                    environments[4].SetActive(true);
                    break;

                case "White":
                    GameManager.instance.DisplayPublicMessage("Heavenly light envelopes the field. White spells will now be twice as effective and Black spells half as effective.");
                    environments[5].SetActive(true);
                    break;

                case "Black":
                    GameManager.instance.DisplayPublicMessage("The field has fallen into an eerie darkness. Black spells will now be twice as effective and White spells half as effective.");
                    environments[6].SetActive(true);
                    break;
            }

            GameManager.instance.CameraShake();
        }

        firstTurn = false;
    }

    void EnvironmentsOff()
    {
        for (int i = 0; i < environments.Length; i++)
        {
            environments[i].SetActive(false);
        }
    }

}
