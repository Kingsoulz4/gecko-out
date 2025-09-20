using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        public static Action<BoosterType> OnShowConfirmUIBooster;
        public static Action<BoosterType> OnHideConfirmUIBooster;
    }


}
