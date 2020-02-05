using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Vector3 projectileStart;
    public Vector3 projectileEnd;
    public GameObject explosion;
    public AudioClip impactSound;
    public bool hasShake;
    
    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, projectileEnd, 6f * Time.deltaTime);

        if (transform.position == projectileEnd)
        {
            if (hasShake)
            {
                if(GameManager.instance != null)
                    GameManager.instance.CameraShake();
                else
                    GameManagerOffline.instance.CameraShake();
            }

            SoundManager.instance.PlaySingle(impactSound, 1);
            Destroy(this.gameObject);
            Instantiate(explosion, projectileEnd, Quaternion.identity);

            if (GameManager.instance != null)
                GameManager.instance.myPlayer.GetComponent<PlayerControl>().canMove = true;
            else
                GameManagerOffline.instance.myPlayer.GetComponent<PlayerControlOffline>().canMove = true;
        }
    }
}
