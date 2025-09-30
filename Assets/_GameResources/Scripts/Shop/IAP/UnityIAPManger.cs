// UnityIAPManger.cs
using Geckout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Models;
using UnityEngine.Purchasing.Security; // keep if you use validation later

public class UnityIAPManger : IAPHandlerBase
{
    private StoreController _storeController;

    // Keep your callback dictionaries (internal ID -> callback)
    private Dictionary<string, UnityAction<bool>> productCallbackDict = new Dictionary<string, UnityAction<bool>>();
    private Dictionary<string, UnityAction<bool>> productCallbackRuntimeDict = new Dictionary<string, UnityAction<bool>>();

    // local struct to hold the product definitions you were building previously
    private class ProductMeta
    {
        public string internalId;
        public string googleStoreId;
        public string appleStoreId;
        public ProductType productType;
    }
    private readonly List<ProductMeta> _metaList = new List<ProductMeta>();

    private async void Start()
    {
        // Optionally call Init() from elsewhere instead of auto-start
        await InitializeAsync();
    }

    #region Public API

    public override void AddProductType(ProductType type, string id, string idStoreGoogle, string idStoreApple, UnityAction<bool> callbackPurchase)
    {
        _metaList.Add(new ProductMeta
        {
            internalId = id,
            productType = type,
            googleStoreId = idStoreGoogle,
            appleStoreId = idStoreApple
        });

        if (!productCallbackDict.ContainsKey(id))
            productCallbackDict.Add(id, callbackPurchase);
        else
            productCallbackDict[id] = callbackPurchase;
    }

    public override void Init()
    {
        _ = InitializeAsync();
    }

    public override void BuyProductID(string internalProductId, UnityAction<bool> callback = null)
    {
        if (callback != null)
            productCallbackRuntimeDict[internalProductId] = callback;
        else if (productCallbackRuntimeDict.ContainsKey(internalProductId))
            productCallbackRuntimeDict.Remove(internalProductId);

        if (_storeController == null)
        {
            Debug.LogError("IAP not connected yet. Call Init() and wait for connection.");
            return;
        }

        var meta = _metaList.Find(m => m.internalId == internalProductId);
        if (meta == null)
        {
            Debug.LogError($"BuyProductID: no product meta for internal id {internalProductId}");
            return;
        }

        try
        {
            // purchase by internalId, StoreSpecificIds will map it
            _storeController.PurchaseProduct(internalProductId);
            Debug.Log($"Purchasing product: {internalProductId}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Purchase error: {ex.Message}");
        }
    }

    public override void RestorePurchases()
    {
        if (_storeController == null)
        {
            Debug.LogWarning("RestorePurchases called but IAP not initialized.");
            return;
        }

        _storeController.RestoreTransactions(OnRestoredPurchase);
    }

    private void OnRestoredPurchase(bool arg1, string arg2)
    {
        if(arg1)
        {

        }
    }
    #endregion

    #region Initialization / fetch / events
    private async Task InitializeAsync()
    {
        if (_storeController != null)
            return;

        _storeController = UnityEngine.Purchasing.UnityIAPServices.StoreController();

        _storeController.OnProductsFetched += OnProductsFetched;
        _storeController.OnPurchasesFetched += OnPurchasesFetched;
        _storeController.OnPurchaseFailed += OnPurchaseFailed;
        _storeController.OnPurchasePending += OnPurchasePending;
        _storeController.OnPurchaseConfirmed += OnPurchaseConfirmed;

        try
        {
            await _storeController.Connect();
            Debug.Log("IAP v5 connected to store.");

            var productDefinitions = new List<ProductDefinition>();
            var catalogProvider = new CatalogProvider();
            foreach (var meta in _metaList)
            {
                var storeIds = new StoreSpecificIds();
                if (!string.IsNullOrEmpty(meta.googleStoreId))
                    storeIds.Add(GooglePlay.Name, meta.googleStoreId);
                if (!string.IsNullOrEmpty(meta.appleStoreId))
                    storeIds.Add(AppleAppStore.Name, meta.appleStoreId);

                //var pd = new ProductDefinition( );
                catalogProvider.AddProduct(meta.internalId, meta.productType, storeIds);
                
                //productDefinitions.Add(catalogProvider);
            }

            if (productDefinitions.Count > 0)
            {
                //_storeController.FetchProducts(productDefinitions);
                
            }

            catalogProvider.FetchProducts((pds) => {
                _storeController.FetchProducts(pds);
            });
        }
        catch (Exception ex)
        {
            Debug.LogError($"IAP Connect error: {ex.Message}");
        }
    }

