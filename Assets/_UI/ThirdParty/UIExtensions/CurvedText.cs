using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class CurvedText : Text
{
    public float radius = 0.5f;
    public float scaleFactor = 100.0f;
    public Slider.Direction direction;

    private float circumference
    {
        get
        {
            if (_radius != radius || _scaleFactor != scaleFactor)
            {
                _circumference = 2.0f * Mathf.PI * radius * scaleFactor;
                _radius = radius;
                _scaleFactor = scaleFactor;
            }

            return _circumference;
        }
    }

    private float _radius = -1;
    private float _scaleFactor = -1;
    private float _circumference = -1;
#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        if (radius <= 0.0f)
        {
            radius = 0.001f;
        }

        if (scaleFactor <= 0.0f)
        {
            scaleFactor = 0.001f;
        }
    }
#endif
   

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        base.OnPopulateMesh(vh);

        List<UIVertex> stream = new List<UIVertex>();

        vh.GetUIVertexStream(stream);

        for (int i = 0; i < stream.Count; i++)
        {
            UIVertex v = stream[i];
            if (direction == Slider.Direction.LeftToRight)
            {
                float percentCircumference = v.position.x / circumference;
                Vector3 offset = Quaternion.Euler(0.0f, 0.0f, -percentCircumference * 360.0f) * Vector3.up;
                v.position = offset * -radius * scaleFactor + offset * v.position.y;
                v.position -= Vector3.down * radius * scaleFactor;
                v.position.x *= -1;
            }
            else
            {
                float percentCircumference = v.position.x / circumference;
                Vector3 offset = Quaternion.Euler(0.0f, 0.0f, -percentCircumference * 360.0f) * Vector3.up;
                v.position = offset * radius * scaleFactor + offset * v.position.y;
                v.position += Vector3.down * radius * scaleFactor;

            }
            
            stream[i] = v;
        }

        vh.AddUIVertexTriangleStream(stream);
    }
}