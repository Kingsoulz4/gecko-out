using AYellowpaper.SerializedCollections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    [CreateAssetMenu(fileName = "ListItemRewardIcon", menuName = "ScriptableObjects/ListItemRewardIcon", order = 1)]
    public class ListItemRewardIcon : ScriptableObject
    {
        public SerializedDictionary<ItemType, Sprite> listIcon;
    }
}
