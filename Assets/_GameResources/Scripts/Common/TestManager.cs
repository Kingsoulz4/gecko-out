using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Geckout
{
    public class TestManager : SingletonMono<TestManager>
    {
        [SerializeField] Button btn_Load;
        [SerializeField] Button btn_nextLevel;
        [SerializeField] Button btn_backLevel;
        [SerializeField] InputField inputField;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                btn_backLevel.onClick.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                btn_backLevel.onClick.Invoke();
            }
        }

        private void Start()
        {
            btn_Load.onClick.AddListener(LoadLevel);
            btn_backLevel.onClick.AddListener(() =>
            {
                UserDataManager.AddHeart(1, "test", false);
                LevelManager.Instance.StartLevel(LevelManager.Instance.CurrentLevel -= 1);
            });
            btn_nextLevel.onClick.AddListener(() =>
            {
                UserDataManager.AddHeart(1, "test", false);
                LevelManager.Instance.StartLevel(LevelManager.Instance.CurrentLevel += 1);
            });
        }

        private void LoadLevel()
        {
            if (string.IsNullOrEmpty(inputField.text))
            {
                return;
            }

            if (int.Parse(inputField.text) == -1)
            {
                AddRes();
                return;
            }

            LevelManager.Instance.CurrentLevel = int.Parse(inputField.text);
            LevelManager.Instance.StartCurrentLevel();
        }

        private void AddRes()
        {
            UserDataManager.AddGold(1000, "test", false);
            UserDataManager.AddHeart(5, "test", false);
        }
    }
}
