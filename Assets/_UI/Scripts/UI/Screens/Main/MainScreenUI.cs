using DG.Tweening;
using Geckout;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

    public class MainScreenUI : ScreenUI
    {
        [Space, Header("UI")]
        [SerializeField] Button btn_Play;
        [SerializeField] Text txt_Level;
        //[SerializeField] ChangeThemeButtonPlay changeThemeButtonPlay;
        [SerializeField] AudioClip soundBG;
        [SerializeField] ScrollScreenHorizontal horizontal;
        [SerializeField] MenuTabSystem menuTab;
        [SerializeField] GameObject blockUI;
        [SerializeField] Text[] arrTextLevel;
        [SerializeField] Button btn_RemoveAds;
        [SerializeField] Button btn_StarterPackage;
        [SerializeField] GameObject objOtherGame;
        [SerializeField] ButtonMainBase[] listButtonPackage;

        [Space, Header("Visual")]
        [SerializeField] RectTransform rect_Left;
        [SerializeField] RectTransform rect_Right;
        [SerializeField] RectTransform rect_Center;
        [Header("Visual - Reward Gold")]
        [SerializeField] GameObject obj_GoldAnimationPrefab;
        [SerializeField] RectTransform rect_TargetGoldPos;
        [SerializeField] RectTransform rect_TargetGoldPosShop;
        [SerializeField] ParticleSystem fx_Gold;
        [SerializeField] Text txt_Gold;
        [SerializeField] Text txt_GoldShop;
        public float timeMoveCoinBack = 0.5f;
        public float timeMoveCoinUp = 0.75f;
        public AudioClip clip_SpawnItem;
        public AudioClip clip_GoldEnd;
        [Header("Visual - Reward Heart")]
        [SerializeField] RectTransform rect_TargetHeartPos;
        [Header("Visual - Reward Booster")]
        [SerializeField] RectTransform rect_TargetBoosterPos;


        public RectTransform RectTargetGold => rect_TargetGoldPos;
        public RectTransform RectTargetGoldShop => rect_TargetGoldPosShop;
        public RectTransform RectTargetHeart => rect_TargetHeartPos;
        public RectTransform RectTargetBooster => rect_TargetBoosterPos;
        public int CurrentPanel => horizontal.currentPanel;

        public override void Initialize(UIManager uiManager)
        {
            base.Initialize(uiManager);
            btn_Play.onClick.AddListener(PlayLevel);
            txt_Level.text = "LEVEL ";
            if (arrTextLevel != null)
            {
                for (int i = 0; i < arrTextLevel.Length; i++)
                {
                    arrTextLevel[i].text = (LevelManager.Instance.CurrentLevelNum + i).ToString();
                }
            }
            UIManager.OnRefeshBannerAndAds += UpdateButtonRemoveAds;
            UpdateButtonRemoveAds();
            btn_RemoveAds.onClick.AddListener(() =>
            {
                UIRemoveAds uiRemove = UIManager.Instance.ShowPopup<UIRemoveAds>(null);
                uiRemove.OnBuySS = deActionButtonRemoveAds;
            });
        }

        public void ResetVisual()
        {
           
        }
        private void UpdateButtonRemoveAds()
        {
         
            ResetVisual();
        }
        void deActionButtonRemoveAds()
        {
            btn_RemoveAds.gameObject.SetActive(false);
        }

        public override void Active()
        {
            base.Active();
            //AudioManager.Instance.StopAllMusic();
            //this.Wait(0.5f, () => AudioManager.Instance.PlayMusic(soundBG, 0.5f, true));
        }

        private void PlayLevel()
        {
            LevelManager.Instance.StartCurrentLevel();
            UIManager.Instance.ShowScreen<InGameScreenUI>();
        }

        public void MoveCoin(int amount, string reason, string where)
        {
        }

        private void MoveValueTop(float timeDelay, GameObject objSpawn, RectTransform targetPos, int currentCoinText, bool isCoin, Action playFx, bool isFinish)
        {
            DOVirtual.DelayedCall(timeDelay + 0.25f, () =>
            {
                AudioManager.Instance.PlayOneShot(clip_SpawnItem, 1);
                objSpawn.SetActive(true);
                Vector3 spawnPos = rect_Left.position;
                spawnPos.x += Mathf.Round(UnityEngine.Random.Range(-7.5f, 7.5f) / 10f) * 10f;
                spawnPos.y += Mathf.Round(UnityEngine.Random.Range(-7.5f, 7.5f) / 10f) * 10f;
                objSpawn.transform.position = spawnPos;
                objSpawn.transform.localScale = Vector3.zero;
                Vector3 backPos = objSpawn.transform.position;
                backPos.x -= 5f;
                backPos.y -= 8.5f;

                Sequence MoveSequence = DOTween.Sequence();
                MoveSequence.Append(objSpawn.transform.DOScale(Vector3.one * (isCoin ? UnityEngine.Random.Range(1.2f, 1.25f) : 1.1f), 0.4f).SetEase(Ease.InOutQuad))
                            .Join(objSpawn.transform.DOMove(backPos, timeMoveCoinBack).SetEase(Ease.InOutQuad))
                            .Append(objSpawn.transform.DOMove(targetPos.position, timeMoveCoinUp).SetEase(Ease.InOutQuad))
                            .OnComplete(() =>
                            {
                                if (isCoin)
                                {
                                    AudioManager.Instance.PlayOneShot(clip_GoldEnd, 1);
                                }
                                Sequence ScaleSequence = DOTween.Sequence();
                                ScaleSequence.Append(targetPos.DOScale(Vector3.one * 1.3f, 0.06f).SetEase(Ease.InBack))
                                            .Append(targetPos.DOScale(Vector3.one, 0.06f).SetEase(Ease.InBack));
                                Destroy(objSpawn);
                                playFx?.Invoke(); 
                                blockUI.gameObject.SetActive(false);
                                if (isFinish)
                                {
                                    
                                }
                            });
            });
        }

        public void PlayFx(ParticleSystem particleSystem)
        {
            ParticleSystem particleSystem1 = Instantiate(particleSystem, particleSystem.transform.position, Quaternion.identity, particleSystem.transform.parent);
            particleSystem1.gameObject.SetActive(true);
            particleSystem1.Play();
            DOVirtual.DelayedCall(2, () =>
            {
                if (particleSystem1)
                {
                    Destroy(particleSystem1.gameObject);
                }
            });
        }

        public void RollToShopGold()
        {
            menuTab.ChangeTab(0);
            ShopScreenTab shopScreenTab = menuTab.menuTabCurrent as ShopScreenTab;
            if (shopScreenTab != null)
            {
                shopScreenTab.RollToShopGold();
            }
        }

        private void OnDisable()
        {
            UIManager.OnRefeshBannerAndAds -= UpdateButtonRemoveAds;
            DOTween.Kill(this);
        }

        public RectTransform GetRectTargetGold()
        {
            if(CurrentPanel == 0)
            {
                return rect_TargetGoldPosShop;
            }
            return rect_TargetGoldPos;
        }

        public RectTransform GetRectTargetBooster()
        {
            if (CurrentPanel == 0)
            {
                return null;
            }
            return rect_TargetBoosterPos;
        }
    }