using System;
using System.Linq;
using Geometry.Bezier;
using Geometry.Voxel;
using TPoolStatic;
using UnityEngine;

public enum ERopePosition
{
    Constant,
    Transform,
}

[ExecuteInEditMode,RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
public class RopeRenderer : MonoBehaviour
{
    [Clamp(0)] public float m_Extend = 3;
    [Clamp(0)] public float m_Width = 0.1f;
    public ERopePosition m_RopePosition = ERopePosition.Constant;

    [MFoldout(nameof(m_RopePosition), ERopePosition.Transform)] public Transform m_EndTransform;
    
    [MFoldout(nameof(m_RopePosition), ERopePosition.Constant)] public Vector3 m_EndPosition;
    [MFoldout(nameof(m_RopePosition), ERopePosition.Constant)] public Vector3 m_EndBiTangent;

    public Damper m_ControlDamper;
    private FBezierCurveQuadratic m_Curve;
    private Mesh m_Mesh;
    private void Awake()
    {
        m_Mesh =new Mesh {name = "Rope (Temp)",hideFlags = HideFlags.HideAndDontSave};
        m_Mesh.MarkDynamic();
        GetComponent<MeshFilter>().sharedMesh = m_Mesh;
        CalculatePositions(out Vector3 srcPosition,out Vector3 srcBiTangent,out Vector3 dstPosition,out Vector3 dstBiTangent,out Vector3 control);
        m_ControlDamper.Begin(control);
    }

    private void OnDestroy()
    {
        GameObject.DestroyImmediate(m_Mesh);
    }

    void CalculatePositions(out Vector3 srcPosition,out Vector3 srcBiTangent,out Vector3 dstPosition,out Vector3 dstBiTangent,out Vector3 control)
    {
        srcPosition = transform.position;
        srcBiTangent = transform.up;
        dstPosition = default;
        dstBiTangent = default; 
        
        switch (m_RopePosition)
        {
            case ERopePosition.Constant: {
                dstPosition = m_EndPosition;
                dstBiTangent = m_EndBiTangent;
            } break;
            case ERopePosition.Transform: {
                if (!m_EndTransform)
                    break;
                dstPosition = m_EndTransform.position;
                dstBiTangent = m_EndTransform.up;
            } break;
        }

        control = (dstPosition + srcPosition) / 2 + Vector3.down * m_Extend;
    }

    private void Update()
    {
        CalculatePositions(out Vector3 srcPosition,out Vector3 srcBiTangent,out Vector3 dstPosition,out Vector3 dstBiTangent,out Vector3 control);
        control = m_ControlDamper.Tick(UTime.deltaTime, control);
        m_Curve = new FBezierCurveQuadratic(srcPosition, dstPosition, control);
        Matrix4x4 worldToLocal = transform.worldToLocalMatrix;

        TSPoolList<Vector3>.Spawn(out var ropePositions);
        TSPoolList<Vector3>.Spawn(out var ropeNormals);

        for (int i = 0; i < 64; i++)
        {
            var evaluate = (float)i/63;
            ropePositions.Add(m_Curve.Evaluate(evaluate));
            var tangent = m_Curve.GetTangent(evaluate);
            var biTangent = Vector3.Lerp(srcBiTangent,dstBiTangent,evaluate);
            ropeNormals.Add(Vector3.Cross(tangent,biTangent));
        }

        TSPoolList<Vector3>.Spawn(out var vertices);
        TSPoolList<Vector4>.Spawn(out var uvs);
        TSPoolList<int>.Spawn(out var indexes);

        var totalLength = 0f;

        var count = ropePositions.Count;
        var curIndex = 0;
        for (int i = 0; i < count - 1; i++)
        {
            var position = ropePositions[i];
            var normal = ropeNormals[i];
            vertices.Add( worldToLocal.MultiplyPoint(position - normal * m_Width));
            uvs.Add(new Vector4(totalLength, 0));
            vertices.Add( worldToLocal.MultiplyPoint(position + normal * m_Width));
            uvs.Add(new Vector4(totalLength, 1));
            
            totalLength += (ropePositions[i]-ropePositions[i+1]).magnitude;

            indexes.Add(curIndex);
            indexes.Add(curIndex + 1);
            indexes.Add(curIndex + 2);

            indexes.Add(curIndex + 2);
            indexes.Add(curIndex + 1);
            indexes.Add(curIndex + 3);
            curIndex += 2;
        }

        var lastPoint = ropePositions.Last();
        var lastUpDelta = ropeNormals.Last().normalized;
        totalLength += lastUpDelta.magnitude;
        var lastNormal = lastUpDelta.normalized;
        vertices.Add(worldToLocal.MultiplyPoint(lastPoint - lastNormal * m_Width));
        uvs.Add( new Vector4(totalLength, 0));
        vertices.Add(worldToLocal.MultiplyPoint(lastPoint + lastNormal * m_Width));
        uvs.Add(new Vector4(totalLength, 1));
        
        m_Mesh.SetVertices(vertices);
        m_Mesh.SetUVs(0,uvs);
        m_Mesh.SetTriangles(indexes,0);
        m_Mesh.RecalculateBounds();
        m_Mesh.RecalculateNormals();
        
        TSPoolList<Vector3>.Recycle(ropePositions);
        TSPoolList<Vector3>.Recycle(ropeNormals);
        TSPoolList<Vector3>.Recycle(vertices);
        TSPoolList<Vector4>.Recycle(uvs);
        TSPoolList<int>.Recycle(indexes);
    }

#if UNITY_EDITOR
    public bool m_DrawGizmos;
    private void OnDrawGizmos()
    {
        if (!m_DrawGizmos)
            return;
        
        CalculatePositions(out Vector3 srcPosition,out Vector3 srcBiTangent,out Vector3 dstPosition,out Vector3 dstBiTangent,out Vector3 control);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(m_ControlDamper.position,.2f);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(control,.2f);
        m_Curve.DrawGizmos();
    }
#endif
}
