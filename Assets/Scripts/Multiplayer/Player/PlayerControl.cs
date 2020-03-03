using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

/// <summary>
/// Multiplayer Version - Handles movement controls and movement animation for the player object
/// </summary>
public class PlayerControl : MonoBehaviourPunCallbacks
{
    SaveLoad saveLoad;

    [Header("Player")]
    public string screenName; //UI display for local players username
    public GameObject redMask; //A marker to distinguish which player belongs to local player
    public GameObject blueMask; //A marker to distinguish which player belongs to local player

    [Header("Movement")]
    public bool canMove; //To easily stop movement
    public Vector3 startPosition;
    public bool spiritWalk; //Whether player has spirit walk effect active
    public GameObject upButton;
    public GameObject downButton;
    public GameObject leftButton;
    public GameObject rightButton;

    [Header("Target")]
    public Vector3 targetPos; //The grid position currently targeted
    public GameObject targetObj; //The object that is in the target position

    [Header("Animation")]
    public GameObject currentSprite; //The active character skin sprite
    public List<GameObject> spriteList = new List<GameObject>(); //A list of all possible character skin sprites
    [HideInInspector] public Animator animator; //The animator of the current character skin sprite
    public GameObject spiritWalkEffect; //Particle effect object to be activated when spirit walk spell is active

    [Header("Health")]
    public bool poisoned; //Whether or not this player is poisoned
    public bool burnt; //Whether or not this player is burnt

    [Header("Managers")]
    private Terraform terraformScript;
    private GameManager gameManager;
    private CellManager cellManager;
    private StatsManager statsManager;
    private SoundManager soundManager;

    [Header("Direction")]
    public bool facingFront = false;
    public bool facingBack = true;
    public bool facingLeft = false;
    public bool facingRight = false;

    [Header("SFX")]
    public AudioClip moveSound; //Sound playen when moving tile to tile

    [Header("Networking")]
    public PhotonView PV;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();

        saveLoad = SaveLoad.instance;

        gameManager = GameManager.instance;
        cellManager = CellManager.instance;

        
        if (PV.IsMine) //Check that this PlayerControl script belongs to the local player
        {
            currentSprite = spriteList[SaveLoad.instance.characterSkin]; //Select the character skin sprite for local player

            transform.parent.name = "My player";
            gameManager.myPlayer = this.gameObject;
            gameManager.playerControl = GetComponent<PlayerControl>();
            gameManager.terraScript = GetComponent<Terraform>();
            gameManager.players.Add(this.gameObject);
        }
        else
        {
            currentSprite = spriteList[gameManager.otherPlayerSkin]; //Select the character skin sprite for other player(s)

            transform.parent.name = "Their player";
            gameManager.theirPlayer = this.gameObject;
        }

        currentSprite.SetActive(true);
        animator = currentSprite.GetComponent<Animator>();

