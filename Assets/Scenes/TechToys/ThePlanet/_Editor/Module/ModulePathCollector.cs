using System;
using System.Collections;
using System.Collections.Generic;
using Geometry;
using MeshFragment;
using TechToys.ThePlanet.Module;
using TechToys.ThePlanet.Module.Prop;
using UnityEngine;
using Gizmos = UnityEngine.Gizmos;

namespace TechToys.ThePlanet.Baking
{
#if UNITY_EDITOR
    public class ModulePathCollector : MonoBehaviour
    {
        public Quad<bool> m_Relation;

        public void Import(Mesh[] _importMeshes,byte _quadByte)
        {
            m_Relation = default;
            m_Relation.SetByteElement(_quadByte);
            var pathName = DBaking.GetPathName(_quadByte);
            gameObject.name = pathName;
            var mesh = _importMeshes?.Find(p => p.name.Equals(pathName));
            if (mesh == null)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (!m_Relation[j])
                        continue;

                    var subCube = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                    subCube.SetParent(transform);
                    subCube.localScale = Vector3.one * .5f;
                    subCube.localPosition = kfloat3.up*.25f + KQuad.k3SquareCentered45Deg[j]*.25f;
                }
                return;
            }
            
            var subMesh = new GameObject("Model").transform;
            subMesh.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            subMesh.gameObject.AddComponent<MeshRenderer>().sharedMaterials =new Material[mesh.subMeshCount];
            subMesh.transform.SetParent(transform);
            subMesh.transform.localPosition = Vector3.zero;
        }

        public FMeshFragmentCluster Export(ref List<Material> _materialLibrary) => UMeshFragmentEditor.BakeMeshFragment(
            transform, ref _materialLibrary,
            DModuleProp.ObjectToOrientedVertex);
        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.up*.5f,Vector3.one);
            for (int i = 0; i < 4; i++)
            {
                Gizmos.color = m_Relation[i] ? Color.green:Color.red.SetA(.5f) ;
                Gizmos.DrawWireSphere(KQuad.k3SquareCentered45Deg[i]*.5f,.1f);
            }
        }
    }
#endif
}