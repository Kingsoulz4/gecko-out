using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;

namespace Geckout
{
    public class IAPHandlerBase : MonoBehaviour, IHandleIAP
    {
        public void AddProductConsume(string id, string idStoreGoogle, string idStoreApple, UnityAction<bool> callbackPurchase)
        {
            AddProductType(ProductType.Consumable, id, idStoreGoogle, idStoreApple, callbackPurchase);
        }

        public void AddProductNonConsume(string id, string idStoreGoogle, string idStoreApple, UnityAction<bool> callbackPurchase)
        {
            AddProductType(ProductType.NonConsumable, id, idStoreGoogle, idStoreApple, callbackPurchase);
        }

        public void AddProductSubscription(string id, string idStoreGoogle, string idStoreApple, UnityAction<bool> callbackPurchase)
        {
            AddProductType(ProductType.Subscription, id, idStoreGoogle, idStoreApple, callbackPurchase);
        }

        public virtual void AddProductType(ProductType type, string id, string idStoreGoogle, string idStoreApple, UnityAction<bool> callbackPurchase)
        {
            
        }

        public virtual void BuyProductID(string internalProductId, UnityAction<bool> callback = null)
        {
            
        }

        public virtual float GetLocalizedPrice(string pPackageId)
        {
            return 0f;
        }

        public virtual string GetLocalizedPriceString(string pPackageId)
        {
            return "$0.00";
        }

        public virtual void Init()
        {
        }

        public virtual void RestorePurchases()
        {
        }

        
    }
}
