using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Behavious script for an in game projectile
/// </summary>
public class Projectile : MonoBehaviour
{
    public Vector3 projectileStart;
    public Vector3 projectileEnd;
    public GameObject explosion; //Visual effect for when this projectile explodes
    public AudioClip impactSound;
    public bool hasShake; //Whether this projectile causes screen shake upon collision
    
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, projectileEnd, 6f * Time.deltaTime); //Move the projectile toward target

        if (transform.position == projectileEnd) //Check to see when the projectile hit's it's target location
        {
            if (hasShake) //Check if the projectile has screen shake
            {
                if(GameManager.instance != null)
                    GameManager.instance.CameraShake();
                else
                    GameManagerOffline.instance.CameraShake();
            }

            SoundManager.instance.PlaySingle(impactSound, 1); //Play impace sound 
            Destroy(this.gameObject); //Destroy the projectile 
            Instantiate(explosion, projectileEnd, Quaternion.identity); //Instantiate explosion VFX

            //Send projectile info to tournament host
            if (!SaveLoad.instance.tournamentHost)
            {
                if (GameManager.instance != null)
                    GameManager.instance.myPlayer.GetComponent<PlayerControl>().canMove = true;
                else
                    GameManagerOffline.instance.myPlayer.GetComponent<PlayerControlOffline>().canMove = true;
            }
        }
    }
}
