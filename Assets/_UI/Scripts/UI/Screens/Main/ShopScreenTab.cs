using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopScreenTab : MenuTabPanel
{
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] GameObject objPackageRemoAds;
    public override void Active()
    {
        ResetVisual();
    }

    public override void Deactive()
    {

    }

    public override void Initialize()
    {

    }

    private void OnEnable()
    {
        UIManager.OnRefeshBannerAndAds += ResetVisual;
    }

    private void ResetVisual()
    {
    }

    private void OnDisable()
    {
        UIManager.OnRefeshBannerAndAds -= ResetVisual;
    }

    public void RollToShopGold()
    {
        Vector3 currentPos = scrollRect.content.localPosition;
        scrollRect.content.localPosition = new Vector3(currentPos.x, 0, currentPos.z);
        Vector3 targetPos = new Vector3(currentPos.x, 600, currentPos.z);
        scrollRect.content.DOLocalMove(targetPos, 0.5f).SetEase(Ease.OutQuad);
        //scrollRect.content.localPosition = new Vector3(scrollRect.content.localPosition.x, 1000, 0);
    }
}
