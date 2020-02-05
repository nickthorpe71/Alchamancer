﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class Terraform : MonoBehaviourPunCallbacks
{
    private GameManager gameManager;
    private CellManager cellManager;
    private SoundManager soundManager;

    [Header("Elements")]
    public int waterCount;
    public int plantCount;
    public int fireCount;
    public int rockCount;
    public int lifeCount;
    public int deathCount;
    public GameObject takeCast;
    public GameObject dirt, water, plant, fire, rock, life, death;

    [Header("Player")]
    private PlayerControl playerCon;
    public GameObject target;
    public bool canTake;
    public float castSpeed = 0.4f;
    private GameObject temp;
    public Animator animator;
    public SpellSO antidoteSpell;
    
    [Header("Energy and Mana")]
    public int energy = 4;
    public GameObject[] energyImages;
    public int currentMana;
    public int maxMana = 8;
    public GameObject[] manaImages;

    [Header ("SFX")]
    public AudioClip takeSound;

    [Header("Buttons")]
    public GameObject takeButton;

    private void Start()
    {
        cellManager = CellManager.instance;
        gameManager = GameManager.instance;
        soundManager = SoundManager.instance;
        playerCon = GetComponent<PlayerControl>();

        animator = playerCon.animator;

        energyImages = gameManager.energyImages;
        manaImages = gameManager.manaImages;

        if (!gameManager.gameStarted)
        {
            energy = 4;
            CheckEnergy();
            CheckMana();
        }

        dirt = cellManager.dirt;
        water = cellManager.water;
        plant = cellManager.plant;
        fire = cellManager.fire;
        rock = cellManager.rock;
        life = cellManager.life;
        death = cellManager.death;

        takeButton = GameObject.FindGameObjectWithTag("Take");
        takeButton.GetComponent<Button>().onClick.AddListener(delegate { Take(false); });

    }

    public void DetermineTarget()
    {
        Vector3 relativePos = transform.position;

        if(playerCon.facingFront)
            target = cellManager.masterGrid[new Vector3(relativePos.x, relativePos.y - 1, relativePos.z)];
        if (playerCon.facingBack)
            target = cellManager.masterGrid[new Vector3(relativePos.x, relativePos.y + 1, relativePos.z)];
        if (playerCon.facingLeft)
            target = cellManager.masterGrid[new Vector3(relativePos.x - 1, relativePos.y, relativePos.z)];
        if (playerCon.facingRight)
            target = cellManager.masterGrid[new Vector3(relativePos.x + 1, relativePos.y, relativePos.z)];
    }

    public void Take(bool eatMana)
    {
        if (playerCon.PV.IsMine)
        {
            DetermineTarget();

            if (currentMana < maxMana || eatMana)
            {
                if (energy > 0 || eatMana)
                {
                    if (target.tag != "Dirt" && target != null)
                    {

                        if (target.tag != "DragonVein" && target != null)
                        {
                            Cast();
                            Vector3 tempVec = target.transform.position;
                            switch (target.tag)
                            {
                                case "Rock":
                                    cellManager.Replace(tempVec, dirt);
                                    if (eatMana)
                                        rockCount += 2;
                                    else
                                        rockCount++;
                                    break;
                                case "Water":
                                    cellManager.Replace(tempVec, dirt);
                                    if (eatMana)
                                        waterCount += 2;
                                    else
                                        waterCount++;
                                    break;
                                case "Plant":
                                    cellManager.Replace(tempVec, dirt);
                                    if (eatMana)
                                        plantCount += 2;
                                    else
                                        plantCount++;
                                    break;
                                case "Fire":
                                    cellManager.Replace(tempVec, dirt);
                                    if (eatMana)
                                        fireCount += 2;
                                    else
                                        fireCount++;
                                    break;
                                case "Life":
                                    cellManager.Replace(tempVec, dirt);
                                    if (eatMana)
                                        lifeCount += 2;
                                    else
                                        lifeCount++;
                                    break;
                                case "Death":
                                    cellManager.Replace(tempVec, dirt);
                                    if (eatMana)
                                        deathCount += 2;
                                    else
                                        deathCount++;
                                    break;
                            }

                            PostTake(eatMana);
                            DetermineTarget();
                        }
                        else
                        {
                            gameManager.DisplayMessage("Cannont absorb this ancient material.");
                        }
                    }
                    else
                    {
                        gameManager.DisplayMessage("Nothing here.");
                    }
                }
                else
                {
                    gameManager.DisplayMessage("You are out of energy until next turn.");
                }
            }
            else
            {
                gameManager.DisplayMessage("Your body cannot hold any more mana. You must cast a spell to make room.");
            }
        }

        if (gameManager.spellCaster.sorted)
            gameManager.spellCaster.SortButtonSorted();
        gameManager.spellCaster.CheckCosts();

    }

    public void PostTake(bool eatMana)
    {
        soundManager.PlaySinglePublic("takeSound", 0.8f);

        if (!eatMana)
        {
            energy--;
            currentMana++;
        }
        else
            currentMana += 2;

        CheckEnergy();
        CheckMana();
        gameManager.UpdateElements();

        DetermineTarget();

        Quaternion castRotation = Quaternion.Euler(0, 0, 0);

        temp = Instantiate(takeCast, target.transform.position, Quaternion.identity);
        if (playerCon.facingBack)
            castRotation = Quaternion.Euler(90, 0, 0);
        if (playerCon.facingFront)
            castRotation = Quaternion.Euler(270, 0, 0);
        if (playerCon.facingRight)
            castRotation = Quaternion.Euler(180, 90, 0);
        if (playerCon.facingLeft)
            castRotation = Quaternion.Euler(0, 90, 0);

        temp.transform.GetChild(0).transform.rotation = castRotation;

        base.photonView.RPC("SpawnTakeCastOther", RpcTarget.Others, target.transform.position, castRotation);
    }

    [PunRPC]
    public void SpawnTakeCastOther(Vector3 position, Quaternion rotation)
    {
        temp = Instantiate(takeCast, position, Quaternion.identity);
        temp.transform.GetChild(0).transform.rotation = rotation;
    }

    public void Cast()
    {
        if (playerCon.facingFront)
            CastFront();
        if (playerCon.facingBack)
            CastBack();
        if (playerCon.facingLeft)
            CastLeft();
        if (playerCon.facingRight)
            CastRight();
        StartCoroutine(CastRoutine());
    }

    public IEnumerator CastRoutine()
    {
        playerCon.canMove = false;
        yield return new WaitForSeconds(castSpeed);
        playerCon.canMove = true;
    }

    public void CastFront()
    {
        ResetAnim();
        string trigger = "CastFront";
        animator.SetTrigger(trigger);
        base.photonView.RPC("OtherCastAnim", RpcTarget.Others, trigger);
    }

    public void CastBack()
    {
        ResetAnim();
        string trigger = "CastBack";
        animator.SetTrigger(trigger);
        base.photonView.RPC("OtherCastAnim", RpcTarget.Others, trigger);
    }
    public void CastLeft()
    {
        ResetAnim();
        string trigger = "CastLeft";
        animator.SetTrigger(trigger);
        base.photonView.RPC("OtherCastAnim", RpcTarget.Others, trigger);
    }
    public void CastRight()
    {
        ResetAnim();
        string trigger = "CastRight";
        animator.SetTrigger(trigger);
        base.photonView.RPC("OtherCastAnim", RpcTarget.Others, trigger);
    }

    [PunRPC]
    public void OtherCastAnim(string trigger)
    {
        gameManager.theirPlayer.GetComponent<PlayerControl>().currentSprite.GetComponent<Animator>().SetTrigger(trigger);
    }

    public void ResetAnim()
    {
        animator.ResetTrigger("CastFront");
        animator.ResetTrigger("CastBack");
        animator.ResetTrigger("CastLeft");
        animator.ResetTrigger("CastRight");
    }

    public void CheckEnergy()
    {
        
        for (int i = 0; i < energyImages.Length; i++)
        {
            energyImages[i].SetActive(false);
        }

        if (energy >= 4)
            energy = 4;

        if (energy <= 0)
        {
            canTake = false;
            energy = 0;
        }
        else
            canTake = true;

        for (int i = 0; i < energy; i++)
        {
            energyImages[i].SetActive(true);
        }
    }

    public void CheckMana()
    {

        for (int i = 0; i < manaImages.Length; i++)
        {
            manaImages[i].SetActive(false);
        }

        if (currentMana >= maxMana)
            currentMana = maxMana;

        if (currentMana <= 0)
            currentMana = 0;

        for (int i = 0; i < currentMana; i++)
            manaImages[i].SetActive(true);
    }
}