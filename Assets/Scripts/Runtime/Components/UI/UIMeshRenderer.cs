using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode,RequireComponent(typeof(CanvasRenderer))]
public class UIMeshRenderer : MaskableGraphic
{
    public Mesh m_Mesh;
    public bool m_ConstantSize = true;
    [MFoldout(nameof(m_ConstantSize),true)]public int m_BaseSize = 200;
    public Vector3 m_OSRotation = Vector3.zero;
    #if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        SetAllDirty();
    }
    #endif

    protected override void UpdateMaterial()
    {
        if (!IsActive())
            return;

        canvasRenderer.materialCount = 1;
        canvasRenderer.SetMaterial(materialForRendering, 0);
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        
        var rotation = Quaternion.Euler(m_OSRotation);
        
        List<UIVertex> uiVertices = new List<UIVertex>();
        var vertices = m_Mesh.vertices;
        var normals = m_Mesh.normals;
        var tangents = m_Mesh.tangents;
        var uvs = m_Mesh.uv;
        var colors = m_Mesh.colors;
        
        bool colorValid = colors.Length > 0;

        var modelSize = UBoundsIncrement.GetBounds(m_Mesh.bounds.GetEdges().Select(p => rotation * p)).size; 
        var rectSize = rectTransform.rect.size;
        var finalSize = Vector3.one;
        
        if (m_ConstantSize)
            rectSize = m_BaseSize * modelSize;
        
        var offset = modelSize.mul(rectTransform.pivot-Vector2.one*.5f);
        finalSize = new Vector3(rectSize.x/modelSize.x,rectSize.y/modelSize.y,float.Epsilon);
        
        var matrixTRS = Matrix4x4.TRS(Vector3.zero,Quaternion.identity, finalSize) * Matrix4x4.Translate(transform.position);
        for (int i = 0; i < vertices.Length; i++)
        {
            uiVertices.Add(new UIVertex()
            {
                position =  matrixTRS * (rotation*vertices[i]-offset),
                normal = rotation * normals[i],
                tangent = rotation * tangents[i],
                uv0 =  uvs[i],
                color = colorValid?colors[i]*color:color,
            });
        }

        var indices = new List<int>();
        m_Mesh.GetIndices(indices,0); 
        vh.AddUIVertexStream(uiVertices,indices);
    }
}

