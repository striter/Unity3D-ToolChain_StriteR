using Geometry;
using TPoolStatic;
using UnityEngine;

namespace PolyGrid.Module.Baking
{
    public class ModuleBakerModel : MonoBehaviour
    {
        public Qube<bool> m_Relation;
        public OrientedModuleMeshData CollectModuleMesh(EModuleType _type,ECornerStatus _status)
        {
            var vertices = TSPoolList<Vector3>.Spawn();
            var indexes = TSPoolList<int>.Spawn();
            var uvs = TSPoolList<Vector2>.Spawn();
            var normals = TSPoolList<Vector3>.Spawn();
            var colors = TSPoolList<Color>.Spawn();

            foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
            {
                var mesh = meshFilter.sharedMesh;
                var localToWorldMatrix = meshFilter.transform.localToWorldMatrix;
                var worldToLocalMatrix = transform.worldToLocalMatrix;

                int indexOffset = vertices.Count;

                var curVertices = mesh.vertices;
                var curNormals = mesh.normals;
                var curColors = mesh.colors;
                for (int i = 0; i < curVertices.Length; i++)
                {
                    var positionWS = localToWorldMatrix.MultiplyPoint(curVertices[i]);
                    var positionOS = worldToLocalMatrix.MultiplyPoint(positionWS);
                    var normalWS = localToWorldMatrix.MultiplyVector(curNormals[i]);
                    var normalOS = worldToLocalMatrix.MultiplyVector(normalWS);
                    vertices.Add(UModule.ObjectToModuleVertex(positionOS));
                    normals.Add(normalOS);
                    colors.Add(_type.ToColor() );//curColors[i]);
                }

                foreach (var index in mesh.GetIndices(0))
                    indexes.Add(indexOffset+index);    
                
                uvs.AddRange(mesh.uv);
            }
            
            var moduleMesh=new OrientedModuleMeshData
            {
                m_Vertices = vertices.ToArray(),
                m_UVs=uvs.ToArray(),
                m_Indexes = indexes.ToArray(),
                m_Normals = normals.ToArray(),
                m_Colors = colors.ToArray(),
            };
            
            TSPoolList<Vector3>.Recycle(vertices);
            TSPoolList<int>.Recycle(indexes);
            TSPoolList<Vector2>.Recycle(uvs);
            TSPoolList<Vector3>.Recycle(normals);
            TSPoolList<Color>.Recycle(colors);
            return moduleMesh;
        }
        
        public void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.up*.5f,Vector3.one);
            for (int i = 0; i < 8; i++)
            {
                Gizmos.color = m_Relation[i] ? Color.green : Color.red.SetAlpha(.5f);
                Gizmos.DrawWireSphere(UModule.unitQube[i],.1f);
            }
        }
    }

}