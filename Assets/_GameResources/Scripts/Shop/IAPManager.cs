using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Models;

//public class IAPManager : Singleton<IAPManager>
//{

//    [SerializeField] private ShopDatabase shopDatabase;

//    private StoreController storeController;

//    // Events you might care about
//    public event Action<List<Product>> OnProductsFetched;
//    public event Action<Orders> OnPurchasesFetched;
//    public event Action<FailedOrder> OnPurchaseFailed;
//    public event Action<PendingOrder> OnPurchasePending;
//    public event Action OnStoreConnected;

//    //private Dictionary<string, >

//    private void Start()
//    {
//        InitializeIAP();
//    }

//    public async void InitializeIAP()
//    {
//        // Get StoreController
//        storeController = UnityIAPServices.StoreController();

//        // Hook up event handlers
//        storeController.OnPurchasePending += HandleOnPurchasePending;
//        storeController.OnPurchaseFailed += HandleOnPurchaseFailed;

//        // Connect to store
//        await storeController.Connect();
//        OnStoreConnected?.Invoke();

//        // Define products to fetch from your shop database
//        var productsToFetch = new List<ProductDefinition>();
//        foreach (var pack in shopDatabase.shopPacks)
//        {
//            // assuming consumable; change type if needed
//            productsToFetch.Add(new ProductDefinition(pack.packId, ProductType.));
//        }

//        // Fetch product metadata (prices, titles, etc.)
//        storeController.OnProductsFetched += HandleOnProductsFetched;
//        storeController.FetchProducts(productsToFetch);

//        // In the OnProductsFetched handler, you can then fetch past purchases
//    }

//    private void HandleOnProductsFetched(List<Product> fetchedProducts)
//    {
//        Debug.Log("Products fetched:");
//        foreach (var p in fetchedProducts)
//        {
//            Debug.Log($"- {p.definition.id} : {p.metadata.localizedTitle} price = {p.metadata.localizedPriceString}");
//        }

//        OnProductsFetched?.Invoke(fetchedProducts);

//        // Now fetch previous/pending purchases
//        storeController.OnPurchasesFetched += HandleOnPurchasesFetched;
//        storeController.FetchPurchases();
//    }

//    private void HandleOnPurchasesFetched(Orders orders)
//    {
//        Debug.Log("Purchases fetched. Processing orders...");
//        OnPurchasesFetched?.Invoke(orders);

//        foreach (var order in orders.All)
//        {
//            if (order is CompletedOrder completed)
//            {
//                // Grant the content if not yet granted
//                GrantRewardsForProduct(completed.productId);

//                // Confirm purchase so store will deliver
//                storeController.ConfirmPurchase(completed);
//            }
//            else if (order is PendingOrder pending)
//            {
//                // maybe notify UI that there's a pending purchase
//                // You might wait until it's completed or cancelled
//                Debug.Log($"Pending order: {pending.productId}");
//                // You may optionally confirm after giving content (depending on flow)
//            }
//            // handle deferred or other states if needed
//        }
//    }

//    public void BuyPack(string packId)
//    {
//        if (storeController == null)
//        {
//            Debug.LogError("IAP V5 not connected yet.");
//            return;
//        }

//        storeController.Purchase(packId);
//    }

//    private void HandleOnPurchaseFailed(FailedOrder failed)
//    {
//        Debug.LogError($"Purchase Failed: {failed.Info.PurchasedProductInfo.First().productId}, Reason: {failed.reason}");
//        OnPurchaseFailed?.Invoke(failed);
//    }

//    private void HandleOnPurchasePending(PendingOrder pending)
//    {
//        Debug.Log($"Purchase Pending: {pending.productId}");
//        OnPurchasePending?.Invoke(pending);
//    }

//    private void GrantRewardsForProduct(string packId)
//    {
//        ShopPack pack = shopDatabase.GetPackById(packId);
//        if (pack == null)
//        {
//            Debug.LogError($"GrantRewards: pack not found for id {packId}");
//            return;
//        }

//        foreach (var reward in pack.rewards)
//        {
//            Debug.Log($"Grant reward {reward.quantity} of {reward.itemId}");
//            // TODO: integrate with your inventory / currency / backend
//        }
//    }
//}
