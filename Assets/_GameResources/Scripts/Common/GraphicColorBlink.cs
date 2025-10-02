using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphicColorBlink : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private MaskableGraphic target;

    [Header("Blink Settings")]
    [SerializeField] private Color blinkColor = Color.red;   
    [SerializeField] private float halfDuration = 0.2f;
    [SerializeField] private bool playOnEnable = false;
    [SerializeField] private bool ignoreTimeScale = true;
    [SerializeField] private Color originColor;

    private Tween _loopTween;    

    private void Reset()
    {
        target = GetComponent<MaskableGraphic>();
        originColor = target.color;
    }

    private void OnEnable()
    {
        if (playOnEnable) StartBlink();
    }

    public void StartBlink()
    {
        if (target == null) return;

        StopBlinkImmediate();
        target.color = originColor;

        _loopTween = target
            .DOColor(blinkColor, halfDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetUpdate(ignoreTimeScale);
    }
    
    public void StopBlink(float backToWhiteDuration = 0.12f)
    {
        if (target == null) return;

        if (_loopTween != null && _loopTween.IsActive())
        {
            _loopTween.Kill();
            _loopTween = null;
        }
        
        target.DOColor(originColor, backToWhiteDuration)
              .SetEase(Ease.OutSine)
              .SetUpdate(ignoreTimeScale);
    }
   
    public void StopBlinkImmediate()
    {
        if (_loopTween != null && _loopTween.IsActive())
        {
            _loopTween.Kill();
            _loopTween = null;
        }
        if (target != null) target.color = originColor;
    }

    private void OnDisable()
    {        
        StopBlinkImmediate();
    }
}
