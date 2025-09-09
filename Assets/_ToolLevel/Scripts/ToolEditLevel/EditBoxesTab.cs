using Geckout.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Geckout
{
    public class EditBoxesTab : MonoBehaviour
    {
        [SerializeField] private Button m_buttonGenerate;
        [SerializeField] private Button m_buttonMoveLeft;
        [SerializeField] private Button m_buttonMoveRight;
        [SerializeField] private Button m_buttonMoveUp;
        [SerializeField] private Button m_buttonMoveDown;

        [SerializeField] private TMP_InputField m_inputWidth;
        [SerializeField] private TMP_InputField m_inputHeight;

        public LevelGameEditTool LevelGame { get; set; }

        private MovableBoxData movableBoxData { get; set; } = new();

        private void Awake()
        {
            m_buttonGenerate.onClick.AddListener(OnClickGenerate);
        }

        private void OnClickGenerate()
        {
            movableBoxData = new MovableBoxData();
            movableBoxData.boxSize = new Vector2Int(int.Parse(m_inputWidth.text), int.Parse(m_inputHeight.text));
            LevelGame.GameMap.SpawnBoxMove(movableBoxData);
        }
    }
}
