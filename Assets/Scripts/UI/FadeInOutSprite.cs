using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeInOutSprite : MonoBehaviour
{
    private SpriteRenderer sprite;
    public float fadeInSpeed;
    public float fadeOutSpeed;

    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();

        InvokeRepeating("FadeIn", 0, fadeInSpeed + fadeOutSpeed + fadeInSpeed/4);
    }

    void FadeIn()
    {
        StartCoroutine(FadeTextToFullAlpha(fadeInSpeed, sprite));
    }

    void FadeOut()
    {
        StartCoroutine(FadeTextToZeroAlpha(fadeOutSpeed, sprite));
    }

	public IEnumerator FadeTextToFullAlpha(float t, SpriteRenderer i)
	{
		i.color = new Color(i.color.r, i.color.g, i.color.b, 0);
		while (i.color.a < 1.0f && this.gameObject.activeSelf)
		{
			i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a + (Time.deltaTime / t));
			yield return null;
		}

        if (this.gameObject.activeSelf)
            FadeOut();
	}

	public IEnumerator FadeTextToZeroAlpha(float t, SpriteRenderer i)
	{
		i.color = new Color(i.color.r, i.color.g, i.color.b, 1);
		while (i.color.a > 0.0f && this.gameObject.activeSelf)
		{
			i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a - (Time.deltaTime / t));
			yield return null;
		}
	}
}
