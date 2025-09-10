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
        [SerializeField] private Button m_buttonPlace;
        [SerializeField] private Button m_buttonDelete;
        [SerializeField] private TMP_Dropdown m_dropDownMoveType;
        [SerializeField] private TMP_Dropdown m_dropDownBoxType;

        [SerializeField] private TMP_InputField m_inputWidth;
        [SerializeField] private TMP_InputField m_inputHeight;

        public LevelGameEditTool LevelGame { get; set; }

        private MovableBoxData MovableBoxData { get; set; } = new();

        private CrateData CrateData { get; set; } = new();

        private void Awake()
        {
            m_buttonGenerate.onClick.AddListener(OnClickGenerate);
            m_buttonMoveDown.onClick.AddListener(OnClickMoveDown);
            m_buttonMoveUp.onClick.AddListener(OnClickMoveUp);
            m_buttonMoveRight.onClick.AddListener(OnClickMoveRight);
            m_buttonMoveLeft.onClick.AddListener(OnClickMoveLeft);
            m_buttonPlace.onClick.AddListener(OnClickPlace);
            m_dropDownMoveType.onValueChanged.AddListener(OnDropDownMoveTypeChangeValue);
        }

        private void OnEnable()
        {
            if (LevelGame != null && LevelGame.SelectedBoxMove != null)
            {
                MovableBoxData = LevelGame.SelectedBoxMove.Data;
                m_dropDownMoveType.value = (int)MovableBoxData.wayDirection;
            }
        }

        private void OnDropDownMoveTypeChangeValue(int arg0)
        {
            MovableBoxData.wayDirection = (WayDirection)m_dropDownMoveType.value;
            LevelGame.SelectedBoxMove.UpdateVisual();
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.UpArrow))
            {
                OnClickMoveUp();
            }

            if(Input.GetKeyDown(KeyCode.DownArrow))
            {
                OnClickMoveDown();
            }

            if(Input.GetKeyDown(KeyCode.LeftArrow))
            {
                OnClickMoveLeft();
            }

            if(Input.GetKeyDown(KeyCode.RightArrow))
            {
                OnClickMoveRight();
            }
        }

        private void OnClickPlace()
        {
            LevelGame.SelectedBoxMove.PlaceMoveBox();
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
            if (m_dropDownBoxType.value == 0)
            {
                MovableBoxData = new MovableBoxData();
                MovableBoxData.wayDirection = (WayDirection)m_dropDownMoveType.value;
                MovableBoxData.boxSize = new Vector2Int(int.Parse(m_inputWidth.text), int.Parse(m_inputHeight.text));
                var moveBox = LevelGame.GameMap.SpawnBoxMove(MovableBoxData);
                LevelGame.SelectBoxMove(moveBox);
            }
            else if(m_dropDownBoxType.value == 1)
            {
                CrateData = new CrateData();
                CrateData.boxSize = new Vector2Int(int.Parse(m_inputWidth.text), int.Parse(m_inputHeight.text));
                var newCrate = LevelGame.GameMap.SpawnCrate(CrateData);
            }
        }
    }
}
