using Runtime.Geometry;
using UnityEngine;
using UnityEngine.Serialization;

public class MeshProcessing : MonoBehaviour
{
    public Mesh m_SharedMesh;
    private GMesh m_Mesh;

    private void OnValidate()
    {
        if (m_SharedMesh == null)
            return;
        m_Mesh = new GMesh(m_SharedMesh);
    }

    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        m_Mesh.DrawGizmos(EDrawMeshFlag.Vertices | EDrawMeshFlag.Edges);
    }

    [InspectorButton]
    public void Reset()
    {
        m_Mesh = new GMesh(m_SharedMesh);
    }

    [InspectorButton]
    public void LoopSubdivision()
    {
        m_Mesh.LoopSubdivision();   
    }
}
