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
        [SerializeField] private Text m_textLevelUnlock;

        public Action OnClick { get; set; }
        public bool IsSelected { get; set; } = false;
        public bool IsUnLocked => UserDataManager.Level > levelUnlock;

        private int levelUnlock = 1;

        private void Awake()
        {
            m_button.onClick.AddListener(OnClickButton);
        }

        public void Init(int levelUnlock)
        {
            this.levelUnlock = levelUnlock;
            IsSelected = false;
            m_lockObject.SetActive(!IsUnLocked);
            m_selectedObject.SetActive(false);
            m_textLevelUnlock.text = $"Lv.{levelUnlock}";
        }

        private void OnClickButton()
        {
            if (!IsUnLocked) return;

            OnClick?.Invoke();

            IsSelected = !IsSelected;
            m_selectedObject.gameObject.SetActive(IsSelected);
        }
    }
}
