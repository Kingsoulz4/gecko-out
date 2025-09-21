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
        if (IsShowConfirm && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                GameTile tile = hitInfo.collider.GetComponent<GameTile>();
                if (tile != null)
                {
                    if (GameMap.Instance.TilesWall.Contains(tile))
                    {
                        StartCoroutine(DoBooster(tile));
                    }
                }
            }
        }
    }

    public override void CancelBooster()
    {
        base.CancelBooster();
        TouchInputHandler.Instance.CanClick = true;
        IsShowConfirm = false;

        GameMap.Instance.HideHammerIcon();
    }

    public override void ActiveBooster()
    {
        base.ActiveBooster();
        UpdateVisualBooster();
        IsShowConfirm = false;
        OnStartUseBooster?.Invoke(this, CurrentCount);
    }

    protected override void ShowBooster()
    {
        base.ShowBooster();
        TouchInputHandler.Instance.CanClick = false;
        IsShowConfirm = true;

        GameMap.Instance.ShowHammerIcon();
    }

    protected override void Done()
    {
        base.Done();
        TouchInputHandler.Instance.CanClick = true;
        GameMap.Instance.HideHammerIcon();

    }

    private IEnumerator DoBooster(GameTile tile)
    {
        ActiveBooster();

        yield return new WaitForEndOfFrame();

        Hammer hammer = Instantiate(hammerPrefab);
        hammer.SmashToBlock(tile.transform.position, 0.35f, () =>
        {
            RemoveHammer(hammer);
            tile.HideWall();
            Done();
        });
    }

    private void RemoveHammer(Hammer hammer)
    {
        hammer.transform.DOScale(Vector3.zero, 0.5f).OnComplete(() =>
        {
            Destroy(hammer.gameObject);
        }).SetEase(Ease.InOutBack);
    }

}
