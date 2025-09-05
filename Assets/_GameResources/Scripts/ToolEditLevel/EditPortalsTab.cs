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

        public LevelGame LevelGame { get; set; }

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
    }
}
