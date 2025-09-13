using Geckout.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
        [SerializeField] private TMP_InputField m_inputFreezeCount;
        [SerializeField] private TMP_InputField m_inputHiddenCount;

        public LevelGameEditTool LevelGame { get; set; }

        private int currentSelectedColorIndex = 0;

        private BodyData bodyData;

        private BodyData BodyData
        {
            get
            {
                if (bodyData == null)
                {
                    bodyData = new BodyData();
                }
                return LevelGame != null && LevelGame.selectedBody != null ? LevelGame.selectedBody.BodyData : bodyData;
            
            }
        }

        private void Awake()
        {
            m_buttonGenerate.onClick.AddListener(OnClickGenerate);
            m_buttonDelete.onClick.AddListener(OnClickDelete);
            foreach(var buttonColorPicked in m_listButtonColorPicked)
            {
                buttonColorPicked.OnClick.AddListener(OnClickSelectColorPicked);
            }
            m_inputFreezeCount.onSubmit.AddListener(OnEnterFreezeCount);
            m_inputHiddenCount.onSubmit.AddListener(OnEnterHiddenCount);
            //UpdateUI(DogData);
        }

        private void OnEnterHiddenCount(string arg0)
        {
            BodyData.hiddenCount = int.Parse(arg0);
            if (LevelGame != null && LevelGame.selectedBody != null)
            {
                LevelGame.selectedBody.InitMechanic();
            }
        }

        private void OnEnterFreezeCount(string arg0)
        {
            BodyData.freezeTimeCount = int.Parse(arg0);
            if(LevelGame != null && LevelGame.selectedBody != null)
            {
                LevelGame.selectedBody.InitMechanic();
            }
        }

        public void UpdateUI(BodyData dogData)
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
                    if (currentSelectedColorIndex >= BodyData.listColor.Count)
                    {
                        BodyData.listColor.Add(color.Key);
                    }
                    else
                    {
                        BodyData.listColor[currentSelectedColorIndex] = color.Key;
                    }

                    if (LevelGame.selectedBody != null)
                    {
                        LevelGame.selectedBody.UpdateColor();
                    }
                        
                });
            }

            m_inputFreezeCount.text = BodyData.freezeTimeCount.ToString();
        }

        private void OnClickDelete()
        {
            LevelGame.DeleteSelectedBody();
            LevelGame.ClearAllSelectedTiles();
        }

        private void OnClickGenerate()
        {
            var newDogData = new BodyData(BodyData);
            var newDog = LevelGame.GenerateNewBody(newDogData);
            LevelGame.SelectDog(newDog);
            UpdateUI(newDogData);
            LevelGame.ClearAllSelectedTiles();

        }

        private void OnClickSelectColorPicked(int index)
        {
            var listColorData = LevelGame.GameLevelData.colorAndMaterialData.listColor;

            if (BodyData.listColor.Count <= index)
            {
                BodyData.listColor.Add(ColorType.Red);
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
