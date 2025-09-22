using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Geckout
{
    public class PopupTutorialNewFeature : PopupUI
    {
        [SerializeField] private Button m_buttonGotIt;
        [SerializeField] private Text m_textFeatureName;
        [SerializeField] private Text m_textFeatureDes;
        [SerializeField] private Image m_imageFeatureIcon;

        private void Awake()
        {
            m_buttonGotIt.onClick.AddListener(OnClickGotIt);
            
        }

        private void OnEnable()
        {
            var feature = NewFeatureManager.Instance.GetNewFeatureInProgress();
            if (feature != null )
            {
                m_textFeatureDes.text = feature.des;
                m_imageFeatureIcon.sprite = feature.icon;
                m_textFeatureName.text = feature.title;
            }
        }

        private void OnClickGotIt()
        {
            Hide();
        }
    }
}
