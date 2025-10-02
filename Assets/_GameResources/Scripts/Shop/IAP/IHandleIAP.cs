using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;

namespace Geckout
{
    public interface IHandleIAP 
    {
        void AddProductConsume(string id, string idStoreGoogle, string idStoreApple, UnityAction<bool> callbackPurchase);

        void AddProductNonConsume(string id, string idStoreGoogle, string idStoreApple, UnityAction<bool> callbackPurchase);

        void AddProductSubscription(string id, string idStoreGoogle, string idStoreApple, UnityAction<bool> callbackPurchase);

        void AddProductType(ProductType type, string id, string idStoreGoogle, string idStoreApple, UnityAction<bool> callbackPurchase);

        public float GetLocalizedPrice(string pPackageId);
        public string GetLocalizedPriceString(string pPackageId);

        public void Init();

        public void BuyProductID(string internalProductId, UnityAction<bool> callback = null);

        public void RestorePurchases();
    }
}
