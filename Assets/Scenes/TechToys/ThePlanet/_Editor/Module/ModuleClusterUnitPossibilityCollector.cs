using System.Collections.Generic;
using Runtime.Geometry;
using MeshFragment;
using TechToys.ThePlanet.Module;
using TechToys.ThePlanet.Module.Cluster;

#if UNITY_EDITOR
namespace TechToys.ThePlanet.Baking
{
    using Runtime.Geometry;
    using UnityEngine;

    public class ModuleClusterUnitPossibilityCollector : MonoBehaviour
    {
        public Qube<bool> m_MixableMask;
        public void Import(Mesh[] _importMeshes,Qube<bool> _relation,string _moduleName)
        {
            var mesh = _importMeshes?.Find(p => p.name.LastEquals(_moduleName));
            if (mesh == null)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (!_relation[j])
                        continue;

                    var subCube = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                    subCube.SetParent(transform);
                    subCube.localScale = Vector3.one * .5f;
                    subCube.localPosition = KQube.kHalfUnitQubeBottomed[j] + kfloat3.up * .25f;
                }
                return;
            }
            var subMesh = new GameObject("Model").transform;
            subMesh.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            subMesh.gameObject.AddComponent<MeshRenderer>().sharedMaterials =new Material[mesh.subMeshCount];
            subMesh.transform.SetParent(transform);
            subMesh.transform.localPosition = Vector3.zero;
        }
        
        public ModuleClusterUnitPossibilityData Export(byte _baseByte,List<Material> _materialLibrary)
        {
            return new ModuleClusterUnitPossibilityData()
            {
                m_MixableReadMask = (byte)(m_MixableMask.ToByte() | _baseByte),
                m_Mesh  = UMeshFragmentEditor.BakeMeshFragment(transform, ref _materialLibrary,
                    DModuleCluster.ObjectToOrientedVertex)
            };
        }
    }
}
#endif