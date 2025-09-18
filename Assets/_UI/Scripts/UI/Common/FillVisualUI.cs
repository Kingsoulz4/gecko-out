using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FillVisualUI : MonoBehaviour
{
    [SerializeField] Text txtValue;
    [SerializeField] RectTransform rectFill;
    [SerializeField] float baseValue;

    public void UpdateFill(float value, string valueText)
    {
        txtValue.text = valueText;
        Vector2 size = rectFill.sizeDelta;
        size.x = value * baseValue;
        rectFill.sizeDelta = size;
    }
}