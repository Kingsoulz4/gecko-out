using Geckout.Data;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Geckout
{
    public class EditBoxesTab : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button m_buttonGenerate;
        [SerializeField] private Button m_buttonMoveLeft;
        [SerializeField] private Button m_buttonMoveRight;
        [SerializeField] private Button m_buttonMoveUp;
        [SerializeField] private Button m_buttonMoveDown;
        [SerializeField] private Button m_buttonPlace;
        [SerializeField] private Button m_buttonDelete;

        [Header("Dropdowns")]
        [SerializeField] private TMP_Dropdown m_dropDownMoveType;
        [SerializeField] private TMP_Dropdown m_dropDownBoxType;

        [Header("Inputs")]
        [SerializeField] private TMP_InputField m_inputWidth;
        [SerializeField] private TMP_InputField m_inputHeight;
        [SerializeField] private TMP_InputField m_inputDifuseCount;

        public LevelGameEditTool LevelGame { get; set; }
        private BoxBaseData BoxBaseData { get; set; } = new();
        private CrateData CrateData { get; set; } = new();

        private void Awake()
        {
            m_buttonGenerate.onClick.AddListener(OnClickGenerate);
            m_buttonPlace.onClick.AddListener(OnClickPlace);

            m_buttonMoveLeft.onClick.AddListener(() => OnClickMove(Vector2Int.left));
            m_buttonMoveRight.onClick.AddListener(() => OnClickMove(Vector2Int.right));
            m_buttonMoveUp.onClick.AddListener(() => OnClickMove(Vector2Int.up));
            m_buttonMoveDown.onClick.AddListener(() => OnClickMove(Vector2Int.down));
            m_buttonDelete.onClick.AddListener(OnClickDelete);

            m_dropDownMoveType.onValueChanged.AddListener(OnDropDownMoveTypeChangeValue);
            m_inputDifuseCount.onSubmit.AddListener(OnEnterDifuseCount);
        }

        private void OnClickDelete()
        {
            LevelGame.DeleteSelectedBoxes();
            LevelGame.ClearAllSelectedTiles();
        }

        private void OnEnable()
        {
            if (LevelGame?.SelectedBoxMove != null)
            {
                BoxBaseData = LevelGame.SelectedBoxMove.Data;
                if (BoxBaseData is MovableBoxData movable)
                {
                    m_dropDownMoveType.value = (int)movable.wayDirection;
                }
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow)) OnClickMove(Vector2Int.up);
            if (Input.GetKeyDown(KeyCode.DownArrow)) OnClickMove(Vector2Int.down);
            if (Input.GetKeyDown(KeyCode.LeftArrow)) OnClickMove(Vector2Int.left);
            if (Input.GetKeyDown(KeyCode.RightArrow)) OnClickMove(Vector2Int.right);
        }

        private void OnEnterDifuseCount(string arg0)
        {
            if(LevelGame.SelectedCrate != null && CrateData != null)
            {
                CrateData.difusionCount = int.Parse(arg0);
            }
        }

        private void OnDropDownMoveTypeChangeValue(int arg0)
        {
            if (BoxBaseData is MovableBoxData movable)
            {
                movable.wayDirection = (WayDirection)m_dropDownMoveType.value;
                LevelGame.SelectedBoxMove.UpdateVisual();
            }
        }

        private void OnClickPlace()
        {
            GetSelectedBoxAction(
                onMoveBox: b => b.PlaceMoveBox(),
                onCrate: c => c.PlaceMoveBox()
            );
        }

        private void OnClickMove(Vector2Int direction)
        {
            GetSelectedBoxAction(
                onMoveBox: b => b.MoveByOffset(direction),
                onCrate: c => c.MoveByOffset(direction)
            );
        }

        private void OnClickGenerate()
        {
            Vector2Int size = new Vector2Int(
                int.Parse(m_inputWidth.text),
                int.Parse(m_inputHeight.text)
            );

            if (m_dropDownBoxType.value == 0) // Movable Box
            {
                var boxMoveData = new MovableBoxData
                {
                    wayDirection = (WayDirection)m_dropDownMoveType.value,
                    boxSize = size
                };

                var moveBox = LevelGame.GameMap.SpawnBoxMove(boxMoveData);
                LevelGame.SelectMovableBox(moveBox);
                BoxBaseData = boxMoveData;
            }
            else if (m_dropDownBoxType.value == 1) // Crate
            {
                CrateData = new CrateData { boxSize = size };
                var crate = LevelGame.GameMap.SpawnCrate(CrateData);
                LevelGame.SelectCrateBox(crate);
            }
        }

        /// <summary>
        /// Helper to avoid duplicate "if dropdown == 0/1" checks.
        /// </summary>
        private void GetSelectedBoxAction(Action<BoxMove> onMoveBox, Action<Crate> onCrate)
        {
            if (m_dropDownBoxType.value == 0 && LevelGame.SelectedBoxMove != null)
            {
                onMoveBox?.Invoke(LevelGame.SelectedBoxMove);
            }
            else if (m_dropDownBoxType.value == 1 && LevelGame.SelectedCrate != null)
            {
                onCrate?.Invoke(LevelGame.SelectedCrate);
            }
        }
    }
}
