using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewFeatureManager : SingletonMono<NewFeatureManager>
{
    [SerializeField] private NewFeatureSO newFeatureSO;

    public NewFeatureSO NewFeatureSO { get => newFeatureSO; }
}
