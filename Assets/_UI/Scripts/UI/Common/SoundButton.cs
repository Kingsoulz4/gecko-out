using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;

public class SoundButton : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] AudioClip soundClick;

    public void OnPointerDown(PointerEventData eventData)
    {
        if(soundClick != null)
        {
            AudioManager.Instance.PlayOneShot(soundClick, 1f);
        }
        else
        {
            AudioManager.Instance.PlayOneShot("SFX_UI_Button_Click_Open", 1);
        }
    }
}
