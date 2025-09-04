using Geckout.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Geckout
{
    public class ToolEditLevelManager : MonoBehaviour
    {
        [SerializeField] private Button m_buttonDesign;
        [SerializeField] private Button m_buttonTest;
        [SerializeField] private Button m_buttonSaveLevel;
        [SerializeField] private Button m_buttonLoadLevel;
        [SerializeField] private TMP_InputField m_inputMapWidth;
        [SerializeField] private TMP_InputField m_inputMapHeight;
        [SerializeField] private TMP_InputField m_inputTime;
        [SerializeField] private TMP_Dropdown m_dropLevelType;
        [SerializeField] private TMP_InputField m_inputLevelNum;
        [SerializeField] private TMP_InputField m_inputLevelIndex;

        [SerializeField] private Button m_buttonEditWalls;
        [SerializeField] private Button m_buttonDesignDog;
        [SerializeField] private Button m_buttonEditPortals;
        [SerializeField] private Button m_buttonEditBoxes;

        [Header("Tabs")]
        [SerializeField] private EditWallsTab m_editWallsTab;
        [SerializeField] private DesignDogTab m_designDogTab;
        [SerializeField] private EditPortalsTab m_editPortalTab;
        [SerializeField] private EditBoxesTab m_editBoxesTab;

        [SerializeField] private LevelGame m_levelRootPrefab;

        public GameLevelData GameLevelData { get; set; }

        public LevelGame LevelGame { get; set; }

        private void Awake()
        {
            m_buttonDesign.onClick.AddListener(OnClickDesign);
            m_buttonLoadLevel.onClick.AddListener(OnClickLoadLevel);
            m_buttonSaveLevel.onClick.AddListener(OnClickSaveLevel);
            m_buttonTest.onClick.AddListener(OnClickTest);

            m_buttonEditWalls.onClick.AddListener(OnClickEditWalls);
            m_buttonDesignDog.onClick.AddListener(OnClickDesignDog);
            m_buttonEditPortals.onClick.AddListener(OnClickEditPortals);
            m_buttonEditBoxes.onClick.AddListener(OnClickEditBoxes);

            m_inputMapWidth.onSubmit.AddListener(OnEditedMapWidth);
            m_inputMapHeight.onSubmit.AddListener(OnEditedMapHeight);
        }

        private void OnEditedMapHeight(string arg0)
        {
            throw new NotImplementedException();
        }

        private void OnEditedMapWidth(string arg0)
        {
            throw new NotImplementedException();
        }

        private void OnClickEditBoxes()
        {
            HideAllTabs();
            m_editBoxesTab.gameObject.SetActive(true);
        }

        private void OnClickEditPortals()
        {
            HideAllTabs();
            m_editPortalTab.gameObject.SetActive(true);
        }

        private void OnClickDesignDog()
        {
            HideAllTabs();
            m_designDogTab.gameObject.SetActive(true);
            m_designDogTab.LevelGame = LevelGame;
            m_designDogTab.UpdateUI();
        }

        private void OnClickEditWalls()
        {
            HideAllTabs();
            m_editWallsTab.gameObject.SetActive(true);
            m_editWallsTab.LevelGame = LevelGame;
        }

        private void OnClickTest()
        {
            
        }

        private void OnClickSaveLevel()
        {
            
        }

        private void OnClickLoadLevel()
        {
            if(LevelGame == null)
            {
                LevelGame = Instantiate(m_levelRootPrefab);
            }

            var levelData = Resources.Load<GameLevelData>($"Levels/{m_inputLevelIndex.text}/Level{m_inputLevelNum.text}");
            
            
            if(levelData == null)
            {
                levelData = CreateNewLevelData(int.Parse(m_inputLevelNum.text), int.Parse(m_inputLevelIndex.text));
            }

            if (int.TryParse(m_inputMapWidth.text, out var width))
            {
                if (int.TryParse(m_inputMapHeight.text, out var height))
                {
                    levelData.mapSize = new Vector2Int(width, height);
                }
            }

            levelData.type = (LevelType)m_dropLevelType.value;

            if(int.TryParse(m_inputTime.text, out var time))
            {
                levelData.time = time;
            }

            LevelGame.SetLevelData(levelData);
        }

        private GameLevelData CreateNewLevelData(int level, int index)
        {
            // Create an instance of MyScriptableObject
            var newLevelData = ScriptableObject.CreateInstance<GameLevelData>();

            // Assign values to its properties
            newLevelData.name = $"Level{level}";
            newLevelData.levelNum = level;
            newLevelData.levelIndex = index;

            if (int.TryParse(m_inputMapWidth.text, out var width))
            {
                if (int.TryParse(m_inputMapHeight.text, out var height))
                {
                    newLevelData.mapSize = new Vector2Int(width, height);
                }
            }

            newLevelData.GenerateDefaultMap();

            // Define the path where the asset will be saved
            string path = $"Assets/_GameResources/Resources/Levels/{index}/Level{level}.asset";

            // Create the asset in the Project window
            AssetDatabase.CreateAsset(newLevelData, path);

            // Save and refresh the AssetDatabase to ensure the new asset is visible
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Optionally, select the newly created asset in the Project window
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = newLevelData;

            Debug.Log("MyScriptableObject created at: " + path);
        
            return newLevelData;
        }

        private void OnClickDesign()
        {

        }

        private void HideAllTabs()
        {
            m_editWallsTab.gameObject.SetActive(false);
            m_designDogTab.gameObject.SetActive(false);
            m_editPortalTab.gameObject.SetActive(false);
            m_editBoxesTab.gameObject.SetActive(false);
        }
    }
}
