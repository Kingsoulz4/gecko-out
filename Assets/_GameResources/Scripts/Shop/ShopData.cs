using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    [CreateAssetMenu(fileName = "ShopData", menuName = "ScriptableObjects/ShopData", order = 1)]
    public class ShopData : ScriptableObject
    {
        public List<ShopPack> listShopPack = new();
    }

    //[CreateAssetMenu(fileName = "ShopPack", menuName = "ScriptableObjects/ShopPack", order = 1)]
    public class ShopPack/*: ScriptableObject*/
    {
        public string id;
        public string googleID;
        public string appleID;
        public string title;
        public string description;
        public Sprite icon;
        public List<RewardDataGameGecko> listReward;
    }

    [Serializable]
    public abstract class RewardData
    {
        public ItemType type;
        public int quantity;

        public abstract void Claim();
    }

    public enum ItemType
    {
        GOLD,
        INFINITY_LIVES,
        BOOSTER_1,
        BOOSTER_2,
        BOOSTER_3,
        BOOSTER_4,
        BOOSTER_5,
    }

}
