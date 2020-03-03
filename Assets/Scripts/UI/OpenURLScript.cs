using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sends user to specified URL
/// </summary>
public class OpenURLScript : MonoBehaviour
{
    public string URL;

    public void OpenLink()
    {
        Application.OpenURL(URL);
    }

}
