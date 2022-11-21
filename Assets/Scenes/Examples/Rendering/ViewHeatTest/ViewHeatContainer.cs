using UnityEngine;

namespace  ExampleScenes.Rendering.ViewHeat
{
    
    public class ViewHeatContainer : MonoBehaviour
    {
        private Vector3[] m_Vertices;
        private Color[] m_Colors;
        private Mesh m_Mesh;
        public void Init()
        {
            var filter = GetComponentInChildren<MeshFilter>();
            var sharedMesh = filter.sharedMesh;
            m_Vertices = sharedMesh.vertices;
            m_Colors = new Color[m_Vertices.Length];

            m_Mesh = new Mesh
            {
                name = $"{sharedMesh.name} clone",
                vertices = m_Vertices,
                colors = m_Colors,
                normals = sharedMesh.normals,
            };
            
            m_Mesh.SetIndices(sharedMesh.GetIndices(0),MeshTopology.Triangles,0);
            m_Mesh.MarkModified();
            m_Mesh.MarkDynamic();
            filter.sharedMesh = m_Mesh;
        }

        public void Clear()
        {
            m_Colors = new Color[m_Vertices.Length];
            m_Mesh.colors = m_Colors;
        }
        public void HeatUp(Vector3 position,float _deltaTime)
        {
            Debug.DrawRay(position,Vector3.forward);
            position = transform.worldToLocalMatrix.MultiplyPoint(position);
            int lastIndex = 0;
            float lastDistance = float.MaxValue;
            foreach (var (index,p) in m_Vertices.LoopIndex())
            {
                var sqrDistance = Vector3.SqrMagnitude(position - p);
                if (sqrDistance > lastDistance)
                    continue;
                lastDistance = sqrDistance;
                lastIndex = index;
            }
            Debug.DrawRay(transform.localToWorldMatrix.MultiplyPoint(m_Vertices[lastIndex]),Vector3.forward);
            m_Colors[lastIndex]=new Color( m_Colors[lastIndex].r+_deltaTime*.1f,0,0,0);
            m_Mesh.colors = m_Colors;
        }
    }

}