using AYellowpaper.SerializedCollections;
using Geckout.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Geckout
{
    public class EditWallsTab : MonoBehaviour
    {
        [SerializeField] private SerializedDictionary<MapTileType, Button> m_listButtonChangeType;

        [SerializeField] private Button m_buttonRotateLeft;
        [SerializeField] private Button m_buttonRotateRight;
        [SerializeField] private Button m_buttonDelete;

        public LevelGame LevelGame { get; set; }

        private void Awake()
        {
            foreach(var item in m_listButtonChangeType)
            {
                item.Value.onClick.AddListener(() =>
                {
                    ChangeToTileType(item.Key);
                });
            }

            m_buttonRotateLeft.onClick.AddListener(OnClickRotateLeft);
            m_buttonRotateRight.onClick.AddListener(OnClickRotateRight);
            m_buttonDelete.onClick.AddListener(OnClickDelete);
        }

        private void OnClickDelete()
        {
            LevelGame.ChangeTypeSelectedTiles(MapTileType.Normal);
        }

        private void OnClickRotateRight()
        {
            LevelGame.RotateSelectedTiles(-90);
        }

        private void OnClickRotateLeft()
        {
            LevelGame.RotateSelectedTiles(90);
        }

        private void ChangeToTileType(MapTileType tileType)
        {
            LevelGame.ChangeTypeSelectedTiles(tileType);
        }

    }
}
