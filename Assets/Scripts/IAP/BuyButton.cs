﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script to be put on donation buttons
/// </summary>
public class BuyButton : MonoBehaviour
{
    /// <summary>
    /// Determines the value of donation that this button is set to
    /// </summary>
    public enum ItemType
    {
        Donate1,
        Donate5,
        Donate25,
        Donate100
    }

    public ItemType itemType;

    public Text priceText;

    private string defaultText;

    void Start()
    {
        StartCoroutine(LookAtPriceRoutine());
    }

    /// <summary>
    /// The function called when someone clicks one of the donate buttons
    /// </summary>
    public void ClickBuy()
    {
        switch (itemType)
        {
            case ItemType.Donate1:
                Purchaser.instance.Donate1();
                break;
            case ItemType.Donate5:
                Purchaser.instance.Donate5();
                break;
            case ItemType.Donate25:
                Purchaser.instance.Donate25();
                break;
            case ItemType.Donate100:
                Purchaser.instance.Donate100();
                break;
        }
    }

    /// <summary>
    /// Checks the price in the store in the app store on use users device and pulls down the price in the correct currency
    /// </summary>
    private IEnumerator LookAtPriceRoutine()
    {
        while (!Purchaser.instance.IsInitialized())
            yield return null;

        string loaderPrice = "";

        switch (itemType)
        {
            case ItemType.Donate1:
                loaderPrice = Purchaser.instance.GetProductPriceFromStore(Purchaser.instance.ProductDonateOne);
                break;
            case ItemType.Donate5:
                loaderPrice = Purchaser.instance.GetProductPriceFromStore(Purchaser.instance.ProductDonate5);
                break;
            case ItemType.Donate25:
                loaderPrice = Purchaser.instance.GetProductPriceFromStore(Purchaser.instance.ProductDonate25);
                break;
            case ItemType.Donate100:
                loaderPrice = Purchaser.instance.GetProductPriceFromStore(Purchaser.instance.ProductDonate100);
                break;
        }

        priceText.text = defaultText + " " + loaderPrice;
    }

}
