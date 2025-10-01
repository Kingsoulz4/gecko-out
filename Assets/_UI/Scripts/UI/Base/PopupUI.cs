using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
public enum AnimShowPopUp
{
    None,
    Move,
    ScalePunch,
    Fade
}
public abstract class PopupUI : MonoBehaviour
{

    protected UIManager uiManager;
    protected Action onClose;
    protected Action onHide;
    protected Action onShowDone;
    public static event Action<PopupUI> OnDestroyPopup;
    public static event Action<PopupUI> OnHide;
    public static event Action<PopupUI> OnShow;
    public bool isCache = false;
    public bool isCloseAnim = false;
    [SerializeField] protected float duration = 0.2f;
    [SerializeField] private AnimShowPopUp animType;
    [SerializeField] protected RectTransform mainPopUp;
    [SerializeField] protected Image m_background;
    [SerializeField] protected CanvasGroup m_canvasGroup;

    public bool isShowing { get; protected set; }

    private CanvasGroup CanvasGroup
    {
        get
        {
            if(m_canvasGroup == null)
            {
                m_canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            if(m_canvasGroup == null)
            {
                m_canvasGroup = gameObject.GetComponent<CanvasGroup>();
            }

            return m_canvasGroup;
        }
    }    

    public virtual void Initialize(UIManager manager)
    {
        duration = 0.2f;
        this.uiManager = manager;
        gameObject.SetActive(false);
        isShowing = false;

    }
    public virtual void Show(Action onClose)
    {
        this.onClose = onClose;
        isShowing = true;
        CanvasGroup.alpha = 1;
        if (mainPopUp)
        {
            switch (animType)
            {
                case AnimShowPopUp.Move:
                    mainPopUp.anchoredPosition = new Vector2(-2000, mainPopUp.anchoredPosition.y);
                    mainPopUp.DOAnchorPos(new Vector2(0, mainPopUp.anchoredPosition.y), duration).SetEase(Ease.OutQuad);
                    break;
                case AnimShowPopUp.ScalePunch:
                    mainPopUp.localScale = Vector3.zero;
                    mainPopUp.DOScale(1.1f, duration).OnComplete(() =>
                    {
                        mainPopUp.DOScale(1, 0.1f);
                    });
                    break;
            }
            if (m_background != null)
            {
                m_background.DOFade(1, 0.4f);
            }
        }
        gameObject.SetActive(true);
        OnShow?.Invoke(this);
        this.Wait(duration, () =>
        {
            onShowDone?.Invoke();
        });
    }

    public virtual void Hide()
    {
        if (!isShowing)
        {
            return;
        }
        //AudioManager.Instance.PlayOneShot("SFX_ClosePopup", 1f);
        isShowing = false;
        float time = 0;
        if (mainPopUp /*&& isCloseAnim*/)
        {
            switch (animType)
            {
                case AnimShowPopUp.Move:
                    mainPopUp.DOAnchorPos(new Vector2(-2000, mainPopUp.anchoredPosition.y), 0.3f).SetEase(Ease.Linear);
                    time = .32f;
                    break;
                case AnimShowPopUp.ScalePunch:
                    time = .2f;
                    mainPopUp.DOScale(0, time).SetEase(Ease.InBack).SetEase(Ease.OutQuart);
                    CanvasGroup.DOFade(0, time * 0.7f);
                    break;
                case AnimShowPopUp.Fade:
                    time = .3f;
                    CanvasGroup.DOFade(0, time);

                    break;

            }
            if(m_background != null)
            {
                m_background.DOFade(0, 0.4f);
            }
        }

        DOVirtual.DelayedCall(time, () =>
        {
            gameObject.SetActive(false);
            onClose?.Invoke();
            onHide?.Invoke();
            onClose = null;
            OnHide?.Invoke(this);
            if (!isCache)
            {
                OnDestroyPopup?.Invoke(this);
                OnPopupDestroyed();
            }
        });

    }


    protected virtual void OnPopupDestroyed()
    {
        Destroy(gameObject);
    }
    private void OnDisable()
    {
        DOTween.Kill(this);
    }
}