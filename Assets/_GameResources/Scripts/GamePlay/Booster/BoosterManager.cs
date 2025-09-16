using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoosterManager : SingletonMono<BoosterManager>
{
    [SerializeField] private BoosterDataSO boosterData;
    [SerializeField] private List<BoosterBase> boosters;
    [SerializeField] private BoosterBase timeInGameBooster;
    [SerializeField] private BoosterBase timePreBooster;
    [SerializeField] private BoosterBase hammerBooster;
    [SerializeField] private BoosterBase cissorBooster;
    [SerializeField] private BoosterBase handMoveBooster;
    [SerializeField] private BoosterBase suffleBooster;
    public BoosterBase TimeIngameBooster { get => timeInGameBooster; set => timeInGameBooster = value; }
    public BoosterBase TimePreBooster { get => timePreBooster; set => timePreBooster = value; }
    public BoosterBase HammerBooster { get => hammerBooster; set => hammerBooster = value; }
    public BoosterDataSO BoosterData { get => boosterData; }
    public List<BoosterBase> Boosters { get => boosters; }
    public BoosterBase CissorBooster { get => cissorBooster; set => cissorBooster = value; }
    public BoosterBase HandMoveBooster { get => handMoveBooster; set => handMoveBooster = value; }
    public BoosterBase SuffleBooster { get => suffleBooster; set => suffleBooster = value; }

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
