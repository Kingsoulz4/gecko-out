using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Geckout
{
    public class CheatGame : MonoBehaviour
    {
        [SerializeField] private InputField m_inputTimeRemain;

        private void Awake()
        {
            m_inputTimeRemain.onSubmit.AddListener(OnEditTimeRemain);
        }

        private void OnEditTimeRemain(string arg0)
        {
            LevelManager.Instance.LevelGame.CurrentTimeLevelRemaining = int.Parse(arg0);
        }
    }
}
