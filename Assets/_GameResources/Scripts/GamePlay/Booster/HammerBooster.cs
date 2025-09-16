using DG.Tweening;
using Geckout;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HammerBooster : BoosterBase
{
    [SerializeField] Hammer hammerPrefab;

    protected override int CurrentCount { get => UserDataManager.HammerBooster; set => UserDataManager.HammerBooster = value; }

    public override void Init()
    {
        base.Init();
        CurrentCount = UserDataManager.HammerBooster;
    }

    private void Update()
    {
        GetTile();
    }

    private void GetTile()
    {
        
    }

    public override void CancelBooster()
    {
        base.CancelBooster();
        TouchInputHandler.Instance.CanClick = true;
        IsShowConfirm = false;

    }

    private IEnumerator IEClearGameTIle(GameTile tile)
    {
        ActiveBooster();
        yield return new WaitForEndOfFrame();
        Hammer hammer = Instantiate(hammerPrefab);

        hammer.SmashToBlock(tile.transform.position, 0.35f, () =>
        {

            Done();
            RemoveHammer(hammer);
        });
    }

    private void RemoveHammer(Hammer hammer)
    {
        hammer.transform.DOScale(Vector3.zero, 0.5f).OnComplete(() =>
        {
            Destroy(hammer.gameObject);
        }).SetEase(Ease.InOutBack);
    }

    public override void ActiveBooster()
    {
        base.ActiveBooster();
        IsShowConfirm = false;
        OnStartUseBooster?.Invoke(this, CurrentCount);
    }

    protected override void ShowBooster()
    {
        base.ShowBooster();
        TouchInputHandler.Instance.CanClick = false;
        IsShowConfirm = true;
    }

    protected override void Done()
    {
        base.Done();
       TouchInputHandler.Instance.CanClick = true;
    }
}
