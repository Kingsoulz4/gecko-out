using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    using System.Collections;
    using DG.Tweening;
    using Geckout;
    using TMPro;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;

    namespace Geckout
    {
        public class PopupReceiveHeart : PopupUI
        {
            [Header("UI References")]
            [SerializeField] private Text _txtQuantity;
            [SerializeField] private Transform _coinContainer;
            [SerializeField] private Transform _startPos;
            [SerializeField] private HeartDisplay _heartBar;
            [SerializeField] private ParticleSystem m_targetPointFx;
            [SerializeField] private GameObject m_coinPrefab;

            [Header("Timings")]
            [SerializeField] private float _moveOutDuration = 0.3f;
            [SerializeField] private float _moveOutDelay = 0.2f;
            [SerializeField] private float _moveToTargetDuration = 0.5f;
            [SerializeField] private float _coinStaggerDelay = 0.15f;

            private Vector3[] _initialPos;
            private Quaternion[] _initialRot;
            private float _originScale = 1f;

            private void Awake()
            {
                int count = _coinContainer.childCount;
                _initialPos = new Vector3[count];
                _initialRot = new Quaternion[count];
                _heartBar.Sync = false;

                for (int i = 0; i < count; i++)
                {
                    var rect = _coinContainer.GetChild(i).GetComponent<RectTransform>();
                    _initialPos[i] = rect.localPosition;
                    _initialRot[i] = rect.localRotation;
                }

                _originScale = _heartBar.transform.localScale.x;
            }

            public void PlayCollectFx(Vector3 coinBarPosition, Vector3 textBonusStartPos, int coinCount, UnityAction onFinish = null)
            {
                gameObject.SetActive(true);
                transform.parent.SetAsLastSibling();

                // Reset before play
                ResetCoins();

                StartCoroutine(CoinFXRoutine(coinBarPosition, coinCount, onFinish));
            }

            private IEnumerator CoinFXRoutine(Vector3 coinBarPosition, int heartCount, UnityAction onFinish)
            {
                _heartBar.transform.position = coinBarPosition;
                _heartBar.Sync = false;
                _heartBar.SetText(UserDataManager.Gold - heartCount);

                PlayTextFx(heartCount);

                //AudioManager.Instance.PlayAudioFX(AudioType.CoinCollecting);

                for(int i=0; i<_coinContainer.childCount; i++)
                {
                    _coinContainer.GetChild(i).gameObject.SetActive(false);
                }

                float delayCount = 0f;
                var countPart = Mathf.Min(_coinContainer.childCount, heartCount);
                for (int i = 0; i < countPart; i++)
                {
                    _heartBar.SetText(UserDataManager.Gold - heartCount);
                    Transform coin = _coinContainer.GetChild(i);
                    coin.gameObject.SetActive(true);

                    Transform coinObject = Instantiate(m_coinPrefab, coin).transform;
                    coinObject.localPosition = Vector3.zero + Vector3.forward * Random.Range(-100f, -120f);
                    //coinObject.DOLocalRotate(coinObject.localRotation.eulerAngles + Vector3.forward * 360, 0.5f, RotateMode.FastBeyond360)
                    //    .SetEase(Ease.Linear)
                    //    .SetLoops(-1, LoopType.Restart);

                    int index = i;

                    Sequence coinSeq = DOTween.Sequence();
                    coinSeq.Append(coin.DOScale(1f, _moveOutDuration).SetEase(Ease.OutBack))
                           .Join(coin.DOLocalMove(_initialPos[i], _moveOutDuration).SetEase(Ease.OutBack))
                           .Join(coinObject.DOLocalRotate(coinObject.localRotation.eulerAngles + Vector3.forward * 360, _moveOutDuration * 5, RotateMode.FastBeyond360)
                                .SetEase(Ease.Linear)
                                /*.SetLoops(-1, LoopType.Restart)*/)
                           .AppendInterval(_moveOutDelay)
                           .Append(coin.DOMove(_heartBar.ImgIcon.transform.position + Vector3.forward * 5, _moveToTargetDuration).SetEase(Ease.InBack))
                           .Join(coin.DOScale(0.8f, _moveToTargetDuration * 0.8f).OnComplete(() =>
                           {
                               coin.DOScale(0f, _moveToTargetDuration * 0.2f).SetEase(Ease.InBack);
                           }))
                           .Join(coinObject.DORotate(coinObject.localRotation.eulerAngles + Vector3.forward * 360, _moveToTargetDuration * 2, RotateMode.FastBeyond360).SetEase(Ease.Linear))
                           //.Append(coin.DOScale(0f, 0.25f).SetEase(Ease.InBack))
                           .Join(_heartBar.transform.DOScale(_originScale * 1.1f, 0.05f).SetEase(Ease.InOutSine)
                                .SetDelay(_moveToTargetDuration)
                                .OnComplete(() =>
                                {
                                    _heartBar.SetText(UserDataManager.Heart - heartCount + heartCount / countPart * (index + 1));
                                    _heartBar.transform.DOScale(_originScale, 0.05f);
                                }))
                           .SetDelay(delayCount)
                           .OnComplete(() =>
                           {
                               //AudioManager.Instance.PlayCoinDingFX();
                               //VibrationManager.VibrateWeak();
                               AudioManager.Instance.PlayOneShot(AudioClipNames.COLLECT_COIN.ToString(), 1f);

                               Destroy(coinObject.gameObject);

                               if (index == countPart - 1)
                               {
                                   // Final sync
                                   _heartBar.SetText(UserDataManager.Gold);
                                   //_goldCounter.Sync = true;
                                   _heartBar.gameObject.SetActive(false);

                                   //m_targetPointFx?.Play();
                                   onFinish?.Invoke();


                                   Hide();
                               }
                           });

                    delayCount += _coinStaggerDelay;
                }

                yield return null;
            }

            private void PlayTextFx(int coinCount)
            {
                _txtQuantity.gameObject.SetActive(true);
                _txtQuantity.text = $"+{coinCount}";
                _txtQuantity.DOFade(0, 0);
                _txtQuantity.transform.localPosition = Vector3.zero;

                Sequence txtSeq = DOTween.Sequence();
                txtSeq.Append(_txtQuantity.DOFade(1, 0.3f))
                      .Join(_txtQuantity.transform.DOLocalMoveY(50, 0.5f))
                      .AppendInterval(_coinContainer.childCount * _coinStaggerDelay)
                      .Append(_txtQuantity.DOFade(0, 0.4f))
                      .Join(_txtQuantity.transform.DOLocalMoveY(100, 0.4f));
            }

            private void ResetCoins()
            {
                _heartBar.gameObject.SetActive(true);
                for (int i = 0; i < _coinContainer.childCount; i++)
                {
                    Transform coin = _coinContainer.GetChild(i);
                    coin.localPosition = _startPos.localPosition;
                    coin.localRotation = Quaternion.identity;
                    coin.localScale = Vector3.zero;
                }
            }

            private void OnDisable()
            {
                DOTween.Kill(_txtQuantity.transform);
                DOTween.Kill(_heartBar.transform);
                foreach (Transform coin in _coinContainer)
                    DOTween.Kill(coin);
            }

#if UNITY_EDITOR
            [ContextMenu("TestReceiveCoinFx")]
            public void TestReceiveCoinFx()
            {
                PlayCollectFx(_heartBar.transform.position, Vector3.zero, 182);
            }
#endif
        }
    }

}
