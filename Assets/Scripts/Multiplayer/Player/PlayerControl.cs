using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class PlayerControl : MonoBehaviourPunCallbacks
{
    private Vector3 fp;   //First touch position
    private Vector3 lp;   //Last touch position
    private float dragDistance;  //minimum distance for a swipe to be registered

    SaveLoad saveLoad;

    [Header("Player")]
    public string screenName;
    public GameObject redMask;
    public GameObject blueMask;

    [Header("Movement")]
    public Vector3 source;
    public bool canMove;
    public Vector3 startPosition;
    public bool spiritWalk;
    public GameObject upButton;
    public GameObject downButton;
    public GameObject leftButton;
    public GameObject rightButton;

    [Header("Target")]
    public Vector3 targetPos;
    public GameObject targetObj;

    [Header("Animation")]
    public GameObject currentSprite;
    public List<GameObject> spriteList = new List<GameObject>();
    [HideInInspector] public Animator animator;
    public GameObject spiritWalkEffect;

    [Header("Health")]
    public bool poisoned;
    public bool burnt;

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
    public AudioClip moveSound;

    [Header("Networking")]
    public PhotonView PV;

    [Header("AI")]
    private AIController aiController;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();

        saveLoad = SaveLoad.instance;

        gameManager = GameManager.instance;
        cellManager = CellManager.instance;

        aiController = GetComponent<AIController>();

        if (PV.IsMine)
        {
            if (Carry.instance.aIAcvive)
                aiController.enabled = true;

            currentSprite = spriteList[SaveLoad.instance.characterSkin];

            transform.parent.name = "My player";
            gameManager.myPlayer = this.gameObject;
            gameManager.playerControl = GetComponent<PlayerControl>();
            gameManager.terraScript = GetComponent<Terraform>();
            gameManager.players.Add(this.gameObject);
        }
        else
        {
            currentSprite = spriteList[gameManager.otherPlayerSkin];

            transform.parent.name = "Their player";
            gameManager.theirPlayer = this.gameObject;
        }

        currentSprite.SetActive(true);
        animator = currentSprite.GetComponent<Animator>();

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
        dragDistance = Screen.height * 8 / 100;

        transform.parent.transform.position = startPosition;

        terraformScript = GetComponent<Terraform>();
        statsManager = StatsManager.instance;
        soundManager = SoundManager.instance;

        cellManager.masterGrid.TryGetValue(targetPos, out targetObj);

        terraformScript.target = targetObj;

        if (PV.IsMine)
        {
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


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            AnimBoolFalse();

            if (blueMask.activeSelf)  
                animator.SetBool("Death", true);
            else
                animator.SetBool("Win", true);
        }

    }


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

    public void KeyboardControls()
    {    
        if (Input.GetKeyDown(KeyCode.W))
        {
            MoveUp();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            MoveDown();
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            MoveLeft();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            MoveRight();
        }
    }

    public void AnimBoolFalse()
    {
        animator.SetBool("FaceFront", false);
        animator.SetBool("FaceBack", false);
        animator.SetBool("FaceLeft", false);
        animator.SetBool("FaceRight", false);
    }

    public void FacingBoolReset()
    {
        facingBack = false;
        facingLeft = false;
        facingRight = false;
        facingFront = false;
    }

    public void MovePosition()
    {
        soundManager.PlaySingle(moveSound, 1);
        transform.position = targetPos;
    }

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

    public bool OtherPlayerCheck()
    {
        if (targetPos == gameManager.theirPlayer.transform.position)
            return true;
        else
            return false;

    }

    public void NextTurn()
    {
        terraformScript.energy = 4;
        terraformScript.CheckEnergy();
    }

    public void UnBurn()
    {
        burnt = false;
    }

    public void UnPoision()
    {
		poisoned = false;
	}

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

    [PunRPC]
    public void RPC_SpiritWalk()
    {
        gameManager.theirPlayer.GetComponent<PlayerControl>().SpiritWalk();
    }
}
