using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Single Player Version - Allows the user to interract with the dialogue box
/// </summary>
public class MessageBoxOffline : MonoBehaviour
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
        GameManagerOffline.instance.CloseMessage();
    }
}
