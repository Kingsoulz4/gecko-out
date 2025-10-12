using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Dreamteck.Splines;
using Dreamteck;

namespace Geckout
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    //[AddComponentMenu("Dreamteck/Splines/Users/Tube Generator")]
    public class BodyVisualRenderer : TubeGenerator
    {
        [SerializeField] protected AnimationCurve m_bodyShapeCurve;

        protected override void Generate()
        {
            int vertexIndex = 0;
            ResetUVDistance();
            bool hasOffset = offset != Vector3.zero;
            for (int i = 0; i < sampleCount; i++)
            {
                GetSample(i, ref evalResult);
                Vector3 center = evalResult.position;
                Vector3 right = evalResult.right;
                float resultSize = GetBaseSize(evalResult) * m_bodyShapeCurve.Evaluate((float)i/sampleCount);
                if (hasOffset)
                {
                    center += (offset.x * resultSize) * right + (offset.y * resultSize) * evalResult.up + (offset.z * resultSize) * evalResult.forward;
                }
                if (uvMode == UVMode.UniformClamp || uvMode == UVMode.UniformClip)
                {
                    AddUVDistance(i);
                }
                Color vertexColor = GetBaseColor(evalResult) * color;
                for (int n = 0; n < _sides + 1; n++)
                {
                    float anglePercent = (float)(n) / _sides;
                    Quaternion rot = Quaternion.AngleAxis(_revolve * anglePercent + rotation + 180f, evalResult.forward);
                    _tsMesh.vertices[vertexIndex] = center + rot * right * (size * resultSize * 0.5f);
                    CalculateUVs(evalResult.percent, anglePercent);
                    _tsMesh.uv[vertexIndex] = Vector2.one * 0.5f + (Vector2)(Quaternion.AngleAxis(uvRotation + 180f, Vector3.forward) * (Vector2.one * 0.5f - (__uvs + Vector2.right * ((float)evalResult.percent * _uvTwist))));
                    _tsMesh.normals[vertexIndex] = Vector3.Normalize(_tsMesh.vertices[vertexIndex] - center);
                    _tsMesh.colors[vertexIndex] = vertexColor;
                    vertexIndex++;
                }
            }
            MeshUtility.GeneratePlaneTriangles(ref _tsMesh.triangles, _sides, sampleCount, false);
        }
    }
}
