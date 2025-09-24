using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class Hammer : MonoBehaviour
{
    [SerializeField] Animator animator;
    //[SerializeField] ParticelHammer efBreak;
    [SerializeField] AudioClip smashClip;
    Action OnDone;
    Vector3 positionTarget;
    Tween tweenMove;
    public void MoveTo(Vector3 destination, float time, Action onComplete)
    {
        if (tweenMove != null)
        {
            tweenMove.Kill();
        }
        tweenMove = transform.DOMove(destination, time).OnComplete(() => onComplete?.Invoke());
    }

    public void SmashToBlock(Vector3 position, float time, Action OnDone)
    {
        this.OnDone = OnDone;
        positionTarget = position;
        positionTarget.z -= 1f;
        animator.Play("Idle");
        MoveTo(positionTarget, time, () =>
        {
            animator.Play("Smash");
        });
    }

    public void SmashDone()// Animation event
    {
        //LevelManager.Instance.CameraController.ShakeCamera();
        if (smashClip)
        {
            AudioManager.Instance.PlayOneShot(smashClip, 1);
        }
        //ParticelHammer ef2 = Instantiate(efBreak);
        //ef2.transform.position = positionTarget;
        //ef2.transform.rotation = Quaternion.Euler(180, 0, 0);
        //ef2.SetColor(color);
        OnDone?.Invoke();
    }

    public void Destroy()// Animation event
    {
        tweenMove.Kill();
    }
}
