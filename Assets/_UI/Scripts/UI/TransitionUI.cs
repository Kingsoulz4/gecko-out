using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
public class TransitionUI : MonoBehaviour
{
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] RectTransform backGroundRect;
    [SerializeField] Animator animator;
    [SerializeField] Text text;
    [SerializeField] float rotateSpeed;
    [SerializeField] Color[] colors1;
    [SerializeField] Color[] colors2;
    private int interval;
    private string txss;
    public void Transition(float duration, TweenCallback onLoad, TweenCallback complete)
    {
        onLoad?.Invoke();
        complete?.Invoke();
        return;
    }
    
    void ChangeText()
    {
        string cnt = txss;
        for (int i = 0; i < interval; i++)
        {
            cnt += ".";
        }
        interval++;
        if (interval >= 3)
        {
            interval = 0;
        }
        text.text = cnt;
    }
}