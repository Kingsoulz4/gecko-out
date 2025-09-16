using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThemeChooseBooster : MonoBehaviour
{
    [SerializeField] Image img_IconBooster;
    [SerializeField] Text txt_Des;

    public void SetUpVisual(BoosterItemData data)
    {
        img_IconBooster.sprite = data.icon;
        txt_Des.text = data.description;
    }
}