    private void OnPurchaseConfirmed(Order order)
    {
        Debug.Log("Purchase Confirmed");
        _storeController.FetchPurchases();
    }

    private void OnProductsFetched(List<Product> products)
    {
        Debug.Log($"OnProductsFetched: {products?.Count ?? 0} products.");
        foreach (var p in products)
        {
            Debug.Log($"Product: {p.definition.id} | title = {p.metadata?.localizedTitle} | price = {p.metadata?.localizedPriceString}");
        }

        _storeController.FetchPurchases();
    }

    private void OnPurchasesFetched(Orders orders)
    {
        Debug.Log("OnPurchasesFetched: processing orders...");
        if (orders == null) return;

        _storeController.ProcessPendingOrdersOnPurchasesFetched(true);

        foreach (var order in orders.PendingOrders)
        {
            if (order is PendingOrder pending)
            {
                Debug.Log($"Pending order found: {pending.Info.PurchasedProductInfo.First().productId} - You should handle UI for pending purchases.");
            }
        }

        foreach (var order in orders.ConfirmedOrders)
        {
            if (order is ConfirmedOrder completed)
            {
                GrantRewardsForStoreId(completed.Info.PurchasedProductInfo.First().productId);
            }
        }
    }

    private void OnPurchasePending(PendingOrder pending)
    {
        Debug.Log($"OnPurchasePending: {pending.Info.TransactionID}");
        _storeController.ConfirmPurchase(pending);
    }

    private void OnPurchaseFailed(FailedOrder failed)
    {
        var productId = failed.Info.PurchasedProductInfo.First().productId;
        Debug.LogError($"OnPurchaseFailed: productId={productId} reason={failed.FailureReason}");

        var meta = _metaList.Find(m => m.internalId == productId);
        if (meta != null)
        {
            if (productCallbackDict.TryGetValue(meta.internalId, out var cb) && cb != null) cb.Invoke(false);
            if (productCallbackRuntimeDict.TryGetValue(meta.internalId, out cb) && cb != null) cb.Invoke(false);
        }
    }
    #endregion

    #region Helpers: grant rewards
    private void GrantRewardsForStoreId(string storeId)
    {
        var meta = _metaList.Find(m => m.internalId == storeId);
        if (meta == null)
        {
            Debug.LogWarning($"GrantRewards: no meta found for storeId {storeId}");
            return;
        }

        try
        {
            if (productCallbackDict.TryGetValue(meta.internalId, out var cb) && cb != null)
                cb.Invoke(true);

            if (productCallbackRuntimeDict.TryGetValue(meta.internalId, out cb) && cb != null)
            {
                cb.Invoke(true);
                productCallbackRuntimeDict.Remove(meta.internalId);
            }

            PushEventIAP_ForStoreId(storeId);
        }
        catch (KeyNotFoundException) { }
    }
    #endregion

    #region Stubs for old helpers
    public void PushEventIAP(PurchaseEventArgs e)
    {
        Debug.Log($"PushEventIAP (deprecated stub) - received storeId from legacy args.");
    }

    private void PushEventIAP_ForStoreId(string storeId)
    {
        Debug.Log($"PushEventIAP_ForStoreId: {storeId}");
    }
    #endregion
}
