using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.Events;

public class ButtonScaler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Vector2 startScale = new Vector2(1f, 1f);
    public Vector2 endScale = new Vector2(0.95f, 0.95f);
    [SerializeField] Transform targetTF;
    [SerializeField] protected UnityEvent eventOnPointDown;
    [SerializeField] protected UnityEvent eventOnPointUp;
    private void Awake()
    {
        if(targetTF == null)
        {
            targetTF = transform;
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        targetTF.DOScale(endScale, 0.1f).SetEase(Ease.Linear).SetUpdate(true).SetId(this);
        if (eventOnPointDown != null)
        {
            eventOnPointDown.Invoke();
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

