using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Geckout
{
    public class ButtonColorPicked : MonoBehaviour
    {
        [SerializeField] private Image m_image;
        [SerializeField] private Image m_imagePlus;
        [SerializeField] private Button m_button;

        private bool isEmpty = true;

        private void Awake()
        {
            m_button.onClick.AddListener(() =>
            {
                OnClick?.Invoke(transform.GetSiblingIndex());
            });
        }

        public UnityEvent<int> OnClick { get; set; } = new();

        public void SetEmpty(bool isEmpty)
        {
            m_image.gameObject.SetActive(!isEmpty);
            m_imagePlus.gameObject.SetActive(isEmpty);
        }

        public void SetColor(Color color)
        {
            m_image.color = color;
        }

    }
}
