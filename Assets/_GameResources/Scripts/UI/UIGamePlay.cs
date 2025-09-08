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

        private void Awake()
        {
            m_designLevel.onClick.AddListener(OnClickDesignLevel);
        }

        private void OnClickDesignLevel()
        {
            SceneManager.LoadScene("ToolEditLevel");
        }
    }
}
