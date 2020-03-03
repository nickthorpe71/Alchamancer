using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Makes an image fade out over a specified amount of seconds
/// </summary>
public class FadeInOut : MonoBehaviour
{
    private Text text;
    public float fadeInSpeed;
    public float fadeOutSpeed;
    public float startSpeed;

    private void Start()
    {
        text = GetComponent<Text>();

        InvokeRepeating("FadeIn", startSpeed, fadeInSpeed + fadeOutSpeed + fadeInSpeed/4);
    }

    void FadeIn()
    {
        if (this.gameObject.activeSelf)
            StartCoroutine(FadeTextToFullAlpha(fadeInSpeed, text));
        else
        {
            StopAllCoroutines();
            CancelInvoke("FadeIn");
        }
    }

    void FadeOut()
    {
        if (this.gameObject.activeSelf)
            StartCoroutine(FadeTextToZeroAlpha(fadeOutSpeed, text));
        else
        {
            StopAllCoroutines();
            CancelInvoke("FadeIn");
        }
    }

	public IEnumerator FadeTextToFullAlpha(float t, Text i)
	{
		i.color = new Color(i.color.r, i.color.g, i.color.b, 0);
		while (i.color.a < 1.0f)
		{
			i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a + (Time.deltaTime / t));
			yield return null;
		}

        FadeOut();
	}

	public IEnumerator FadeTextToZeroAlpha(float t, Text i)
	{
		i.color = new Color(i.color.r, i.color.g, i.color.b, 1);
		while (i.color.a > 0.0f)
		{
			i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a - (Time.deltaTime / t));
			yield return null;
		}
	}
}
