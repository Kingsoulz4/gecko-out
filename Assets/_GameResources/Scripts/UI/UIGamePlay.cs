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
            if (GamePlayManager.Instance.LevelGame != null && GamePlayManager.Instance.LevelGame.GameLevelData != null)
            {
                var timeSecond = GamePlayManager.Instance.LevelGame.GameLevelData.time;
                m_textTime.text = $"{timeSecond / 60}:{timeSecond % 60}";
            }
        }

        private void OnClickDesignLevel()
        {
            SceneManager.LoadScene("ToolEditLevel");
        }
    }
}
