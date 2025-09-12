using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public class LevelEvent : SingletonMono<LevelEvent>
    {
        public Action<BodyController,Portal> OnMoveToPortalStart;
        public Action<BodyController, Portal> OnMoveToPortalDone;
        public Action<int> OnWin;
        public Action<int> OnLose;
        public Action<int> OnLevelStart;
    }
}
