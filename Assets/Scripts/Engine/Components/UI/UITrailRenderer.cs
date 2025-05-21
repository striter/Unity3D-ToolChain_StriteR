using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode,RequireComponent(typeof(CanvasRenderer))]
public class UITrailRenderer : MaskableGraphic
{
    struct TrailPath
    {
        public float time;
        public Vector3 position;
    }
    
    public float m_Time = 5f;
    public AnimationCurve m_Width;
    public float m_MinVertexDistance = .1f;
    public bool m_Emitting = true;
    public Gradient m_Gradient = new Gradient();
    
    private float kMinVertexDistanceSqr = 0f;
    private List<TrailPath> m_TrailPaths = new List<TrailPath>();
    private float m_TimeElapsed;
    private Vector3 m_LastPosition;
    
    protected override void Awake()
    {
        base.Awake();
        Initialize();
        Clear();
    }

    void Initialize()
    {
        kMinVertexDistanceSqr = m_MinVertexDistance * m_MinVertexDistance;
    }
    
    protected override void UpdateMaterial()
    {
        if (!IsActive())
            return;

        canvasRenderer.materialCount = 1;
        canvasRenderer.SetMaterial(materialForRendering, 0);
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        Initialize();
    }
#endif
    
    public void Clear()
    {
        m_TimeElapsed = 0f;
        m_TrailPaths.Clear();
        EnqueuePosition();
        SetVerticesDirty();
    }

    void EnqueuePosition()
    {
        m_LastPosition = transform.position;
        m_TrailPaths.Add(new TrailPath(){time = m_TimeElapsed,position = m_LastPosition });
    }

    private void Update()
    {
        float deltaTime = UTime.deltaTime;
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
        
        if(m_TrailPaths.Count==0 || (m_TrailPaths[m_TrailPaths.Count-1].position.XY() - transform.position.XY()).sqrMagnitude > kMinVertexDistanceSqr)
            EnqueuePosition();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (m_TrailPaths.Count<=1)
            return;

        var worldToObject = transform.worldToLocalMatrix;
        var objectToWorld = transform.localToWorldMatrix;
        var vertices = new List<UIVertex>();
        var indexes = new List<int>();
        
        var count = m_TrailPaths.Count;
        var forwardDirections = new Vector2[count - 1];
        for (var i = 0; i < count - 1; i++)
            forwardDirections[i] = (m_TrailPaths[i + 1].position - m_TrailPaths[i].position).normalized;
        
        var curIndex = 0;

        var startForward = forwardDirections[0];
        var startUpward = new Vector3(-startForward.y, startForward.x,0f);
        startUpward = objectToWorld.MultiplyVector(startUpward);
        var startPosition = m_TrailPaths[0].position;
        var startWidth = m_Width.Evaluate(1f);
        var startColor = m_Gradient.Evaluate(1f);
        vertices.Add(new UIVertex(){position = worldToObject.MultiplyPoint(startPosition+startUpward*startWidth),uv0 = new Vector4(1,0),color = color*startColor});
        vertices.Add(new UIVertex(){position = worldToObject.MultiplyPoint(startPosition-startUpward*startWidth),uv0 = new Vector4(1,1),color = color*startColor});
        
        for (var i = 1; i < count - 1; i++)
        {
            var curPosition = m_TrailPaths[i].position;
            var curEvaluation = (float) i / count;
            var curU = 1f - curEvaluation;
            var curColor = m_Gradient.Evaluate(curEvaluation);
            
            var forward0 =  forwardDirections[i-1];
            var forward1 = forwardDirections[i];
            var upward0 = new Vector2(-forward0.y, forward0.x);
            var upward1 = new Vector2(-forward1.y, forward1.x);
            // var clockwise = Vector2.Dot(upward1, forward0) > 0;
            var cornerDirection = ((upward0 + upward1) / 2).ToVector3_XY(0f) ;
            cornerDirection = objectToWorld.MultiplyVector(cornerDirection);
            cornerDirection *= m_Width.Evaluate((m_TimeElapsed-m_TrailPaths[i].time)/m_Time);
            var pointT = curPosition + cornerDirection;
            var pointD = curPosition - cornerDirection;

            vertices.Add(new UIVertex(){position = worldToObject.MultiplyPoint( pointT),uv0 = new Vector4(curU,0),color = color*curColor});
            vertices.Add(new UIVertex(){position = worldToObject.MultiplyPoint( pointD),uv0 = new Vector4(curU,1),color = color*curColor});
            
            indexes.Add(curIndex);
            indexes.Add(curIndex + 2);
            indexes.Add(curIndex + 1);

            indexes.Add(curIndex + 2);
            indexes.Add(curIndex + 3);
            indexes.Add(curIndex + 1);
            
            curIndex += 2;
        }

        var finalPoint = transform.position;
        var lastForwardDirection = forwardDirections[forwardDirections.Length-1];
        var lastUpDirection = (new Vector2(-lastForwardDirection.y,lastForwardDirection.x).normalized).ToVector3_XY();
        lastUpDirection = objectToWorld.MultiplyVector(lastUpDirection);
        var lastWidth = m_Width.Evaluate(0f);
        var lastColor = m_Gradient.Evaluate(0f);
        vertices.Add(new UIVertex(){position = worldToObject.MultiplyPoint( finalPoint + lastUpDirection * lastWidth),uv0 = new Vector4(0,0),color = color*lastColor});
        vertices.Add(new UIVertex(){position = worldToObject.MultiplyPoint( finalPoint - lastUpDirection * lastWidth),uv0 = new Vector4(0,1),color = color*lastColor});

        indexes.Add(curIndex);
        indexes.Add(curIndex + 2);
        indexes.Add(curIndex + 1);

        indexes.Add(curIndex + 2);
        indexes.Add(curIndex + 3);
        indexes.Add(curIndex + 1);
        
        vh.AddUIVertexStream(vertices,indexes);
        
    }
}
