using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public class Utils
    {
        public static void RemoveAllChilds(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    GameObject.DestroyImmediate(parent.GetChild(i).gameObject);
                else
                    GameObject.DestroyImmediate(parent.GetChild(i).gameObject);
#else
            GameObject.DestroyImmediate(parent.GetChild(i).gameObject);
#endif
            }
        }
    }
}
