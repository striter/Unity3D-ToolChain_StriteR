using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode,RequireComponent(typeof(CanvasRenderer))]
public class UITrailRenderer : MaskableGraphic
{
    struct TrailPath
    {
        public float time;
        public Vector2 position;
    }
    
    public float m_Time = 5f;
    public AnimationCurve m_Width;
    public float m_MinVertexDistance = .1f;
    public bool m_Emitting = true;
    
    private float kMinVertexDistanceSqr = 0f;
    private List<TrailPath> m_TrailPaths = new List<TrailPath>();
    private float m_TimeElapsed;
    private Vector2 m_LastPosition;
    
    protected override void Awake()
    {
        base.Awake();
        OnValidate();
        Clear();
    }
    
    protected override void UpdateMaterial()
    {
        if (!IsActive())
            return;

        canvasRenderer.materialCount = 1;
        canvasRenderer.SetMaterial(materialForRendering, 0);
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        SetVerticesDirty();
        kMinVertexDistanceSqr = m_MinVertexDistance * m_MinVertexDistance;
    }

    public void Clear()
    {
        m_TimeElapsed = 0f;
        m_TrailPaths.Clear();
        EnqueuePosition();
        SetVerticesDirty();
    }

    void EnqueuePosition()
    {
        m_LastPosition = transform.position.XY();
        m_TrailPaths.Add(new TrailPath(){time = m_TimeElapsed,position = m_LastPosition });
    }

    float GetDeltaTime()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            return UEditorTime.deltaTime;
#endif
        return Time.deltaTime;
    }
    
    private void Update()
    {
        float deltaTime = GetDeltaTime();
        m_TimeElapsed += deltaTime;
        SetVerticesDirty();

        int dequeueCount = 0;
        foreach (var trailPath in m_TrailPaths)
        {
            if (m_TimeElapsed - trailPath.time > m_Time)
            {
                dequeueCount += 1;
                continue;                
            }
            break;
        }

        for (int i = 0; i < dequeueCount; i++)
            m_TrailPaths.RemoveAt(0);

        if (!m_Emitting)
            return;
        
        if(m_TrailPaths.Count==0 || (m_TrailPaths[^1].position - transform.position.XY()).sqrMagnitude > kMinVertexDistanceSqr)
            EnqueuePosition();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (m_TrailPaths is not {Count: > 1})
            return;

        var worldToObject = transform.worldToLocalMatrix;
        List<UIVertex> vertices = new List<UIVertex>();
        List<int> indexes = new List<int>();
        
        var count = m_TrailPaths.Count;
        Vector2[] forwardDirections = new Vector2[count - 1];
        for (int i = 0; i < count - 1; i++)
            forwardDirections[i] = (m_TrailPaths[i + 1].position - m_TrailPaths[i].position).normalized;
        
        var curIndex = 0;

        var startForward = forwardDirections[0];
        var startUpward = new Vector2(-startForward.y, startForward.x);
        var startPosition = m_TrailPaths[0].position;
        var startWidth = m_Width.Evaluate(1f);
        vertices.Add(new UIVertex(){position = worldToObject.MultiplyPoint(startPosition+startUpward*startWidth),uv0 = new Vector4(1,0),color = color});
        vertices.Add(new UIVertex(){position = worldToObject.MultiplyPoint(startPosition-startUpward*startWidth),uv0 = new Vector4(1,1),color = color});
        
        for (int i = 1; i < count - 1; i++)
        {
            var curPosition = m_TrailPaths[i].position;
            var curU = 1f- ((float) i / count);
            var forward0 =  forwardDirections[i-1];
            var forward1 = forwardDirections[i];
            var upward0 = new Vector2(-forward0.y, forward0.x);
            var upward1 = new Vector2(-forward1.y, forward1.x);
            // var clockwise = Vector2.Dot(upward1, forward0) > 0;
            var cornerDirection = (upward0 + upward1) / 2 ;
            cornerDirection *= m_Width.Evaluate((m_TimeElapsed-m_TrailPaths[i].time)/m_Time);
            var pointT = curPosition + cornerDirection;
            var pointD = curPosition - cornerDirection;

            vertices.Add(new UIVertex(){position = worldToObject.MultiplyPoint( pointT),uv0 = new Vector4(curU,0),color = color});
            vertices.Add(new UIVertex(){position = worldToObject.MultiplyPoint( pointD),uv0 = new Vector4(curU,1),color = color});
            
            indexes.Add(curIndex);
            indexes.Add(curIndex + 2);
            indexes.Add(curIndex + 1);

            indexes.Add(curIndex + 2);
            indexes.Add(curIndex + 3);
            indexes.Add(curIndex + 1);
            
            curIndex += 2;
        }

        var finalPoint = transform.position.XY();
        var lastForwardDirection = forwardDirections[^1];
        var lastUpDirection = new Vector2(-lastForwardDirection.y,lastForwardDirection.x).normalized;
        var lastWidth = m_Width.Evaluate(0f);
        vertices.Add(new UIVertex(){position = worldToObject.MultiplyPoint( finalPoint + lastUpDirection * lastWidth),uv0 = new Vector4(0,0),color = color});
        vertices.Add(new UIVertex(){position = worldToObject.MultiplyPoint( finalPoint - lastUpDirection * lastWidth),uv0 = new Vector4(0,1),color = color});

        indexes.Add(curIndex);
        indexes.Add(curIndex + 2);
        indexes.Add(curIndex + 1);

        indexes.Add(curIndex + 2);
        indexes.Add(curIndex + 3);
        indexes.Add(curIndex + 1);
        
        vh.AddUIVertexStream(vertices,indexes);
        
    }
}
