using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.Events;
using Spine.Unity;

public class ButtonClickAnimate : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Vector2 startScale = new Vector2(1f, 1f);
    public Vector2 targetScale = new Vector2(0.85f, 0.85f);
    [SerializeField] Transform targetTF;
    [SerializeField] protected UnityEvent eventOnPointDown;
    [SerializeField] protected UnityEvent eventOnPointUp;

    [Header("Audio")]
    [SerializeField] AudioClip soundClick;

    [Header("Anims")]
    [SerializeField] SkeletonGraphic anim;

    private void Awake()
    {
        if(targetTF == null)
        {
            targetTF = transform;
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        targetTF.DOScale(targetScale, 0.1f).SetEase(Ease.Linear).SetUpdate(true).SetId(this);

        if(anim != null)
        {
            anim.AnimationState.SetAnimation(0, "animation", false);
        }

        if (eventOnPointDown != null)
        {
            eventOnPointDown.Invoke();
        }

        if (soundClick != null)
        {
            AudioManager.Instance.PlayOneShot(soundClick, 1f);
        }
        else
        {
            AudioManager.Instance.PlayOneShot("SFX_UI_Button_Click_Open", 1);
        }
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        targetTF.DOScale(startScale, 0.1f).SetEase(Ease.Linear).SetUpdate(true).SetId(this);
        if (eventOnPointUp != null)
        {
            eventOnPointUp.Invoke();
        }
    }
    private void OnDisable()
    {
        this.DOKill();
    }
}

