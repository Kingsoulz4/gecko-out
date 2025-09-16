using Geckout.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Geckout
{
    public partial class LevelController : MonoBehaviour
    {
        [SerializeField] protected GameLevelData m_gameLevelData;
        [SerializeField] protected GameMap m_gameMap;
        [SerializeField] protected BodyController m_bodyPrefab;
        [SerializeField] protected Transform m_bodyParent;


        private List<BodyController> listBody = new();

        public GameLevelData GameLevelData => m_gameLevelData;

        public GameMap GameMap => m_gameMap;

        public List<BodyController> ListBody { get => listBody;}

        private void OnEnable()
        {
            LevelEvent.OnMoveToPortalDone += OnBodyMoveToPortal;
        }

        private void OnDisable()
        {
            LevelEvent.OnMoveToPortalDone -= OnBodyMoveToPortal;
        }

        public void SetLevelData(GameLevelData gameLevelData)
        {
            m_gameLevelData = gameLevelData;
            m_gameMap.SetLevelData(gameLevelData);
            Utils.RemoveAllChilds(m_bodyParent);
            int i=0;
            foreach (var bodyData in gameLevelData.listDogData)
            {
                var body = SpawnBody(bodyData);
                body.name = $"Body_{i}";
                i++;
                ListBody.Add(body);
            }
#if UNITY_EDITOR
            EditorUtility.SetDirty(this.gameObject);   
#endif
        }

        private void WinLevel()
        {
            LevelEvent.OnWin?.Invoke(m_gameLevelData.levelIndex);
        }

        private void LoseLevel()
        {
            LevelEvent.OnLose?.Invoke(m_gameLevelData.levelIndex);
        }

        private void OnBodyMoveToPortal(BodyController body, Portal portal)
        {
            if (listBody.Contains(body))
            {
                listBody.Remove(body);
            }

            if (listBody.Count == 0)
            {
                WinLevel();
            }
        }

        private void TimeOut()
        {
            LoseLevel();
        }

        public BodyController SpawnBody(BodyData bodyData)
        {
            var bodyPrefab = m_bodyPrefab;


#if UNITY_EDITOR
            var newBody = (BodyController)PrefabUtility.InstantiatePrefab(m_bodyPrefab, m_bodyParent);
#else
            var newBody = Instantiate(m_bodyPrefab, m_bodyParent);
#endif
            newBody.Initialize(bodyData);
            return newBody;
        }

    }
}
