using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewFeatureManager : Singleton<NewFeatureManager>
{
    [SerializeField] private NewFeatureSO newFeatureSO;

    public NewFeatureSO NewFeatureSO { get => newFeatureSO;}
}
