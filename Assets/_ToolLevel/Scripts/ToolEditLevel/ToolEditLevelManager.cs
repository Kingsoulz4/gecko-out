using Geckout.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
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

        [SerializeField] private LevelGameEditTool m_levelRootPrefab;

        public GameLevelData GameLevelData { get; set; }

        public LevelGameEditTool LevelGame { get; set; }

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

            HideAllTabs();

            LevelManager.Instance.IsEdittingLevel = true;
            Utils.SetExistingGameViewSize(1920, 1080);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                OnPressDelete();
            }
        }

        private void OnPressDelete()
        {
            LevelGame.DeleteSelectedBody();
            LevelGame.ChangeTypeSelectedTiles(MapTileType.Normal);
            HideAllTabs();
        }

        private void OnEditedMapHeight(string arg0)
        {
        }

        private void OnEditedMapWidth(string arg0)
        {
        }

        public void OnClickEditBoxes()
        {
            HideAllTabs();
            m_editBoxesTab.LevelGame = LevelGame;
            m_buttonEditBoxes.GetComponent<ButtonToolTab>().SetSelected(true);
            m_editBoxesTab.gameObject.SetActive(true);
        }

        public void OnClickEditPortals()
        {
            HideAllTabs();
            
            m_buttonEditPortals.GetComponent<ButtonToolTab>().SetSelected(true);
            m_editPortalTab.gameObject.SetActive(true);
            m_editPortalTab.LevelGame = LevelGame;
            if (LevelGame != null && LevelGame.selectedPortal != null)
            {
                m_editPortalTab.UpdateUI(LevelGame.selectedPortal.PortalData);
            }
            else
            {
                m_editPortalTab.UpdateUI();
            }
        }

        public void OnClickDesignDog()
        {
            HideAllTabs();
            m_buttonDesignDog.GetComponent<ButtonToolTab>().SetSelected(true);
            m_designDogTab.gameObject.SetActive(true);
            m_designDogTab.LevelGame = LevelGame;
            if (LevelGame != null && LevelGame.selectedBody != null)
            {
                LevelGame.ClearAllSelectedTiles();
                m_designDogTab.UpdateUI(LevelGame.selectedBody.BodyData);
            }
            else
            {
                m_designDogTab.UpdateUI();
            }
        }

        public void OnClickEditWalls()
        {
            HideAllTabs();
            m_buttonEditWalls.GetComponent<ButtonToolTab>().SetSelected(true);
            m_editWallsTab.gameObject.SetActive(true);
            m_editWallsTab.LevelGame = LevelGame;
        }

        private void OnClickTest()
        {
            LevelManager.Instance.IsEdittingLevel = false;
            if (LevelGame != null)
            {
                LevelGame.ClearAllSelected();
            }
            Utils.SetExistingGameViewSize(1080, 1920);
            SceneManager.LoadScene("GameSceneTestLevel");
            LevelManager.Instance.CurrentLevelNum = int.Parse(m_inputLevelNum.text);
            LevelManager.Instance.CurrentLevelIndex = int.Parse(m_inputLevelIndex.text);
           
        }

        private void OnClickSaveLevel()
        {
#if UNITY_EDITOR
            Undo.RecordObject(LevelGame.GameLevelData, LevelGame.GameLevelData.name);
            EditorUtility.SetDirty(LevelGame.GameLevelData);
            EditorApplication.ExecuteMenuItem("File/Save Project");
#endif
        }

        private void OnClickLoadLevel()
        {
            if(LevelGame == null)
            {
                LevelGame = Instantiate(m_levelRootPrefab);
            }

            LevelManager.Instance.LevelGame = LevelGame;

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
            GameLevelData newLevelData = new();

#if UNITY_EDITOR
            newLevelData = ScriptableObject.CreateInstance<GameLevelData>();

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
#endif

            return newLevelData;
        }

        private void OnClickDesign()
        {
            LevelManager.Instance.IsEdittingLevel = true;
        }

        private void HideAllTabs()
        {
            m_editWallsTab.gameObject.SetActive(false);
            m_designDogTab.gameObject.SetActive(false);
            m_editPortalTab.gameObject.SetActive(false);
            m_editBoxesTab.gameObject.SetActive(false);

            m_buttonEditWalls.GetComponent<ButtonToolTab>().SetSelected(false);
            m_buttonEditPortals.GetComponent<ButtonToolTab>().SetSelected(false);   
            m_buttonEditBoxes.GetComponent<ButtonToolTab>().SetSelected(false);
            m_buttonDesignDog.GetComponent<ButtonToolTab>().SetSelected(false);

            
        }
    }
}
