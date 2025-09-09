using System;
using System.Collections;
using System.Collections.Generic;
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

        public LevelGameEditTool LevelGame { get; set; }

        private void Awake()
        {
            //m_buttonGenerate.onClick.AddListener(OnClickGenerate);
        }

        private void OnClickGenerate()
        {
            LevelGame.GameMap.SpawnBoxMove(new Data.MovableBoxData() { 
                boxSize = new Vector2Int(3, 3)
            });
        }
    }
}
