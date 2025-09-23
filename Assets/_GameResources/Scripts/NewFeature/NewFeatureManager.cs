using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NewFeatureManager : SingletonMono<NewFeatureManager>
{
    [SerializeField] private NewFeatureSO newFeatureSO;
    public NewFeatureSO NewFeatureSO { get => newFeatureSO; }
    private Dictionary<int, NewFeatureItemData> featurePopupDataDic = new Dictionary<int, NewFeatureItemData>();

    public Dictionary<int, NewFeatureItemData> FeaturePopupDataDic { get => featurePopupDataDic; set => featurePopupDataDic = value; }

    protected void Awake()
    {
        for (int i = 0; i < NewFeatureSO.newFeatureItemDatas.Count; i++)
        {
            FeaturePopupDataDic.Add(NewFeatureSO.newFeatureItemDatas[i].level, NewFeatureSO.newFeatureItemDatas[i]);
        }
    }

    public NewFeatureItemData GetNewFeatureInProgress()
    {
        return FeaturePopupDataDic.FirstOrDefault(x => x.Key >= UserDataManager.Level).Value;
    }


        
}
