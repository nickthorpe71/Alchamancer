using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Makes an image fade in over a specified amount of seconds
/// </summary>
public class FadeIn : MonoBehaviour
{
    private Image image;
    public float fadeInSpeed;

    private void Start()
    {
        image = GetComponent<Image>();
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
        StartCoroutine(FadeTextToFullAlpha(fadeInSpeed, image));
    }

    public IEnumerator FadeTextToFullAlpha(float t, Image i)
    {
        i.color = new Color(i.color.r, i.color.g, i.color.b, 0);
        while (i.color.a < 1.0f)
        {
            i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a + (Time.deltaTime / t));
            yield return null;
        }
    }
}
