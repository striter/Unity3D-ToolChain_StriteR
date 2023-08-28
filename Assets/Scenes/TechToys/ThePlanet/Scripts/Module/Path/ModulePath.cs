using Geometry;
using MeshFragment;
using TPool;
using TObjectPool;
using UnityEditor;
using UnityEngine;

namespace TechToys.ThePlanet.Module.Prop
{
    public class ModulePath : PoolBehaviour<PCGID>,IModuleStructureElement
    {
        public IVoxel m_Voxel { get; private set; }
        
        private byte m_PathByte;
        private Mesh m_Mesh;
        private MeshRenderer m_Renderer;
        public int Identity => identity.GetIdentity(DModule.kIDPath);
        
        public void Init(IVoxel _voxel)
        {
            m_Voxel = _voxel;
            m_Mesh = new Mesh();
            GetComponent<MeshFilter>().sharedMesh = m_Mesh;
            m_Mesh.MarkDynamic();
            m_Renderer = GetComponent<MeshRenderer>();
            transform.SyncPositionRotation(_voxel.transform);
        }

        public void Clear()
        {
            m_PathByte = byte.MinValue;
            m_Mesh.Clear();
        }
        
        public void Validate(Quad<bool> _result,ModuleData _data,Material[] _library)
        {
            var pathByte=_result.ToByte();
            if (m_PathByte == pathByte)
                return;
            m_PathByte = pathByte;
                    
            if (!_data.m_Paths.GetOrientedPath(m_PathByte,out var pathData,out var orientation))
                return;
            
            TSPoolList<IMeshFragment>.Spawn(out var orientedMesh);
            m_Mesh.name = $"{m_Voxel.Identity}";

            TSPoolList<Vector3>.Spawn(out var vertices);
            TSPoolList<Vector3>.Spawn(out var normals);
            TSPoolList<Vector4>.Spawn(out var tangents);

            var directionRotation = Quaternion.Euler(0f, orientation*90, 0f);
            foreach (var fragment in pathData.m_MeshFragments)
            {
                vertices.Clear();
                normals.Clear();
                tangents.Clear();

                int vertexCount = fragment.vertices.Length;
                for (int i = 0; i < vertexCount; i++)
                {
                    var srcVertex = fragment.vertices[i];
                    var srcNormal = fragment.normals[i];
                    var srcTangents = fragment.tangents[i];
                    
                    vertices.Add(DModuleProp.OrientedToObjectVertex(m_Voxel.m_ShapeOS,srcVertex,orientation));
                    normals.Add(directionRotation*srcNormal);
                    tangents.Add((directionRotation*srcTangents).ToVector4(srcTangents.w));
                }
                
                orientedMesh.Add(new FMeshFragmentData(){vertices = vertices.ToArray(),normals = normals.ToArray(),tangents = tangents.ToArray()
                    ,colors = fragment.colors,indexes = fragment.indexes,embedMaterial = fragment.embedMaterial,uvs = fragment.uvs});
            }
            TSPoolList<Vector3>.Recycle(vertices);
            TSPoolList<Vector3>.Recycle(normals);
            TSPoolList<Vector4>.Recycle(tangents);

            UMeshFragment.Combine(orientedMesh,m_Mesh,_library,out var embedMaterials);
            m_Renderer.materials = embedMaterials;
            TSPoolList<IMeshFragment>.Recycle(orientedMesh);
        }
        
        public override void OnPoolRecycle()
        {
            base.OnPoolRecycle();
            m_Voxel = null;
        }
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (m_Voxel == null)
                return;
            if(Selection.activeObject!=this.gameObject)
                return;
            Gizmos.color = Color.blue;
            var indexer = UModulePropByte.GetOrientedPathIndex(m_PathByte);
            UGizmos.DrawString(transform.position,$"{indexer.srcByte}");
        }
#endif


        public void TickLighting(float _deltaTime, Vector3 _lightDir)
        {
            
        }
    }

}