using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

namespace Geckout
{
    public class LevelEvent : SingletonMono<LevelEvent>
    {
        public static Action<BodyController, Portal> OnMoveToPortalStart;
        public static Action<BodyController, Portal> OnMoveToPortalDone;
        public static Action<BodyController, List<Vector2Int>> OnGetLastPath;
        public static Action<int> OnWin;
        public static Action<int> OnLose;
        public static Action<int> OnLevelStart;
        //public static Action<int> 

        private void OnDestroy()
        {
            OnMoveToPortalStart = null;
            OnMoveToPortalDone = null;
            OnWin = null;
            OnLose = null;
            OnLevelStart = null;
            OnGetLastPath = null;
        }
    }


}
