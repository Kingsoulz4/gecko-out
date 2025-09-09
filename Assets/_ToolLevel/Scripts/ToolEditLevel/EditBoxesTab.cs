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
            m_buttonMoveDown.onClick.AddListener(OnClickMoveDown);
            m_buttonMoveUp.onClick.AddListener(OnClickMoveUp);
            m_buttonMoveRight.onClick.AddListener(OnClickMoveRight);
            m_buttonMoveLeft.onClick.AddListener(OnClickMoveLeft);
        }

        private void OnClickMoveLeft()
        {
            LevelGame.SelectedBoxMove.MoveByOffset(Vector2Int.left);
        }

        private void OnClickMoveRight()
        {
            LevelGame.SelectedBoxMove.MoveByOffset(Vector2Int.right);
        }

        private void OnClickMoveUp()
        {
            LevelGame.SelectedBoxMove.MoveByOffset(Vector2Int.up);
        }

        private void OnClickMoveDown()
        {
            LevelGame.SelectedBoxMove.MoveByOffset(Vector2Int.down);
        }

        private void OnClickGenerate()
        {
            movableBoxData = new MovableBoxData();
            movableBoxData.boxSize = new Vector2Int(int.Parse(m_inputWidth.text), int.Parse(m_inputHeight.text));
            var moveBox = LevelGame.GameMap.SpawnBoxMove(movableBoxData);
            LevelGame.SelectBoxMove(moveBox);
        }
    }
}
