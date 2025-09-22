using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BoosterDataSO", menuName = "ScriptableObject/BoosterDataSO", order = 1)]
public class BoosterDataSO : ScriptableObject
{
    public List<BoosterItemData> boosterItemDatas;


    public BoosterItemData GetBoosterItemData(BoosterType boosterType)
    {
        return boosterItemDatas.Find(x => x.boosterType == boosterType);
    }
}


[Serializable]
public class BoosterItemData
{
    public BoosterType boosterType;
    public string title;
    public string description;
    public Sprite icon;
    public Sprite iconBig;
    public int price;
    public int quantity;
    public int levelUnlock;
}