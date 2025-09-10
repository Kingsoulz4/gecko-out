using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    using UnityEngine;

    [ExecuteInEditMode]
    public class MeshCombiner : MonoBehaviour
    {
        [ContextMenu("Combine Meshes (Tight)")]
        public void CombineMeshesTight()
        {
            MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
            if (meshFilters.Length == 0)
            {
                Debug.LogWarning("No MeshFilters found under " + gameObject.name);
                return;
            }

            CombineInstance[] combine = new CombineInstance[meshFilters.Length];
            Material sharedMat = null;

            // Use parent as reference to "zero out" transforms
            Matrix4x4 parentMatrix = transform.worldToLocalMatrix;

            for (int i = 0; i < meshFilters.Length; i++)
            {
                MeshFilter mf = meshFilters[i];
                if (mf.sharedMesh == null) continue;

                combine[i].mesh = mf.sharedMesh;

                // Instead of using full worldToLocal, rebase relative to parent
                combine[i].transform = parentMatrix * mf.transform.localToWorldMatrix;

                if (sharedMat == null && mf.TryGetComponent(out Renderer renderer))
                    sharedMat = renderer.sharedMaterial;
            }

            Mesh combinedMesh = new Mesh();
            combinedMesh.name = "CombinedMesh";
            combinedMesh.CombineMeshes(combine, true, true); // merge submeshes & apply transforms

            // Clean old combined if exists
            Transform old = transform.Find("CombinedMeshObj");
            if (old != null) DestroyImmediate(old.gameObject);

            GameObject combinedObj = new GameObject("CombinedMeshObj", typeof(MeshFilter), typeof(MeshRenderer));
            combinedObj.transform.SetParent(transform, false);

            combinedObj.GetComponent<MeshFilter>().sharedMesh = combinedMesh;
            combinedObj.GetComponent<MeshRenderer>().sharedMaterial = sharedMat;

            Debug.Log("Meshes combined tightly into " + combinedObj.name);
        }
    }

}
