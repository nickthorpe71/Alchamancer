using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class BarScript : MonoBehaviour
{
    [HideInInspector]
	public float fillAmount;

    [SerializeField]
    private float lerpSpeed;

    public Image fill;

    [SerializeField]
    private Text valueText;

    [SerializeField]
    private Color fullColor;
    [SerializeField]
    private Color medColor;
    [SerializeField]
    private Color lowColor;

    public float MaxValue;

    public void SetValue(float value)
    {
        string temp = valueText.text;
        valueText.text = value + "/" + MaxValue;
        fillAmount = Map(value, MaxValue);
        CheckColor();
    }

    void Update()
    {
		HandleBar();
    }

    private void HandleBar()
	{
        if (fillAmount != fill.fillAmount)
        {
            fill.fillAmount = Mathf.Lerp(fill.fillAmount, fillAmount, Time.deltaTime * lerpSpeed);
        }
	}

    private void CheckColor()
    {
        if (fillAmount >= 0.6)
            fill.color = fullColor;
        else if (fillAmount > 0.2 && fillAmount <= 0.6)
            fill.color = medColor;
        else  if (fillAmount <= 0.2)
            fill.color = lowColor;
    }

    public float Map(float value, float inMax)
    {
        return value / inMax;
    }
}
