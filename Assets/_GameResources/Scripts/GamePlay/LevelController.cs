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

        public void SetLevelData(GameLevelData gameLevelData)
        {
            m_gameLevelData = gameLevelData;
            m_gameMap.SetLevelData(gameLevelData);
            Utils.RemoveAllChilds(m_bodyParent);
            foreach(var dogData in gameLevelData.listDogData)
            {
                ListBody.Add(SpawnBody(dogData));
            }
#if UNITY_EDITOR
            EditorUtility.SetDirty(this.gameObject);   
#endif
        }

        public void OnBodyMoveToHole(BodyController body)
        {
            if (listBody.Contains(body))
            {
                listBody.Remove(body);
            }
        }

        public BodyController SpawnBody(BodyData bodyData)
        {

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
