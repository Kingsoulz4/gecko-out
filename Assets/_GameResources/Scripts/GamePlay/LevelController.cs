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
        [SerializeField] protected BodyController m_dogPrefab;
        [SerializeField] protected Transform m_listDogContainer;

        private List<BodyController> listBody = new();

        public GameLevelData GameLevelData => m_gameLevelData;

        public GameMap GameMap => m_gameMap;

        public List<BodyController> ListBody { get => listBody;}

        public void SetLevelData(GameLevelData gameLevelData)
        {
            m_gameLevelData = gameLevelData;
            m_gameMap.SetLevelData(gameLevelData);
            Utils.RemoveAllChilds(m_listDogContainer);
            foreach(var dogData in gameLevelData.listDogData)
            {
                ListBody.Add(SpawnDog(dogData));
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

        private void Update()
        {
            
        }

        public BodyController SpawnDog(DogData dogData)
        {

#if UNITY_EDITOR
            var newDog = (BodyController)PrefabUtility.InstantiatePrefab(m_dogPrefab, m_listDogContainer);
#else
            var newDog = Instantiate(m_dogPrefab, m_listDogContainer);
#endif
            newDog.Initialize(dogData);
            return newDog;
        }

    }
}
