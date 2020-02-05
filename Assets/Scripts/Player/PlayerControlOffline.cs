using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControlOffline : MonoBehaviour
{
    private Vector3 fp;   
    private Vector3 lp;   
    private float dragDistance;

    [Header("Player")]
    public string screenName;
    public GameObject redMask;
    public GameObject blueMask;
    public bool isAI;

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
    [HideInInspector] public GameObject currentSprite;
    public List<GameObject> spriteList = new List<GameObject>();
    public Animator animator;
    public GameObject spiritWalkEffect;

    [Header("Health")]
    public bool poisoned;
    public bool burnt; 

    [Header("Managers")]
    private TerraformOffline terraformScript;
    public GameManagerOffline gameManager;
    public CellManagerOffline cellManager;
    public StatsManagerOffline statsManager;
    private SoundManager soundManager;

    [Header("Direction")]
    public bool facingFront = true;
    public bool facingBack;
    public bool facingLeft;
    public bool facingRight;

    [Header("SFX")]
    public AudioClip moveSound;

    private void Awake()
    {
        if (!isAI)
        {
            currentSprite = spriteList[SaveLoad.instance.characterSkin];
            redMask.SetActive(true);
        }
        else
        {
            currentSprite = spriteList[GetComponent<OpponentCreator>().skin];
            if(GetComponent<OpponentCreator>().skin == SaveLoad.instance.numOfAvailableSkins)
                blueMask.SetActive(false);
            else
                blueMask.SetActive(true);
        }

        currentSprite.SetActive(true);
        animator = currentSprite.GetComponent<Animator>();

        facingFront = true;

        if(!isAI)
            screenName = SaveLoad.instance.playerName;

    }

    void Start()
    {
        dragDistance = Screen.height * 8 / 100;

        terraformScript = GetComponent<TerraformOffline>();
        soundManager = SoundManager.instance;

        if (!isAI)
        {
            gameManager.playerControl = GetComponent<PlayerControlOffline>();
            gameManager.terraScript = GetComponent<TerraformOffline>();

            upButton = GameObject.FindGameObjectWithTag("UpButton");
            upButton.GetComponent<Button>().onClick.AddListener(delegate { MoveUp(); });

            downButton = GameObject.FindGameObjectWithTag("DownButton");
            downButton.GetComponent<Button>().onClick.AddListener(delegate { MoveDown(); });

            leftButton = GameObject.FindGameObjectWithTag("LeftButton");
            leftButton.GetComponent<Button>().onClick.AddListener(delegate { MoveLeft(); });

            rightButton = GameObject.FindGameObjectWithTag("RightButton");
            rightButton.GetComponent<Button>().onClick.AddListener(delegate { MoveRight(); });
        }

        cellManager.masterGrid.TryGetValue(targetPos, out targetObj);

        terraformScript.target = targetObj;

    }

    public void MoveUp()
    {
        targetPos = new Vector3(transform.position.x, transform.position.y + 1, 0);

        cellManager.masterGrid.TryGetValue(targetPos, out targetObj);

        if (facingBack)
        {

            if ((targetObj != null && targetObj.tag == "Dirt" && !OtherPlayerCheck()) || (targetObj != null && spiritWalk))
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
        if (!isAI)
        {
            if (poisoned)
            {
                gameManager.DisplayMessage(screenName + " was hurt by poison");
                statsManager.MyHealth(-20, true);
            }
            if (burnt)
            {
                gameManager.DisplayMessage(screenName + " was hurt by burn");
                statsManager.MyHealth(-20, true);
            }
        }
        else
        {
            if (poisoned)
            {
                gameManager.DisplayMessage(screenName + " was hurt by poison");
                statsManager.TheirHealth(-20, true);
            }
            if (burnt)
            {
                gameManager.DisplayMessage(screenName + " was hurt by burn");
                statsManager.TheirHealth(-20, true);
            }
        }
    }

    public bool OtherPlayerCheck()
    {
        if (isAI)
        {
            if (targetPos == gameManager.myPlayer.transform.position)
                return true;
            else
                return false;
        }
        else
        {
            if (targetPos == gameManager.theirPlayer.transform.position)
                return true;
            else
                return false;
        }

    }

    public void NextTurn()
    {
        terraformScript.energy = 4;
        if(!isAI)
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
        if (spiritWalk)
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
}
