using Geckout.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Geckout
{
    public class DesignDogTab : MonoBehaviour
    {
        [SerializeField] private Button m_buttonGenerate;
        [SerializeField] private Button m_buttonDelete;
        [SerializeField] private Transform m_listColorPickContainer;
        [SerializeField] private Button m_colorPickPrefab;
        [SerializeField] private List<ButtonColorPicked> m_listButtonColorPicked;

        public LevelGameEditTool LevelGame { get; set; }

        private int currentSelectedColorIndex = 0;

        private DogData dogData;

        private DogData DogData
        {
            get
            {
                if (dogData == null)
                {
                    dogData = new DogData();
                }
                return LevelGame != null && LevelGame.selectedDog != null ? LevelGame.selectedDog.DogData : dogData;
            
            }
        }

        private void Awake()
        {
            m_buttonGenerate.onClick.AddListener(OnClickGenerate);
            m_buttonDelete.onClick.AddListener(OnClickDelete);
            foreach(var buttonColorPicked in m_listButtonColorPicked)
            {
                buttonColorPicked.OnClick = OnClickSelectColorPicked;
            }
            UpdateUI(DogData);
        }

        public void UpdateUI(DogData dogData)
        {
            var listColorData = LevelGame.GameLevelData.colorAndMaterialData.listColor;
            for (int i=0; i< m_listButtonColorPicked.Count; i++)
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
            foreach(var color in listColorData.listColor)
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

        private void OnClickDelete()
        {
            LevelGame.DeleteSelectedDog();
            LevelGame.ClearAllSelectedTiles();
        }

        private void OnClickGenerate()
        {
            var newDogData = new DogData(DogData);
            var newDog = LevelGame.GenerateNewDog(newDogData);
            LevelGame.SelectDog(newDog);
            UpdateUI(newDogData);
            LevelGame.ClearAllSelectedTiles();

        }

        private void OnClickSelectColorPicked(int index)
        {
            var listColorData = LevelGame.GameLevelData.colorAndMaterialData.listColor;

            if (DogData.listColor.Count <= index)
            {
                DogData.listColor.Add(ColorType.Red);
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
