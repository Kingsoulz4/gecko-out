using DG.Tweening;
using Geckout;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimePreBooster : BoosterBase
{
    public float bonusTime = 15;
    public float timePlayAnim = 2;
    [SerializeField] private GameObject m_clockPrefab;
    [SerializeField] private Transform m_spawnPoint;
    [SerializeField] private Transform m_targetPoint;

    protected override int CurrentCount { get => UserDataManager.TimePreBooster; set => UserDataManager.TimePreBooster = value; }

    public bool IsSelectedToUse { get; set; }

    private float currentTime;

    public override void Init()
    {
        base.Init();
        CurrentCount = UserDataManager.TimePreBooster;
    }
    protected override void ShowBooster()
    {
        //base.ShowBooster();
    }

    private void Update()
    {
        if (!InProgress || GameManager.GameState == GameState.Paused)
        {
            return;
        }
        
    }

    public override void ActiveBooster()
    {
        if (!IsSelectedToUse) return;

        IsSelectedToUse = true;

        base.ActiveBooster();
        UserDataManager.TimePreBooster = CurrentCount;

        OnStartUseBooster?.Invoke(this, CurrentCount);

        //DOVirtual.DelayedCall(timePlayAnim, Done);
        StartCoroutine(IEAnimateBooster());

    }

    private IEnumerator IEAnimateBooster()
    {
        var scissorObject = Instantiate(m_clockPrefab, transform);
        scissorObject.transform.localScale = Vector3.zero;
        scissorObject.transform.position = m_spawnPoint.position;
        scissorObject.transform.DOScale(1, 0.5f);
        scissorObject.transform.DOLocalRotate(scissorObject.transform.localRotation.eulerAngles + Vector3.up * 360, 0.5f, RotateMode.FastBeyond360);
        yield return new WaitForSeconds(0.6f);
        //scissorObject.transform.DOMove(body.Segments.Last().transform.position, 0.5f);

        
        var target = m_targetPoint.position;
        target = new Vector3(target.x, target.y, m_spawnPoint.position.z);
        float flightDuration = 0.5f;

        Vector3 startPos = scissorObject.transform.position;
        Vector3 endPos = target;
        Transform controlPointA = null;
        Transform controlPointB = null;
        //Vector3 cpA = controlPointA ? controlPointA.position : (startPos + (endPos - startPos) * 0.33f + new Vector3(3, 2, 0));
        Vector3 cpB = controlPointB ? controlPointB.position : (startPos + (endPos - startPos) * 0.66f + new Vector3(-3, 2, 0));

        Vector3 cpA = controlPointA ? controlPointA.position : (startPos + (endPos - startPos) * 0.33f + new Vector3(-3, 2, 0));


        // path goes through control points -> natural XY curve
        Vector3[] path = new Vector3[] { startPos, cpA, endPos };

        Tween flightTween = scissorObject.transform
            .DOPath(path, flightDuration, PathType.CatmullRom, PathMode.Full3D, 10, Color.green)
            .SetEase(Ease.InOutSine);
        //.SetLookAt(0.01f); // rotate toward movement
        flightTween.OnUpdate(() =>
            {
                // direction = next position - current position
                Vector3 dir = flightTween.PathGetPoint(flightTween.ElapsedPercentage() + 0.01f) - transform.position;

                if (dir.sqrMagnitude > 0.001f)
                {
                    // if plane forward is +X, use Vector3.right
                    Quaternion lookRot = Quaternion.LookRotation(Vector3.forward, dir);
                    transform.rotation = lookRot;
                }
            });

        yield return new WaitForSeconds(flightDuration * 0.9f);
        Destroy(scissorObject.gameObject);
        Done();
    }


    protected override void Done()
    {
        base.Done();
        currentTime = 0;

    }
}
