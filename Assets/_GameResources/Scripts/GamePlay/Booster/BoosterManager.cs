using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoosterManager : SingletonMono<BoosterManager>
{
    [SerializeField] private BoosterDataSO boosterData;
    [SerializeField] private List<BoosterBase> boosters;
    [SerializeField] private BoosterBase timeBooster;
    [SerializeField] private BoosterBase clearOneBooster;
    [SerializeField] private BoosterBase clearSameBooster;
    public BoosterBase TimeBooster { get => timeBooster; set => timeBooster = value; }
    public BoosterBase ClearOneBooster { get => clearOneBooster; set => clearOneBooster = value; }
    public BoosterBase ClearSameBooster { get => clearSameBooster; set => clearSameBooster = value; }
    public BoosterDataSO BoosterData { get => boosterData; }
    public List<BoosterBase> Boosters { get => boosters; }

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        foreach (var booster in Boosters)
        {
            booster.Init();
        }
    }

    public void UpdateVisualBooster()
    {
        foreach(var booster in Boosters)
        {
            booster.UpdateVisualBooster();
        }
    }

    public BoosterBase GetBoosterByType(BoosterType type)
    {
        return boosters.FirstOrDefault(booster => booster.BoosterType == type);
    }
}
