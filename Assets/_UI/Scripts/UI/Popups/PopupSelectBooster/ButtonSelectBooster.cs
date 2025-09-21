using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Geckout
{
    public class ButtonSelectBooster : MonoBehaviour
    {
        [SerializeField] private Button m_button;
        [SerializeField] private GameObject m_lockObject;
        [SerializeField] private GameObject m_selectedObject;
        [SerializeField] private GameObject m_quantityObj;
        [SerializeField] private Text m_textLevelUnlock;
        [SerializeField] private Text m_textQuantity;

        public Action OnClick { get; set; }
        public bool IsSelected { get; set; } = false;
        public bool IsUnLocked => UserDataManager.Level > levelUnlock;

        private int levelUnlock = 1;

        private int quantity = 0;

        private void Awake()
        {
            m_button.onClick.AddListener(OnClickButton);
        }

        public void Init(int levelUnlock, int quantity = 0)
        {
            this.levelUnlock = levelUnlock;
            this.quantity = quantity;
            IsSelected = false;
            m_lockObject.SetActive(!IsUnLocked);
            m_selectedObject.SetActive(false);
            m_textLevelUnlock.text = $"Lv.{levelUnlock}";
            m_textQuantity.text = quantity.ToString();  
        }

        private void OnClickButton()
        {
            if (!IsUnLocked) return;

            OnClick?.Invoke();
            
            IsSelected = quantity > 0? !IsSelected: false;
            m_selectedObject.gameObject.SetActive(IsSelected);
            m_quantityObj.gameObject.SetActive(!IsSelected && IsUnLocked);
        }
    }
}
