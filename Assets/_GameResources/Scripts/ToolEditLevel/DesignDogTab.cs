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

        public LevelGame LevelGame { get; set; }

        private int currentSelectedColorIndex = 0;

        private DogData dogData = new();

        private void Awake()
        {
            m_buttonGenerate.onClick.AddListener(OnClickGenerate);
            m_buttonDelete.onClick.AddListener(OnClickDelete);
            foreach(var buttonColorPicked in m_listButtonColorPicked)
            {
                buttonColorPicked.OnClick = OnClickSelectColorPicked;
            }
        }

        public void UpdateUI(DogData dogData)
        {
            foreach (var itemColor in dogData.listColor)
            {

            }
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
                    dogData.listColor[currentSelectedColorIndex] = color.Key;
                });
            }
        }

        private void OnClickDelete()
        {
            LevelGame.DeleteSelectedDog();
        }

        private void OnClickGenerate()
        {
            dogData = new();
            LevelGame.GenerateNewDog(dogData);
        }

        private void OnClickSelectColorPicked(int index)
        {
            var listColorData = LevelGame.GameLevelData.colorAndMaterialData.listColor;

            if (dogData.listColor.Count <= index)
            {
                dogData.listColor.Add(ColorList.Red);
                m_listButtonColorPicked[index].SetEmpty(false);
                m_listButtonColorPicked[index].SetColor(listColorData[ColorList.Red]);
            }
            else
            {
                currentSelectedColorIndex = index;
            }
        }

    }
}
