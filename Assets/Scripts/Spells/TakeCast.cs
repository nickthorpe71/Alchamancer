using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script to fade in and out Take Cast effect
/// </summary>
public class TakeCast : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(FadeInOut());
        Destroy(this, 2);
    }
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.T))
        {
            StartCoroutine(FadeTo(0.0f, 0.25f));
        }
        if (Input.GetKeyUp(KeyCode.F))
        {
            StartCoroutine(FadeTo(8.0f, 0.25f));
        }
    }

    IEnumerator FadeTo(float aValue, float aTime)
    {
        float alpha = GetComponent<SpriteRenderer>().color.a;
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / aTime)
        {
            Color newColor = new Color(0, 0, 0, Mathf.Lerp(alpha, aValue, t));
            GetComponent<SpriteRenderer>().color = newColor;
            yield return null;
        }
    }

    IEnumerator FadeInOut()
    {
        StartCoroutine(FadeTo(1.0f, 0.25f));
        yield return new WaitForSeconds(0.25f);
        StartCoroutine(FadeTo(0.0f, 0.25f));
    }
}
