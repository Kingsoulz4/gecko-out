using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Geckout
{
    public class InGameScreenUI : ScreenUI
    {
        [SerializeField] private Button m_buttonPause;
        [SerializeField] private Button m_buttonReplay;

        [SerializeField] private TextMeshProUGUI m_textTime;
        [SerializeField] private TextMeshProUGUI m_textLevel;

        private void Awake()
        {
            
        }

        private void Update()
        {
            if (LevelManager.Instance.LevelGame != null && LevelManager.Instance.LevelGame.GameLevelData != null)
            {
                var timeSecond = LevelManager.Instance.LevelGame.CurrentTimeLevelRemaining;
                m_textTime.text = $"{(int)(timeSecond / 60)}:{(int)(timeSecond % 60)}";
            }
        }

        
    }
}
