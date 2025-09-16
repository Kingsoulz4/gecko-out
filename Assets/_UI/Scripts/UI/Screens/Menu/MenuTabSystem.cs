using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuTabSystem : MonoBehaviour
{
    [SerializeField] MenuTabButton[] tabButtons;
    [SerializeField] MenuTabPanel[] tabPanels;
    [SerializeField] ScrollScreenHorizontal scrollScreen;
    private int currentIndex;
    public MenuTabPanel menuTabCurrent
    {
        get => tabPanels[currentIndex];
    }
    private void Awake()
    {
        scrollScreen.OnInitialized += Init;
    }
    private void Init()
    {
        for (int i = 0; i < tabPanels.Length; i++)
        {
            tabPanels[i].Initialize();
        }
        for (int i = 0; i < tabButtons.Length; i++)
        {
            tabButtons[i].Initialize(this);
        }
        ScrollScreenReferences.OnChangePanel += Change;
        currentIndex = 1;
        SelectTab(1, true);
    }
    private void OnDestroy()
    {
        ScrollScreenReferences.OnChangePanel -= Change;
    }
    private void Change(int index)
    {
        string log = "";
        switch (index)
        {
            case 0:
                log = "shop";
                break;
            case 1:
                log = "home";
                break;
            case 2:
                log = "clan";
                break;
        }
       
        SelectTab(index, false);
    }
    public void ChangeTab(int id)
    {
        scrollScreen.ChangePanel(id);
    }
    public void SelectTab(int id, bool ignoreAnimation = false)
    {
        if (ignoreAnimation)
        {
            for (int i = 0; i < tabButtons.Length; i++)
            {
                if (tabButtons[i].index == currentIndex)
                {
                    tabButtons[i].Deselect();
                }
                if (tabButtons[i].index == id)
                {
                    tabButtons[i].Select();
                }
            }
            for (int i = 0; i < tabPanels.Length; i++)
            {
                if (tabPanels[i].id == id)
                {
                    tabPanels[i].Active();
                }
                if (tabPanels[i].id == currentIndex)
                {
                    tabPanels[i].Deactive();
                }
            }
            return;
        }
        if (currentIndex == id)
        {
            return;
        }

        for (int i = 0; i < tabButtons.Length; i++)
        {
            if (tabButtons[i].index == currentIndex)
            {
                tabButtons[i].Deselect();
            }
            if (tabButtons[i].index == id)
            {
                tabButtons[i].Select();
            }
        }
        MenuTabPanel old = null;
        MenuTabPanel next = null;
        for (int i = 0; i < tabPanels.Length; i++)
        {
            if (tabPanels[i].id == id)
            {
                next = tabPanels[i];
            }
            if (tabPanels[i].id == currentIndex)
            {
                old = tabPanels[i];
            }
        }
        if (next)
            next.Active();

        if (old)
            old.Deactive();
        currentIndex = id;
    }

    public void GetTransFly(out RectTransform rectGold, out RectTransform rectBooster)
    {
        rectGold = null;
        rectBooster = null;
        MenuTabPanel tabCurrent = null;
        foreach (var tab in tabPanels)
        {
            if (tab.id == currentIndex)
            {
                tabCurrent = tab;
                break;
            }
        }
    }
}
