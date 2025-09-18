using System;
using System.Collections;
using System.Collections.Generic;
using DanielLochner.Assets.SimpleScrollSnap;
using UnityEngine;
using UnityEngine.UI;

public class AutoSwiftSnapScroll : MonoBehaviour
{
    [SerializeField] private SimpleScrollSnap scrollSnap;
    [SerializeField] private List<Image> fillBar;
    [SerializeField] private float delayTime;
    private float timer;
    private Coroutine coroutine;

    private void OnEnable()
    {
        coroutine = StartCoroutine(AutoScroll());
    }

    private IEnumerator AutoScroll()
    {
        timer = -1f;
        ActiveFillBar(scrollSnap.CenteredPanel);
        scrollSnap.OnPanelSelected.AddListener(OnPanelSelected);
        while (true)
        {
            fillBar[scrollSnap.CenteredPanel].fillAmount = timer / delayTime;
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
            if (timer > delayTime)
            {
                scrollSnap.GoToNextPanel();
                timer = 0;
            }
        }
    }

    private void OnPanelSelected(int arg0)
    {
        timer = 0f;
        ActiveFillBar(scrollSnap.CenteredPanel);
    }

    private void ActiveFillBar(int index)
    {
        if (index < 0) index = 0;
        for (int i = 0; i < fillBar.Count; i++)
        {
            if (fillBar[i] == null) continue;
            fillBar[i].gameObject.SetActive(index == i);
        }
    }

    public void Remove(int index)
    {
        var trans = scrollSnap.Pagination.transform;
        var ob = trans.GetChild(index);
        ob.SetParent(null);
        Destroy(ob.gameObject);
        scrollSnap.Remove(index);
        fillBar.RemoveAt(index);
        scrollSnap.Pagination.gameObject.SetActive(trans.childCount > 1);
        enabled = trans.childCount > 1;
        ActiveFillBar(scrollSnap.CenteredPanel);
        if (trans.childCount <= 1 && coroutine != null)
        {
            scrollSnap.UseSwipeGestures = false;
            StopCoroutine(coroutine);
        }
    }
}
