#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Geckout
{
    public static class MyTool 
    {

        [MenuItem("Tools/Tool Edit Level")]
        static void OpenToolEditLevel()
        {
            if (Application.isPlaying)
            {
                SceneManager.LoadScene("ToolEditLevel");
            }
            else
            {
                EditorSceneManager.OpenScene("Assets/_ToolLevel/Scenes/ToolEditLevel.unity");
            }
        }

    }
}
#endif
