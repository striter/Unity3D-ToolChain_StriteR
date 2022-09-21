using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class UILineRenderer : MaskableGraphic
{
    [Clamp(0)] public float m_Width;
    public Vector2[] m_LocalPositions;
#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        SetAllDirty();
    }
#endif

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (m_LocalPositions.Length <= 1)
            return;

        List<UIVertex> vertices = new List<UIVertex>();
        List<int> indexes = new List<int>();

        var totalLength = 0f;

        var count = m_LocalPositions.Length;
        Vector2[] upDeltas = new Vector2[count - 1];
        for (int i = 0; i < count - 1; i++)
        {
            var forward = m_LocalPositions[i + 1] - m_LocalPositions[i];
            upDeltas[i] = new Vector2(-forward.y, forward.x);
        }


        var curIndex = 0;
        for (int i = 0; i < count - 1; i++)
        {
            var curPosition = m_LocalPositions[i];

            var upDelta = upDeltas[i];
            var upDirection = upDelta;
            var currentLength = upDelta.magnitude;
            upDirection = (upDirection.normalized + upDeltas[Mathf.Max(i - 1, 0)].normalized).normalized;

            vertices.Add(new UIVertex() { position = curPosition - upDirection * m_Width, uv0 = new Vector4(totalLength, 0), color = color });
            vertices.Add(new UIVertex() { position = curPosition + upDirection * m_Width, uv0 = new Vector4(totalLength, 1), color = color });
            totalLength += currentLength;

            indexes.Add(curIndex);
            indexes.Add(curIndex + 1);
            indexes.Add(curIndex + 2);

            indexes.Add(curIndex + 2);
            indexes.Add(curIndex + 1);
            indexes.Add(curIndex + 3);
            curIndex += 2;
        }

        var lastPoint = Enumerable.Last(m_LocalPositions);
        var lastUpDelta = Enumerable.Last(upDeltas).normalized;
        totalLength += lastUpDelta.magnitude;
        var lastUpDirection = lastUpDelta.normalized;
        vertices.Add(new UIVertex() { position = lastPoint - lastUpDirection * m_Width, uv0 = new Vector4(totalLength, 0), color = color });
        vertices.Add(new UIVertex() { position = lastPoint + lastUpDirection * m_Width, uv0 = new Vector4(totalLength, 1), color = color });

        vh.AddUIVertexStream(vertices, indexes);
    }
}
