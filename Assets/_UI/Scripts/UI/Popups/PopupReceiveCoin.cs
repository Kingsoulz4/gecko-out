using System.Collections;
using DG.Tweening;
using Geckout;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Geckout
{
    public class PopupReceiveCoin : PopupUI
    {
        [Header("UI References")]
        [SerializeField] private Text _txtQuantity;
        [SerializeField] private Transform _coinContainer;
        [SerializeField] private Transform _startPos;
        [SerializeField] private GoldDisplay _goldCounter;
        [SerializeField] private ParticleSystem m_targetPointFx;

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

            for (int i = 0; i < count; i++)
            {
                var rect = _coinContainer.GetChild(i).GetComponent<RectTransform>();
                _initialPos[i] = rect.localPosition;
                _initialRot[i] = rect.localRotation;
            }

            _originScale = _goldCounter.transform.localScale.x;
        }

        public void PlayCoinFX(Vector3 coinBarPosition, Vector3 textBonusStartPos, int coinCount, UnityAction onFinish = null)
        {
            gameObject.SetActive(true);
            transform.parent.SetAsLastSibling();

            // Reset before play
            ResetCoins();

            StartCoroutine(CoinFXRoutine(coinBarPosition, coinCount, onFinish));
        }

        private IEnumerator CoinFXRoutine(Vector3 coinBarPosition, int coinCount, UnityAction onFinish)
        {
            _goldCounter.transform.position = coinBarPosition;
            _goldCounter.SetText(UserDataManager.Gold - coinCount);
            _goldCounter.Sync = false;

            PlayTextFx(coinCount);

            //AudioManager.Instance.PlayAudioFX(AudioType.CoinCollecting);

            float delayCount = 0f;
            for (int i = 0; i < _coinContainer.childCount; i++)
            {
                Transform coin = _coinContainer.GetChild(i);
                int index = i;

                Sequence coinSeq = DOTween.Sequence();
                coinSeq.Append(coin.DOScale(1f, _moveOutDuration).SetEase(Ease.OutBack))
                       .Join(coin.DOLocalMove(_initialPos[i], _moveOutDuration).SetEase(Ease.OutBack))
                       .AppendInterval(_moveOutDelay)
                       .Append(coin.DOMove(_goldCounter.ImgCoinIcon.transform.position, _moveToTargetDuration).SetEase(Ease.InBack))
                       .Join(coin.DOScale(0.5f, _moveToTargetDuration))
                       .Join(coin.DORotate(Vector3.zero, _moveToTargetDuration, RotateMode.Fast))
                       .Append(coin.DOScale(0f, 0.25f).SetEase(Ease.InBack))
                       .SetDelay(delayCount)
                       .OnComplete(() =>
                       {
                           //AudioManager.Instance.PlayCoinDingFX();
                           //VibrationManager.VibrateWeak();
                           _goldCounter.SetText(UserDataManager.Gold - coinCount + coinCount / _coinContainer.childCount * (index + 1));

                           if (index == _coinContainer.childCount - 1)
                           {
                               // Final sync
                               _goldCounter.SetText(UserDataManager.Gold);
                               _goldCounter.Sync = true;

                               //m_targetPointFx?.Play();
                               onFinish?.Invoke();
                           }
                       });
                

                delayCount += _coinStaggerDelay;
            }

            // Counter bounce FX
            _goldCounter.transform.DOScale(_originScale * 1.1f, 0.1f)
                .SetLoops(8, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetDelay(1f);

            yield return null;
        }

        private void PlayTextFx(int coinCount)
        {
            _txtQuantity.gameObject.SetActive(true);
            _txtQuantity.text = $"+{coinCount}";
            _txtQuantity.DOFade(0,0);
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
            DOTween.Kill(_goldCounter.transform);
            foreach (Transform coin in _coinContainer)
                DOTween.Kill(coin);
        }

#if UNITY_EDITOR
        [ContextMenu("TestReceiveCoinFx")]
        public void TestReceiveCoinFx()
        {
            PlayCoinFX(_goldCounter.transform.position, Vector3.zero, 182);
        }
#endif
    }
}
