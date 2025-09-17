using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MenuTabPanel : ScrollScreenPanel
{
    [Space, Header("ID Panel")]
    public int id;

    protected RectTransform _rect;

    public abstract void Initialize();
    public abstract void Active();
    public abstract void Deactive();
}