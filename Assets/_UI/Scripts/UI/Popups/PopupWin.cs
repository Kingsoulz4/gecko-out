using DG.Tweening;
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
        [SerializeField] private GameObject m_ribbonObject;
        [SerializeField] private Text m_textLevelComplete;
        [SerializeField] private GameObject m_progressNewFeatureObject;

        [Header("New Feature")]
        [SerializeField] private GameObject m_newFeatureContent;
        [SerializeField] private Button m_buttonContinue;
        [SerializeField] private Image m_imageNewFeatureIcon;
        [SerializeField] private Text m_textDes;
        [SerializeField] private Text m_textFeatureName;

        
        private int finalFeatureLv;

        private bool isLockClick = false;

        private float currentProgressNewFeature = 0;

        public Action<int> OnClaimedReward { get; set;}

        private void Awake()
        {
            m_buttonClaim.onClick.AddListener(OnClickClaim);
            m_buttonClaimX2.onClick.AddListener(OnClickClaimX2);
            m_buttonContinue.onClick.AddListener(OnClickContinue);
            finalFeatureLv = NewFeatureManager.Instance.FeaturePopupDataDic.Last(x => x.Value.displayType == NewFeatureTutDisplayType.POPUP_TEXT).Key;
            UpdateFillInstant(out var _, out var _);
        }

        private void OnClickContinue()
        {
            Hide();
        }

        private void OnClickClaimX2()
        {
            if (isLockClick) return;

            UserDataManager.AddGold(GameManager.Instance.CoinRewardWinGame * 2, "WinX2");

            if (currentProgressNewFeature < 1)
            {
                Hide();
                OnClaimedReward.Invoke(GameManager.Instance.CoinRewardWinGame * 2);
            }
            else
            {
                ShowNewFeature();
            }
        }

        private void OnClickClaim()
        {
            if (isLockClick) return;

            UserDataManager.AddGold(GameManager.Instance.CoinRewardWinGame , "Win");

            if (currentProgressNewFeature < 1)
            {
                Hide();
                OnClaimedReward.Invoke(GameManager.Instance.CoinRewardWinGame);
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


        public override void Hide()
        {
            base.Hide();
            m_buttonClaimX2.transform.DOKill();
        }

        public override void Show(Action onClose)
        {
            m_winContent.SetActive(true);
            m_newFeatureContent.SetActive(false);
            UpdateFillInstant(out var _, out var _);
            base.Show(onClose);
         
            if (LevelManager.Instance.CurrentLevel > finalFeatureLv)
            {
                progressText.enabled = false;
                iconFeatureDisplay.enabled = false;
                iconFill.enabled = false;
            }

            StartCoroutine(IEAnimateShow());
            
        }

        private IEnumerator IEAnimateShow()
        {
            m_ribbonObject.transform.localScale = Vector3.zero;
            m_textLevelComplete.transform.localScale = Vector3.zero;
            m_progressNewFeatureObject.transform.localScale = Vector3.zero;
            m_buttonClaimX2.transform.localScale = Vector3.zero;
            m_buttonClaim.transform.localScale = Vector3.zero;
            yield return null;
            m_ribbonObject.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(0.25f);
            m_textLevelComplete.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(0.25f);
            m_progressNewFeatureObject.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(0.35f);
            UpdateFill();
            yield return new WaitForSeconds(0.5f);
            m_buttonClaimX2.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(0.5f);
            m_buttonClaimX2.transform.DOScale(0.85f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
            m_buttonClaim.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);

        }


        public void UpdateFill()
        {
            float lastProgress, progress;
            var feature = UpdateFillInstant(out lastProgress, out progress);

            // If no feature (disabled UI case), just stop here
            if (feature == null)
                return;

            // Animate fill progress
            StartCoroutine(Fill(lastProgress, progress, 0.5f));
        }

        public NewFeatureItemData UpdateFillInstant(out float lastProgress, out float progress)
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
                lastProgress = progress = 0f;
                return null;
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

            progress = (LevelManager.Instance.CurrentLevel + 1 - featureLevelOld) / totalStep;
            lastProgress = (LevelManager.Instance.CurrentLevel - featureLevelOld) / totalStep;

            var feature = NewFeatureManager.Instance.FeaturePopupDataDic[featureLevel];

            iconFeatureDisplay.sprite = feature.icon;
            iconFill.sprite = feature.icon;
            m_imageNewFeatureIcon.sprite = feature.icon;

            currentProgressNewFeature = lastProgress;
            m_textDes.text = feature.des;
            m_textFeatureName.text = feature.title;

            return feature;
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