        //Determines which player goes first by which one is the master client
        if (!PhotonNetwork.IsMasterClient)
        {
            AnimBoolFalse();
            facingLeft = true;
            animator.SetBool("FaceLeft", true);
            screenName = SaveLoad.instance.playerName;

            if(PV.IsMine)
                blueMask.SetActive(true);
            else
                redMask.SetActive(true);
        }
        else
        {
            AnimBoolFalse();
            facingRight = true;
            animator.SetBool("FaceRight", true);
            screenName = SaveLoad.instance.playerName;

            if (PV.IsMine)
                redMask.SetActive(true);
            else
                blueMask.SetActive(true);
        }
    }

    void Start()
    {
        transform.parent.transform.position = startPosition;

        terraformScript = GetComponent<Terraform>();
        statsManager = StatsManager.instance;
        soundManager = SoundManager.instance;

        //Set targetObj to the element in front of player using masterGrid in CellManager
        cellManager.masterGrid.TryGetValue(targetPos, out targetObj);

        //Pass targetObj to Terraform script target
        terraformScript.target = targetObj;

        if (PV.IsMine)
        {
            //Set all UI buttons to apropriate functions
            upButton = GameObject.FindGameObjectWithTag("UpButton");
            upButton.GetComponent<Button>().onClick.AddListener(delegate { MoveUp(); });

            downButton = GameObject.FindGameObjectWithTag("DownButton");
            downButton.GetComponent<Button>().onClick.AddListener(delegate { MoveDown(); });

            leftButton = GameObject.FindGameObjectWithTag("LeftButton");
            leftButton.GetComponent<Button>().onClick.AddListener(delegate { MoveLeft(); });

            rightButton = GameObject.FindGameObjectWithTag("RightButton");
            rightButton.GetComponent<Button>().onClick.AddListener(delegate { MoveRight(); });
        }


    }

    /// <summary>
    /// Moves the player up if they are facing up otherwise faces them up
    /// </summary>
    public void MoveUp()
    {
        Debug.Log("MoveUp");

        targetPos = new Vector3(transform.position.x, transform.position.y + 1, 0);

        cellManager.masterGrid.TryGetValue(targetPos, out targetObj);

        if (facingBack)
        {

            if ((targetObj != null && targetObj.tag == "Dirt" && !OtherPlayerCheck()) || (targetObj != null &&  spiritWalk))
            {
               
                MovePosition();
                targetPos = new Vector3(transform.position.x, transform.position.y + 1, 0);

                cellManager.masterGrid.TryGetValue(targetPos, out targetObj);
            }

        }
        else
        {
            FacingBoolReset();
            facingBack = true;

            AnimBoolFalse();
            animator.SetBool("FaceBack", true);
        }

        terraformScript.target = targetObj;
    }

    /// <summary>
    /// Moves the player down if they are facing up otherwise faces them down
    /// </summary>
    public void MoveDown()
    {

        Debug.Log("MoveDown");

        targetPos = new Vector3(transform.position.x, transform.position.y - 1, 0);

        cellManager.masterGrid.TryGetValue(targetPos, out targetObj);

        if (facingFront)
        {

            if ((targetObj != null && targetObj.tag == "Dirt" && !OtherPlayerCheck()) || (targetObj != null && spiritWalk))
            {
                
                MovePosition();
                targetPos = new Vector3(transform.position.x, transform.position.y - 1, 0);

                cellManager.masterGrid.TryGetValue(targetPos, out targetObj);
            }

        }
        else
        {
            FacingBoolReset();
            facingFront = true;

            AnimBoolFalse();
            animator.SetBool("FaceFront", true);
        }

        terraformScript.target = targetObj;
    }

    /// <summary>
    /// Moves the player left if they are facing up otherwise faces them left
    /// </summary>
    public void MoveLeft()
    {
        targetPos = new Vector3(transform.position.x - 1, transform.position.y, 0);

        cellManager.masterGrid.TryGetValue(targetPos, out targetObj);

        if (facingLeft)
        {

            if ((targetObj != null && targetObj.tag == "Dirt" && !OtherPlayerCheck()) || (targetObj != null && spiritWalk))
            {
                MovePosition();
                targetPos = new Vector3(transform.position.x - 1, transform.position.y, 0);

                cellManager.masterGrid.TryGetValue(targetPos, out targetObj);
            }

        }
        else
        {
            FacingBoolReset();
            facingLeft = true;

            AnimBoolFalse();
            animator.SetBool("FaceLeft", true);
        }

        terraformScript.target = targetObj;
    }

    /// <summary>
    /// Moves the player right if they are facing up otherwise faces them right
    /// </summary>
    public void MoveRight()
    {
        targetPos = new Vector3(transform.position.x + 1, transform.position.y, 0);

        cellManager.masterGrid.TryGetValue(targetPos, out targetObj);

        if (facingRight)
        {
            if ((targetObj != null && targetObj.tag == "Dirt" && !OtherPlayerCheck()) || (targetObj != null && spiritWalk))
            {
                
                MovePosition();
                targetPos = new Vector3(transform.position.x + 1, transform.position.y, 0);

                cellManager.masterGrid.TryGetValue(targetPos, out targetObj);
            }
        }
        else
        {
            FacingBoolReset();
            facingRight = true;

            AnimBoolFalse();
            animator.SetBool("FaceRight", true);
        }

        terraformScript.target = targetObj;
    }

    /// <summary>
    /// Sets all facing direction bools in animator to false
    /// </summary>
    public void AnimBoolFalse()
    {
        animator.SetBool("FaceFront", false);
        animator.SetBool("FaceBack", false);
        animator.SetBool("FaceLeft", false);
        animator.SetBool("FaceRight", false);
    }

    /// <summary>
    /// Sets all facing direction bools in this PlayerControl to false
    /// </summary>
    public void FacingBoolReset()
    {
        facingBack = false;
        facingLeft = false;
        facingRight = false;
        facingFront = false;
    }

    /// <summary>
    /// Grouping of things to be done on every movement
    /// </summary>
    public void MovePosition()
    {
        soundManager.PlaySingle(moveSound, 1);
        transform.position = targetPos;
    }

    /// <summary>
    /// Function to check if the player is poisoned or burnt at the end of each turn - if so it also sends a message to remove health
    /// </summary>
    public void DoT()
    {
        if (poisoned)
        {
            gameManager.DisplayPublicMessage(screenName + " was hurt by poison");
            statsManager.MyHealth(-20, true);
        }
        if (burnt)
        {
            gameManager.DisplayPublicMessage(screenName + " was hurt by burn");
            statsManager.MyHealth(-20, true);
        }
    }

    /// <summary>
    /// Returns true if the space we are trying to move to is already occupied by another player
    /// </summary>
    public bool OtherPlayerCheck()
    {
        if (targetPos == gameManager.theirPlayer.transform.position)
            return true;
        else
            return false;

    }

    /// <summary>
    /// Refills energy at the beginning of each turn
    /// </summary>
    public void NextTurn()
    {
        terraformScript.energy = 4;
        terraformScript.CheckEnergy();
    }

    /// <summary>
    /// Sets burnt bool to false
    /// </summary>
    public void UnBurn()
    {
        burnt = false;
    }

    /// <summary>
    /// Sets poisoned bool to false
    /// </summary>
    public void UnPoision()
    {
		poisoned = false;
	}

    /// <summary>
    /// Turns spirit walk on and off
    /// </summary>
    public void SpiritWalk()
    {
        if(spiritWalk)
        {
            spiritWalk = false;
            spiritWalkEffect.SetActive(false);
        }
        else
        {
            spiritWalk = true;
            spiritWalkEffect.SetActive(true);
        }
    }

    /// <summary>
    /// Sends message to other player that local player has changed status of spirit walk
    /// </summary>
    [PunRPC]
    public void RPC_SpiritWalk()
    {
        gameManager.theirPlayer.GetComponent<PlayerControl>().SpiritWalk();
    }
}
