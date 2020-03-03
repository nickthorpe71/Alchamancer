using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

/// <summary>
/// Purchasing script from Unity - Allows communication with App and Play stores
/// </summary>
// Deriving the Purchaser class from IStoreListener enables it to receive messages from Unity Purchasing.
public class Purchaser : MonoBehaviour, IStoreListener
{
    private static IStoreController m_StoreController;          // The Unity Purchasing system.
    private static IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.

    //[HideInInspector] public string ProductDonate1 = "donate0.99";
    [HideInInspector] public string ProductDonateOne = "donate0.99";
    [HideInInspector] public string ProductDonate5 = "donate5";
    [HideInInspector] public string ProductDonate25 = "donate25";
    [HideInInspector] public string ProductDonate100 = "donate100";

    // App Store product identifier subscription product.
    private static string kProductNameAppleSubscription = "com.unity3d.subscription.new";

    // Google Play Store-specific product identifier subscription product.
    private static string kProductNameGooglePlaySubscription = "com.unity3d.subscription.original";

    public static Purchaser instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (m_StoreController == null)
        {
            InitializePurchasing();
        }
    }

    public void InitializePurchasing()
    {
        if (IsInitialized())
        {
            return;
        }

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct(ProductDonateOne, ProductType.Consumable);
        builder.AddProduct(ProductDonate5, ProductType.Consumable);
        builder.AddProduct(ProductDonate25, ProductType.Consumable);
        builder.AddProduct(ProductDonate100, ProductType.Consumable);

        UnityPurchasing.Initialize(this, builder);
    }


    public bool IsInitialized()
    {
        return m_StoreController != null && m_StoreExtensionProvider != null;
    }


    public void Donate1()
    {
        BuyProductID(ProductDonateOne);
    }

    public void Donate5()
    {
        BuyProductID(ProductDonate5);
    }

    public void Donate25()
    {
        BuyProductID(ProductDonate25);
    }

    public void Donate100()
    {
        BuyProductID(ProductDonate100);
    }

    public string GetProductPriceFromStore(string id)
    {
        if (m_StoreController != null && m_StoreController.products != null)
            return m_StoreController.products.WithID(id).metadata.localizedPriceString;
        else
            return "";
    }

    void BuyProductID(string productId)
    {
        // If Purchasing has been initialized ...
        if (IsInitialized())
        {
            // ... look up the Product reference with the general product identifier and the Purchasing 
            // system's products collection.
            Product product = m_StoreController.products.WithID(productId);

            // If the look up found a product for this device's store and that product is ready to be sold ... 
            if (product != null && product.availableToPurchase)
            {
                Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                // ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed 
                // asynchronously.
                m_StoreController.InitiatePurchase(product);
            }
            // Otherwise ...
            else
            {
                // ... report the product look-up failure situation  
                Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
            }
        }
        // Otherwise ...
        else
        {
            // ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or 
            // retrying initiailization.
            Debug.Log("BuyProductID FAIL. Not initialized.");
        }
    }


    // Restore purchases previously made by this customer. Some platforms automatically restore purchases, like Google. 
    // Apple currently requires explicit purchase restoration for IAP, conditionally displaying a password prompt.
    public void RestorePurchases()
    {
        // If Purchasing has not yet been set up ...
        if (!IsInitialized())
        {
            // ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
            Debug.Log("RestorePurchases FAIL. Not initialized.");
            return;
        }

        // If we are running on an Apple device ... 
        if (Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.OSXPlayer)
        {
            // ... begin restoring purchases
            Debug.Log("RestorePurchases started ...");

            // Fetch the Apple store-specific subsystem.
            var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
            // Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
            // the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
            apple.RestoreTransactions((result) => {
                // The first phase of restoration. If no more responses are received on ProcessPurchase then 
                // no purchases are available to be restored.
                Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
            });
        }
        // Otherwise ...
        else
        {
            // We are not running on an Apple device. No work is necessary to restore purchases.
            Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
        }
    }

    //  
    // --- IStoreListener
    //

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        // Purchasing has succeeded initializing. Collect our Purchasing references.
        Debug.Log("OnInitialized: PASS");

        // Overall Purchasing system, configured with products for this application.
        m_StoreController = controller;
        // Store specific subsystem, for accessing device-specific store features.
        m_StoreExtensionProvider = extensions;
    }
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
        Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        if (String.Equals(args.purchasedProduct.definition.id, ProductDonateOne, StringComparison.Ordinal))
        {
            Debug.Log("You Donated $0.99!");
            if (DonateScreen.instance != null)
                DonateScreen.instance.DonateResult(0, 10, 0.99f, 1);
        }
        else if (String.Equals(args.purchasedProduct.definition.id, ProductDonate5, StringComparison.Ordinal))
        {
            Debug.Log("You Donated $4.99!");
            if (DonateScreen.instance != null)
                DonateScreen.instance.DonateResult(1, 60, 4.99f, 1);
        }
        else if (String.Equals(args.purchasedProduct.definition.id, ProductDonate25, StringComparison.Ordinal))
        {
            Debug.Log("You Donated $24.99!");
            if (DonateScreen.instance != null)
                DonateScreen.instance.DonateResult(2, 350, 24.99f, 1);
        }
        else if (String.Equals(args.purchasedProduct.definition.id, ProductDonate100, StringComparison.Ordinal))
        {
            Debug.Log("You Donated $99.99!");
            if (DonateScreen.instance != null)
                DonateScreen.instance.DonateResult(3, 1500, 99.99f, 2);
        }
        else
        {
            Debug.Log(string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'", args.purchasedProduct.definition.id));
        }

        return PurchaseProcessingResult.Complete;
    }


    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
    }
}