using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Geckout
{
    public class LoadingScreen : ScreenUI
    {
        [SerializeField] private Slider m_sliderProgressLoading;

        public void Show(Action callback)
        {
            m_sliderProgressLoading.DOValue(1, 1f).OnComplete(() =>
            {
                callback?.Invoke();
            });
        }
    }
}
