using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Geckout
{
    public class UIGamePlay : MonoBehaviour
    {
        [SerializeField] private Button m_designLevel;
        [SerializeField] private TextMeshProUGUI m_textTime;

        private void Awake()
        {
            m_designLevel.onClick.AddListener(OnClickDesignLevel);
        }

        private void Update()
        {
            if (LevelManager.Instance.LevelGame != null && LevelManager.Instance.LevelGame.GameLevelData != null)
            {
                var timeSecond = LevelManager.Instance.LevelGame.CurrentLevelRemaining;
                m_textTime.text = $"{(int)(timeSecond / 60)}:{(int)(timeSecond % 60)}";
            }
        }

        private void OnClickDesignLevel()
        {
            SceneManager.LoadScene("ToolEditLevel");
        }
    }
}
