using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Multiplayer Version - Allows the user to interract with the dialogue box
/// </summary>
public class MessageBox : MonoBehaviour
{
    public GameObject tapToClose;

    private void Start()
    {
        Invoke("TapToClose", 10);
    }

    void TapToClose()
    {
        tapToClose.SetActive(true);
    }

    public void DestroyMe()
    {
        if(GameManager.instance != null)
            GameManager.instance.CloseMessage();
        else
            Destroy(this.gameObject);
    }
}
