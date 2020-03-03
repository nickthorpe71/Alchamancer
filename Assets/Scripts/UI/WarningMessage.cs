using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to show warning before online matches
/// </summary>
public class WarningMessage : MonoBehaviour
{
    public GameObject warningBox;
    public GameObject tap;
    public GameObject loading;

    private void Start()
    {
        if(!warningBox.activeSelf)
            warningBox.SetActive(true);

        if(tap.activeSelf)
            tap.SetActive(false);

        if(!loading.activeSelf)
            loading.SetActive(true);

        StartCoroutine("StopLoading");
    }

    IEnumerator StopLoading()
    {
        yield return new WaitForSeconds(3.5f);
        tap.SetActive(true);
        loading.SetActive(false);
    }

    public void Continue()
    {
        tap.SetActive(false);
        loading.SetActive(false);
        warningBox.SetActive(false);
    }
}
