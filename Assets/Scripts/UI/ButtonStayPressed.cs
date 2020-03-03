using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Used to make a button stay predded until "pressed" bool is reset
/// </summary>
public class ButtonStayPressed : MonoBehaviour
{
	private bool pressed = false;

	public void StayPressed()
	{
		if (!pressed)
		{
			this.gameObject.GetComponent<Image>().color = Color.gray;
			pressed = true;
		}
		else
		{
			this.gameObject.GetComponent<Image>().color = Color.white;
			pressed = false;
		}
	}
}
