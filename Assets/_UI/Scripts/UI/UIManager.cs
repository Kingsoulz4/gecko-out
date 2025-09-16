using DG.Tweening;
using Geckout;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class UIManager : SingletonMono<UIManager>
{
    public Camera UICamera;
    public Canvas canvas;
    public GameObject UIBlock;
    public NotifyPanel notifyPanel;
    public TransitionUI transitionUI;
    public RectTransform screenHolder;
    public RectTransform popupHolder;
    public Transform transSpawnItemFly;
    public CanvasScaler[] canvasScaler;
    [SerializeField] List<PopupUI> listPopupCached;
    [SerializeField] List<PopupUI> listPopupExist;
    [SerializeField] List<ScreenUI> listScreenCached;
    [SerializeField] List<ScreenUI> listScreenExist;
    public ScreenUI CurrentScreen { get; private set; }
    public static event Action<Vector2> OnChangeScreen;
    public static event Action OnRefeshBannerAndAds;
    public Vector2 currentScreenSize;
    public static bool isFold { get; private set; }
    private bool ready;

    [EditorButton]
    private void SetRef()
    {
        UIManager u = GetComponent<UIManager>();
        u.canvasScaler = u.GetComponentsInChildren<CanvasScaler>(true);
        // u.popups = FindObjectsOfType<PopupUI>(true);
        PopupUI[] popups = Resources.LoadAll<PopupUI>("UI/Popups/");
        for (int i = 0; i < popups.Length; i++)
        {
            string nname = popups[i].GetType().Name;
            Debug.Log(nname);
            popups[i].gameObject.name = nname;
            string pa = AssetDatabase.GetAssetPath(popups[i].gameObject);
            AssetDatabase.RenameAsset(pa, nname);
            AssetDatabase.SaveAssetIfDirty(popups[i].gameObject);
        }
        ScreenUI[] screens = Resources.LoadAll<ScreenUI>("UI/Screens/");
        for (int i = 0; i < screens.Length; i++)
        {
            string nname = screens[i].GetType().Name;
            Debug.Log(nname);
            screens[i].gameObject.name = nname;
            string pa = AssetDatabase.GetAssetPath(screens[i].gameObject);
            AssetDatabase.RenameAsset(pa, nname);
            AssetDatabase.SaveAssetIfDirty(screens[i].gameObject);
        }
        EditorUtility.SetDirty(u);
    }

    private void Start()
    {
        MainScreenUI mainScreenUI = UIManager.Instance.ShowScreen<MainScreenUI>();
    }
    public Vector2 GetCanvasSize()
    {
        return canvas.GetComponent<RectTransform>().sizeDelta;
    }
    public virtual void Initialize()
    {
        listPopupCached = popupHolder.GetComponentsInChildren<PopupUI>(true).ToList();
        listScreenCached = screenHolder.GetComponentsInChildren<ScreenUI>(true).ToList();
        isFold = false;
        for (int i = 0; i < canvasScaler.Length; i++)
        {
            canvasScaler[i].matchWidthOrHeight = 0;
            if (MyUtil.IsiPad())
            {
                canvasScaler[i].matchWidthOrHeight = 1f;
            }
#if UNITY_EDITOR
            else
            {
                float w = Screen.width;
                float h = Screen.height;
                float aspect = w / h;
                if (aspect > 1.6f)
                {
                    canvasScaler[i].matchWidthOrHeight = 0.8f;
                }
            }
#endif

        }
        for (int i = 0; i < listScreenCached.Count; i++)
        {
            listScreenCached[i].Initialize(this);
        }
        for (int i = 0; i < listPopupCached.Count; i++)
        {
            listPopupCached[i].Initialize(this);
        }
        listPopupExist = new List<PopupUI>(listPopupCached);
        listScreenExist = new List<ScreenUI>(listScreenCached);
        UIBlock.SetActive(false);

        PopupUI.OnDestroyPopup += OnPopupDestroyed;
        ScreenUI.OnDestroyScreen += OnDestroyScreen;
        currentScreenSize = new Vector2(Screen.width, Screen.height);
        if (currentScreenSize.x / currentScreenSize.y < 1f && currentScreenSize.x / currentScreenSize.y > 0.8f)
        {
            isFold = true;
        }
        ready = true;
        //GameManager.OnPause += Pause;
        //GameManager.OnResume += Resume;
    }
    public void NotifyContent(string content, string key = "", float number = 0)
    {
        notifyPanel.ShowNotify(content, key, number);
    }
    private void Pause()
    {
        // blockerUI.SetActive(true);
    }
    private void Resume()
    {
        // blockerUI.SetActive(false);
    }
    private void Update()
    {
        if (!ready) return;
        if (Screen.width != currentScreenSize.x && Screen.height != currentScreenSize.y)
        {
            currentScreenSize.x = Screen.width;
            currentScreenSize.y = Screen.height;
            for (int i = 0; i < canvasScaler.Length; i++)
            {
                canvasScaler[i].matchWidthOrHeight = 0;
                if (MyUtil.IsiPad())
                {
                    canvasScaler[i].matchWidthOrHeight = 1f;
                }
            }
            if (currentScreenSize.x / currentScreenSize.y < 1f && currentScreenSize.x / currentScreenSize.y > 0.8f)
            {
                isFold = true;
            }
            else
            {
                isFold = false;
            }
            for (int i = 0; i < canvasScaler.Length; i++)
            {
                canvasScaler[i].matchWidthOrHeight = 0;
                if (MyUtil.IsiPad())
                {
                    canvasScaler[i].matchWidthOrHeight = 1f;
                }
#if UNITY_EDITOR
                else
                {
                    float w = Screen.width;
                    float h = Screen.height;
                    float aspect = w / h;
                    if (aspect > 1.6f)
                    {
                        canvasScaler[i].matchWidthOrHeight = 0.8f;
                    }
                }
#endif
            }
            OnChangeScreen?.Invoke(currentScreenSize);
        }
    }
    private void OnPopupDestroyed(PopupUI obj)
    {
        if (listPopupCached.Contains(obj))
        {
            listPopupCached.Remove(obj);
        }
        listPopupExist.Remove(obj);
        GameObject.Destroy(obj.gameObject);
    }
    private void OnDestroyScreen(ScreenUI screen)
    {
        if (listScreenCached.Contains(screen))
        {
            listScreenCached.Remove(screen);
        }
        if (listScreenExist.Contains(screen))
        {
            listScreenExist.Remove(screen);
        }
        GameObject.Destroy(screen.gameObject);
    }
    public T ShowScreen<T>() where T : ScreenUI
    {
        if (CurrentScreen)
        {
            CurrentScreen.Deactive();
        }
        for (int i = 0; i < listScreenCached.Count; i++)
        {
            if (listScreenCached[i] is T)
            {
                CurrentScreen = listScreenCached[i];
                listScreenCached[i].Active();
                listScreenCached[i].transform.SetAsLastSibling();
                return listScreenCached[i].GetComponent<T>();
            }
        }
        T screen = CreateScreen<T>();
        CurrentScreen = screen;
        screen.Active();
        screen.transform.SetAsLastSibling();
        return screen;
    }
    private T CreateScreen<T>() where T : ScreenUI
    {
        string screenName = typeof(T).Name;
        T screen = Instantiate(Resources.Load<T>("UI/Screens/" + screenName), screenHolder);
        listScreenExist.Add(screen);
        if (screen.isCache)
        {
            listScreenCached.Add(screen);
        }
        screen.Initialize(this);
        return screen;
    }
    public T GetScreen<T>() where T : ScreenUI
    {
        T screen = default;
        for (int i = 0; i < listScreenExist.Count; i++)
        {
            if (listScreenExist[i] is T)
            {
                screen = listScreenExist[i].GetComponent<T>();
                return screen;
            }
        }
        screen = CreateScreen<T>();
        return screen;
    }
    public T GetScreenActive<T>() where T : ScreenUI
    {
        T screen = default;
        for (int i = 0; i < listScreenExist.Count; i++)
        {
            if (listScreenExist[i] is T)
            {
                screen = listScreenExist[i].GetComponent<T>();
                return screen;
            }
        }
        return screen;
    }
    public T ShowPopup<T>(System.Action onClose) where T : PopupUI
    {
        for (int i = 0; i < listPopupCached.Count; i++)
        {
            if (listPopupCached[i] is T)
            {
                listPopupCached[i].Show(onClose);
                listPopupCached[i].transform.SetAsLastSibling();
                return listPopupCached[i].GetComponent<T>();
            }
        }
        T popup = CreatePopup<T>();
        popup.Show(onClose);
        popup.transform.SetAsLastSibling();
        return popup;
    }
    public void CloseAllPopup()
    {
        for (int i = 0; i < listPopupCached.Count; i++)
        {
            listPopupCached[i].Hide();
        }
        for (int i = 0; i < listPopupExist.Count; i++)
        {
            listPopupExist[i].Hide();
        }
    }
    private T CreatePopup<T>() where T : PopupUI
    {
        string popupName = typeof(T).Name;
        T popup = Instantiate(Resources.Load<T>("UI/Popups/" + popupName), popupHolder);
        listPopupExist.Add(popup);
        if (popup.isCache)
        {
            listPopupCached.Add(popup);
        }
        popup.Initialize(this);
        return popup;
    }
    public T GetPopup<T>() where T : PopupUI
    {
        T popup = default;
        for (int i = 0; i < listPopupCached.Count; i++)
        {
            if (listPopupCached[i] is T)
            {
                popup = listPopupCached[i].GetComponent<T>();
                return popup;
            }
        }
        popup = CreatePopup<T>();
        return popup;
    }
    public T GetPopupActive<T>() where T : PopupUI
    {
        T popup = default;
        for (int i = 0; i < listPopupCached.Count; i++)
        {
            if (listPopupCached[i] is T)
            {
                popup = listPopupCached[i].GetComponent<T>();
                return popup;
            }
        }
        return popup;
    }
    public bool HasPopupShowing()
    {
        foreach (var item in listPopupExist)
        {
            if (item.isShowing) return true;
        }
        return false;
    }
    public void OnLoading(Action OnLoading = null, float timeDelay = 0f)
    {
        //uiLoading.Loading(OnLoading, timeDelay);
    }

    public static string FormatString(object value)
    {
        return value.ToString();
    }
    public void Transition(float time, TweenCallback onload, TweenCallback complete)
    {
        transitionUI.Transition(time, onload, complete);
    }

    private Coroutine checkRestore;
    public void CheckRestore()
    {
        if (checkRestore != null)
        {
            StopCoroutine(checkRestore);
        }
        checkRestore = StartCoroutine(ILoopCheck());

        IEnumerator ILoopCheck()
        {
            int count = 0;
            while (count < 100)
            {
                yield return new WaitForSeconds(0.1f);
                count++;
                ResetBanner();
            }
        }
    }

    public void ResetBanner()
    {
    }
}
public static class CanvasPositioningExtensions
{
    public static Vector3 WorldToCanvasPosition(this Canvas canvas, Vector3 worldPosition, Camera camera = null)
    {
        if (camera == null)
        {
            camera = Camera.main;
        }
        var viewportPosition = camera.WorldToViewportPoint(worldPosition);
        return canvas.ViewportToCanvasPosition(viewportPosition);
    }

    public static Vector3 ScreenToCanvasPosition(this Canvas canvas, Vector3 screenPosition)
    {
        var viewportPosition = new Vector3(screenPosition.x / Screen.width, screenPosition.y / Screen.height, 0);
        return canvas.ViewportToCanvasPosition(viewportPosition);
    }

    public static Vector3 ViewportToCanvasPosition(this Canvas canvas, Vector3 viewportPosition)
    {
        var centerBasedViewPortPosition = viewportPosition - new Vector3(0.5f, 0.5f, 0);
        var canvasRect = canvas.GetComponent<RectTransform>();
        var scale = canvasRect.sizeDelta;
        return Vector3.Scale(centerBasedViewPortPosition, scale);
    }

}
