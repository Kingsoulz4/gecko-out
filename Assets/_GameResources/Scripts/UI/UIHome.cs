using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Geckout
{
    public class UIHome : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI m_textLevel;
        [SerializeField] private Button m_buttonPlay;

        private void Awake()
        {
            m_buttonPlay.onClick.AddListener(OnClickPlay);
        }

        private void OnEnable()
        {
            m_textLevel.text = $"Level {LevelManager.Instance.CurrentLevel}";
        }

        private void OnClickPlay()
        {
            LevelManager.Instance.StartCurrentLevel();
            gameObject.SetActive(false);
        }
    }
}
