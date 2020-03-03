using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// Script for behaviour of health bar and exp bar
/// </summary>
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

    /// <summary>
    /// Sets the value of the bar
    /// </summary>
    /// <param name="value"></param>
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

    /// <summary>
    /// Used to trigger the fill of the bar
    /// </summary>
    private void HandleBar()
	{
        if (fillAmount != fill.fillAmount)
        {
            fill.fillAmount = Mathf.Lerp(fill.fillAmount, fillAmount, Time.deltaTime * lerpSpeed);
        }
	}

    /// <summary>
    /// Changes bar color depending what percent it is filled
    /// </summary>
    private void CheckColor()
    {
        if (fillAmount >= 0.6)
            fill.color = fullColor;
        else if (fillAmount > 0.2 && fillAmount <= 0.6)
            fill.color = medColor;
        else  if (fillAmount <= 0.2)
            fill.color = lowColor;
    }

    /// <summary>
    /// Returns the fill amount by dividing value by inMax
    /// </summary>
    /// <param name="value"></param>
    /// <param name="inMax"></param>
    public float Map(float value, float inMax)
    {
        return value / inMax;
    }
}
