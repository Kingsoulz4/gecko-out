using Geckout.Data;
using System;
using System.Collections;
using System.Collections.Generic;
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

        public LevelGame LevelGame { get; set; }

        private int currentSelectedColorIndex = 0;

        private PortalData portalData;

        private PortalData DogData
        {
            get
            {
                if (portalData == null)
                {
                    portalData = new PortalData();
                }
                return LevelGame != null && LevelGame.selectedDog != null ? LevelGame.selectedPortal.PortalData : portalData;

            }
        }

        private void Awake()
        {
            m_buttonAddPortal.onClick.AddListener(OnClickAddPortal);
            m_buttonDeletePortal.onClick.AddListener(OnClickDeletePortal);
        }

        private void OnClickDeletePortal()
        {
            LevelGame.ChangeTypeSelectedTiles(Data.MapTileType.Normal);
        }

        private void OnClickAddPortal()
        {
            LevelGame.ChangeTypeSelectedTiles(Data.MapTileType.Portal);

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
                    if (currentSelectedColorIndex >= DogData.listColor.Count)
                    {
                        DogData.listColor.Add(color.Key);
                    }
                    else
                    {
                        DogData.listColor[currentSelectedColorIndex] = color.Key;
                    }

                    if (LevelGame.selectedDog != null)
                    {
                        LevelGame.selectedDog.UpdateColor();
                    }

                });
            }
        }
    }
}
