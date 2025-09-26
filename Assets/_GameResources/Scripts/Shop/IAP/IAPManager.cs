using AYellowpaper.SerializedCollections;
using Dreamteck;
using Geckout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Models;

public enum IAPProvider
{
    UNITY_IAP,
    QON_IAP
}

public class IAPManager : SingletonDontDestroyMono<IAPManager>, IHandleIAP
{
    [SerializeField] private SerializedDictionary<IAPProvider, IAPHandlerBase> listIAPProvider;

    private IHandleIAP IAPHandler
    { 
        get
        {
            return listIAPProvider[IAPProvider.UNITY_IAP];
        } 
    }

    protected override void Awake()
    {
        base.Awake();
    }

    public void AddProductType(ProductType type, string id, string idStoreGoogle, string idStoreApple, UnityAction<bool> callbackPurchase)
    {
        IAPHandler.AddProductType(type, id, idStoreGoogle, idStoreApple, callbackPurchase);
    }

    public void BuyProductID(string internalProductId, UnityAction<bool> callback = null)
    {
        IAPHandler.BuyProductID(internalProductId, callback);
    }

    public void Init()
    {
        IAPHandler.Init();
    }

    public void RestorePurchases()
    {
        IAPHandler.RestorePurchases();
    }

    public void AddProductConsume(string id, string idStoreGoogle, string idStoreApple, UnityAction<bool> callbackPurchase)
    {
        IAPHandler.AddProductConsume(id, idStoreGoogle, idStoreApple, callbackPurchase);
    }

    public void AddProductNonConsume(string id, string idStoreGoogle, string idStoreApple, UnityAction<bool> callbackPurchase)
    {
        IAPHandler.AddProductNonConsume(id, idStoreGoogle, idStoreApple, callbackPurchase);
    }

    public void AddProductSubscription(string id, string idStoreGoogle, string idStoreApple, UnityAction<bool> callbackPurchase)
    {
        IAPHandler.AddProductSubscription(id, idStoreGoogle, idStoreApple, callbackPurchase);
    }
}
