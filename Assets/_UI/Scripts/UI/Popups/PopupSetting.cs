using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupSetting : PopupUI
{
    [SerializeField] Button btn_Close;
    [SerializeField] Button btn_Restore;

    public override void Initialize(UIManager manager)
    {
        base.Initialize(manager);
        btn_Close.onClick.AddListener(Hide);
        btn_Restore.onClick.AddListener(Resrote);
    }

    public void Resrote()
    {
        UIManager.Instance.CheckRestore();
        Hide();
    }
}
