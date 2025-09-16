using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ScrollScreenReferences : MonoBehaviour
{
    public static Action<int> OnChangePanel;

    [Serializable]
    public struct GroupScrollScreenComponent
    {
        public ScrollScreenPanel Panel;
        public ScrollScreenButton Button;
    }
    public GroupScrollScreenComponent[] component;
    public RectTransform ImageSlider;
    public float DurationAutoScroll = 0.5f;
    public float DeltaMaxToNextPanel = 200;
    public float VelocityMaxToNextPanel = 1000;
    [Range(1f, 3f)] public float ButtonScaleUpPercent = 2f;
    public int StartIndex = 0;
}
