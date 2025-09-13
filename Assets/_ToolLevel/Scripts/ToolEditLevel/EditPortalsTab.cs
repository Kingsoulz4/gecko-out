using Geckout.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Geckout
{
    public class EditPortalsTab : MonoBehaviour
    {
        [SerializeField] private Button m_buttonAddPortal;
        [SerializeField] private Button m_buttonDeletePortal;
        [SerializeField] private List<ButtonColorPicked> m_listButtonColorPicked;
        [SerializeField] private Transform m_listColorPickContainer;
        [SerializeField] private Button m_colorPickPrefab;
        [SerializeField] private TMP_InputField m_inputFreezeCount;

        public LevelGameEditTool LevelGame { get; set; }

        private int currentSelectedColorIndex = 0;

        private PortalData portalData;

        private PortalData PortalData
        {
            get
            {
                if (portalData == null)
                {
                    portalData = new PortalData();
                }
                return LevelGame != null && LevelGame.SelectedPortal != null ? LevelGame.SelectedPortal.PortalData : portalData;

            }
        }

        private void Awake()
        {
            m_buttonAddPortal.onClick.AddListener(OnClickAddPortal);
            m_buttonDeletePortal.onClick.AddListener(OnClickDeletePortal);
            foreach (var buttonColorPicked in m_listButtonColorPicked)
            {
                buttonColorPicked.OnClick.AddListener(OnClickSelectColorPicked);
            }
            m_inputFreezeCount.onSubmit.AddListener(OnEnterFreezeCount);
        }

        private void Start()
        {
            UpdateUI(PortalData);
        }

        private void OnClickDeletePortal()
        {
            //LevelGame.ChangeTypeSelectedTiles(Data.MapTileType.Normal);
            LevelGame.RemoveAllSelectedPortals();
        }

        private void OnClickAddPortal()
        {
            //LevelGame.ChangeTypeSelectedTiles(Data.MapTileType.Portal);
            LevelGame.AddNewPortal(PortalData);
            PortalData.freezeTimeCount = int.Parse(m_inputFreezeCount.text);
            LevelGame.SelectedPortal.InitMechanic();
        }

        private void OnEnterFreezeCount(string arg0)
        {
            PortalData.freezeTimeCount = int.Parse(arg0);
            if (LevelGame != null && LevelGame.SelectedPortal != null)
            {
                LevelGame.SelectedPortal.InitMechanic();
            }
        }

        public void UpdateUI(PortalData dogData)
        {
            var listColorData = LevelGame.GameLevelData.colorAndMaterialData.listColor;
            for (int i = 0; i < m_listButtonColorPicked.Count; i++)
            {
                if (i < dogData.listColor.Count)
                {
                    m_listButtonColorPicked[i].SetEmpty(false);
                    m_listButtonColorPicked[i].SetColor(listColorData[dogData.listColor[i]]);
                }
                else
                {
                    m_listButtonColorPicked[i].SetEmpty(true);
                }
            }
            UpdateUI();
        }

        public void UpdateUI()
        {
            var listColorData = LevelGame.GameLevelData.colorAndMaterialData;
            Utils.RemoveAllChilds(m_listColorPickContainer);
            foreach (var color in listColorData.listColor)
            {
                var itemColorPick = Instantiate(m_colorPickPrefab, m_listColorPickContainer);
                itemColorPick.image.color = color.Value;
                itemColorPick.gameObject.SetActive(true);
                itemColorPick.onClick.AddListener(() =>
                {

                    m_listButtonColorPicked[currentSelectedColorIndex].SetColor(color.Value);
                    m_listButtonColorPicked[currentSelectedColorIndex].SetEmpty(false);
                    if (currentSelectedColorIndex >= PortalData.listColor.Count)
                    {
                        PortalData.listColor.Add(color.Key);
                    }
                    else
                    {
                        PortalData.listColor[currentSelectedColorIndex] = color.Key;
                    }

                    if (LevelGame.SelectedPortal != null)
                    {
                        LevelGame.SelectedPortal.UpdateVisual();
                    }

                });
            }
            m_inputFreezeCount.text = PortalData.freezeTimeCount.ToString();

        }

        private void OnClickSelectColorPicked(int index)
        {
            var listColorData = LevelGame.GameLevelData.colorAndMaterialData.listColor;

            if (PortalData.listColor.Count <= index)
            {
                PortalData.listColor.Add(ColorType.Red);
                m_listButtonColorPicked[index].SetEmpty(false);
                m_listButtonColorPicked[index].SetColor(listColorData[ColorType.Red]);
            }
            else
            {
                currentSelectedColorIndex = index;
            }
        }
    }
}
