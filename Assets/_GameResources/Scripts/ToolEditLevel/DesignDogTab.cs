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

        public LevelGame LevelGame { get; set; }

        private DogData dogData = new();

        private void Awake()
        {
            m_buttonGenerate.onClick.AddListener(OnClickGenerate);
            m_buttonDelete.onClick.AddListener(OnClickDelete);
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
    }
}
