// QonversionIAPHandler.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;
//using QonversionUnity;
using Geckout;

namespace QOnVersionIAP
{
    public class QonversionIAPManager : IAPHandlerBase
    {
        //private static List<QonversionUnity.Product> _products = new List<QonversionUnity.Product>();
        private Dictionary<string, UnityAction<bool>> productCallbackDict = new();
        private Dictionary<string, UnityAction<bool>> productCallbackRuntimeDict = new();

        private bool _initialized = false;

        public static event Action OnBuyIAPSuccess;

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        public override void Init()
        {
            //if (_initialized) return;

            //QonversionConfig config = new QonversionConfigBuilder(
            //    "US4ZmQfuZpqIzPxLu6hUF7RxibPbmfVA",   // TODO: move to settings
            //    LaunchMode.SubscriptionManagement
            //)
            //.SetEnvironment(QonversionUnity.Environment.Production)
            //.SetEntitlementsCacheLifetime(EntitlementsCacheLifetime.Week)
            //.Build();

            //Qonversion.Initialize(config);

            //Qonversion.GetSharedInstance().Offerings((offerings, error) =>
            //{
            //    if (error == null && offerings?.Main != null)
            //    {
            //        _products = offerings.Main.Products;
            //        Debug.Log($"Qonversion initialized. Products count: {_products.Count}");
            //        _initialized = true;
            //    }
            //    else
            //    {
            //        Debug.LogError($"Qonversion Init error: {error}");
            //    }
            //});
        }

        public override void AddProductType(ProductType type, string id, string idStoreGoogle, string idStoreApple, UnityAction<bool> callbackPurchase)
        {
            if (!productCallbackDict.ContainsKey(id))
                productCallbackDict[id] = callbackPurchase;
            else
                productCallbackDict[id] = callbackPurchase;
        }

        public override void BuyProductID(string internalProductId, UnityAction<bool> callback = null)
        {
            //if (callback != null)
            //    productCallbackRuntimeDict[internalProductId] = callback;
            //else if (productCallbackRuntimeDict.ContainsKey(internalProductId))
            //    productCallbackRuntimeDict.Remove(internalProductId);

            //if (!_initialized)
            //{
            //    Debug.LogError("Qonversion not initialized yet.");
            //    return;
            //}

            //var product = _products.FirstOrDefault(p => p.QonversionId == internalProductId || p.StoreId == internalProductId);
            //if (product == null)
            //{
            //    Debug.LogError($"Qonversion product not found: {internalProductId}");
            //    return;
            //}

            //Debug.Log($"[Qonversion] Buying product: {product.QonversionId}/{product.StoreId}");

            //Qonversion.GetSharedInstance().PurchaseProduct(product, (entitlements, error, cancelled) =>
            //{
            //    if (error == null)
            //    {
            //        HandleSuccess(internalProductId);
            //    }
            //    else
            //    {
            //        HandleFailure(internalProductId, error.ToString());
            //    }
            //});
        }

        public override void RestorePurchases()
        {
//#if UNITY_EDITOR
//            Debug.Log("RestorePurchases simulated in editor.");
//            foreach (var kv in productCallbackDict) kv.Value?.Invoke(true);
//#else
//            Qonversion.GetSharedInstance().Restore((entitlements, error) =>
//            {
//                if (error == null)
//                {
//                    foreach (var entitlement in entitlements.Values)
//                    {
//                        if (entitlement.IsActive)
//                        {
//                            HandleSuccess(entitlement.ProductId);
//                        }
//                    }
//                }
//                else
//                {
//                    Debug.LogError("Qonversion restore error: " + error);
//                }
//            });
//#endif
        }

        private void HandleSuccess(string internalId)
        {
            if (productCallbackDict.TryGetValue(internalId, out var cb) && cb != null)
                cb.Invoke(true);

            if (productCallbackRuntimeDict.TryGetValue(internalId, out cb) && cb != null)
            {
                cb.Invoke(true);
                productCallbackRuntimeDict.Remove(internalId);
            }

            OnBuyIAPSuccess?.Invoke();
            Debug.Log($"[Qonversion] Purchase success for {internalId}");
        }

        private void HandleFailure(string internalId, string reason)
        {
            if (productCallbackDict.TryGetValue(internalId, out var cb) && cb != null)
                cb.Invoke(false);

            if (productCallbackRuntimeDict.TryGetValue(internalId, out cb) && cb != null)
                cb.Invoke(false);

            Debug.LogError($"[Qonversion] Purchase failed for {internalId}: {reason}");
        }
    }
}
