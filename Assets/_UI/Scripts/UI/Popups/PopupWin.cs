using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Geckout
{
    public class PopupWin : PopupUI
    {
        [SerializeField] private Button m_buttonClaim;
        [SerializeField] private Button m_buttonClaimX2;
        [SerializeField] private Text progressText;
        [SerializeField] private Image iconFeatureDisplay;
        [SerializeField] private Image iconFill;
        [SerializeField] private Slider m_sliderProgress;
        [SerializeField] private GameObject m_winContent;

        [Header("New Feature")]
        [SerializeField] private GameObject m_newFeatureContent;
        [SerializeField] private Button m_buttonContinue;
        [SerializeField] private Image m_imageNewFeatureIcon;
        [SerializeField] private Text m_textDes;
        [SerializeField] private Text m_textFeatureName;

        
        private int finalFeatureLv;

        private bool isLockClick = false;

        private float currentProgressNewFeature = 0;

        private void Awake()
        {
            m_buttonClaim.onClick.AddListener(OnClickClaim);
            m_buttonClaimX2.onClick.AddListener(OnClickClaimX2);
            m_buttonContinue.onClick.AddListener(OnClickContinue);
            finalFeatureLv = NewFeatureManager.Instance.FeaturePopupDataDic.Last().Key;
        }

        private void OnClickContinue()
        {
            Hide();
        }

        private void OnClickClaimX2()
        {
            if (isLockClick) return;
            if (currentProgressNewFeature < 1)
            {
                Hide();
            }
            else
            {
                ShowNewFeature();
            }
        }

        private void OnClickClaim()
        {
            if (isLockClick) return;
            if (currentProgressNewFeature < 1)
            {
                Hide();
            }
            else
            {
                ShowNewFeature();
            }
        }

        private void ShowNewFeature()
        {
            m_winContent.gameObject.SetActive(false);
            m_newFeatureContent.gameObject.SetActive(true);
        }

        private void OnEnable()
        {
            onShowDone += UpdateFill;
        }

        private void OnDisable()
        {
            onShowDone -= UpdateFill;
        }

        public override void Hide()
        {
            base.Hide();

        }

        public override void Show(Action onClose)
        {
            m_winContent.SetActive(true);
            m_newFeatureContent.SetActive(false);
            base.Show(onClose);
         
            if (LevelManager.Instance.CurrentLevel > finalFeatureLv)
            {
                onShowDone -= UpdateFill;
                progressText.enabled = false;
                iconFeatureDisplay.enabled = false;
                iconFill.enabled = false;
            }
        }

        public void UpdateFill()
        {
            int featureLevel = -1;
            int featureLevelOld = -1;
            var FeatureLevelKeys = NewFeatureManager.Instance.FeaturePopupDataDic.Keys.ToList();
            for (int i = 0; i < FeatureLevelKeys.Count; i++)
            {
                if (LevelManager.Instance.CurrentLevel < FeatureLevelKeys[i])
                {
                    featureLevel = FeatureLevelKeys[i];
                    if (i > 0)
                    {
                        featureLevelOld = FeatureLevelKeys[i - 1];
                    }
                    break;
                }
            }

            if (featureLevel == -1)
            {
                iconFeatureDisplay.enabled = false;
                this.progressText.enabled = false;
                iconFill.enabled = false;
                return;
            }
            float totalStep = 0;
            if (featureLevelOld != -1)
            {
                totalStep = featureLevel - featureLevelOld;
            }
            else
            {
                featureLevelOld = 1;
                totalStep = featureLevel - 1;
            }

            //var currentFeatureInProgress = NewFeatureManager.Instance.Get

            float progress = (LevelManager.Instance.CurrentLevel + 1 - featureLevelOld) / totalStep;
            float lastProgress = (LevelManager.Instance.CurrentLevel - featureLevelOld) / totalStep;

            var feature = NewFeatureManager.Instance.FeaturePopupDataDic[featureLevel];

            iconFeatureDisplay.sprite = NewFeatureManager.Instance.FeaturePopupDataDic[featureLevel].icon;
            iconFill.sprite = NewFeatureManager.Instance.FeaturePopupDataDic[featureLevel].icon;
            m_imageNewFeatureIcon.sprite = NewFeatureManager.Instance.FeaturePopupDataDic[featureLevel].icon;
            //iconFeatureDisplay.SetNativeSize();
            //iconFill.SetNativeSize();

            currentProgressNewFeature = progress;
            m_textDes.text = feature.des;
            m_textFeatureName.text = feature.title;

            StartCoroutine(Fill(lastProgress, progress, 1f));

        }

        private IEnumerator Fill(float start, float target, float speed = 1)
        {
            float t = start;

            if(target >= 1)
            {
                isLockClick = true;
            }

            while (start < target)
            {
                start += Time.deltaTime * speed;
                start = Mathf.Clamp01(start);
                iconFill.fillAmount = 1 - start;
                m_sliderProgress.value = start;
                this.progressText.text = $"{(int)(start * 100)}%";
                yield return null;
            }

            isLockClick = false;
            

        }
    }
}
