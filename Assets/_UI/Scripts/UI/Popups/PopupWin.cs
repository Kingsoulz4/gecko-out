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
        [SerializeField] private Text progress;
        [SerializeField] private Image bg;
        [SerializeField] private Image iconFill;
        private int finalFeatureLv;

        private void Awake()
        {
            m_buttonClaim.onClick.AddListener(OnClickClaim);
            m_buttonClaimX2.onClick.AddListener(OnClickClaimX2);
            var FeatureLevelKeys = NewFeatureManager.Instance.FeaturePopupDataDic.Keys.ToList();
            finalFeatureLv = FeatureLevelKeys[FeatureLevelKeys.Count - 1];
        }


        private void OnClickClaimX2()
        {
            Hide();
        }

        private void OnClickClaim()
        {
            Hide();
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
            base.Show(onClose);
            if (LevelManager.Instance.CurrentLevel > finalFeatureLv)
            {
                onShowDone -= UpdateFill;
                progress.enabled = false;
                bg.enabled = false;
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
                if (LevelManager.Instance.CurrentLevel - 1 < FeatureLevelKeys[i])
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
                bg.enabled = false;
                this.progress.enabled = false;
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

            float progress = (LevelManager.Instance.CurrentLevel - featureLevelOld) / totalStep;
            float lastProgress = (LevelManager.Instance.CurrentLevel - 1 - featureLevelOld) / totalStep;

            bg.sprite = NewFeatureManager.Instance.FeaturePopupDataDic[featureLevel].spriteBG;
            iconFill.sprite = NewFeatureManager.Instance.FeaturePopupDataDic[featureLevel].spriteFill;
            bg.SetNativeSize();
            iconFill.SetNativeSize();

            StartCoroutine(Fill(lastProgress, progress, 1f));

        }

        private IEnumerator Fill(float start, float target, float speed = 1)
        {
            float t = start;

            while (start < target)
            {
                start += Time.deltaTime * speed;
                start = Mathf.Clamp01(start);
                iconFill.fillAmount = start;
                this.progress.text = $"{(int)(start * 100)}%";
                yield return null;
            }
        }
    }
}
