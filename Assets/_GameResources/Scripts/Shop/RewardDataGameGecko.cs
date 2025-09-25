using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    [Serializable]
    public class RewardDataGameGecko : RewardData
    {
        public override void Claim()
        {
            switch (type)
            {
                case ItemType.GOLD:
                    UserDataManager.AddGold(quantity, "Shop");
                    break;
                case ItemType.INFINITY_LIVES:
                    HeartManager.InfinityEndTime += quantity;
                    break;
                case ItemType.BOOSTER_1:
                    UserDataManager.TimePreBooster += quantity;
                    break;
                case ItemType.BOOSTER_2:
                    UserDataManager.CissorBooster += quantity;
                    break;
                case ItemType.BOOSTER_3:
                    UserDataManager.TimeIngameBooster += quantity;
                    break;
                case ItemType.BOOSTER_4:
                    UserDataManager.HammerBooster += quantity;
                    break;
                case ItemType.BOOSTER_5:
                    UserDataManager.HandMoveBooster += quantity;
                    break;
            }
        }
    }
}
