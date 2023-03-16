using System.Collections.Generic;
using System.Linq;
using TechToys.ThePlanet.Module;

#if UNITY_EDITOR
namespace TechToys.ThePlanet.Baking
{
    using Geometry;
    using UnityEngine;
    public class ModuleClusterUnitCollector : MonoBehaviour
    {
        [HideInInspector] public Qube<bool> m_Relation;

        public void Import(Mesh[] _importMeshes,byte _voxelByte,string _moduleName)
        {
            m_Relation= default;
            m_Relation.SetByteElement(_voxelByte);
            var possibility = new GameObject("Possibility").transform;
            possibility.SetParent(transform);
            possibility.gameObject.AddComponent<ModuleClusterUnitPossibilityCollector>().Import(_importMeshes,m_Relation,_moduleName);
            possibility.transform.localPosition = Vector3.zero;
        }

        public ModuleClusterUnitData Export(List<Material> _materialLibrary)
        {
            return new ModuleClusterUnitData()
            {
                m_Possibilities = transform.GetComponentsInChildren<ModuleClusterUnitPossibilityCollector>()
                    .Select(_p => _p.Export(m_Relation.ToByte(), _materialLibrary)).ToArray(),
            };
        }

        void DrawQubeGizmos(Transform _transform,Color _color)
        {
            Gizmos.matrix = _transform.localToWorldMatrix;
            Gizmos.color = _color;
            Gizmos.DrawWireCube(Vector3.up*.5f,Vector3.one);
            Qube<bool> mixableRelation = KQube.kFalse;
            var possibility = _transform.GetComponent<ModuleClusterUnitPossibilityCollector>();
            if (possibility)
                mixableRelation = possibility.m_MixableMask;
            
            for (int i = 0; i < 8; i++)
            {
                Gizmos.color = m_Relation[i] ? Color.green : mixableRelation[i]?Color.yellow: Color.red.SetAlpha(.5f);
                Gizmos.DrawWireSphere(KQube.kUnitQubeBottomed[i],.1f);
            }
        }
        
        public void OnDrawGizmos()
        {
            int childCount = transform.childCount;
            if (childCount == 0)
            {
                DrawQubeGizmos(transform,Color.red.SetAlpha(.5f));
                return;
            }
            Gizmos_Extend.DrawString(transform.position+Vector3.up*.5f,m_Relation.ToByte().ToString(),0f);
            for (int i = 0; i < childCount; i++)
            {
                var setTransform = transform.GetChild(i);
                setTransform.localPosition = Vector3.up * (childCount-i-1)*1.5f;
                setTransform.localRotation = Quaternion.identity;
                setTransform.localScale = Vector3.one;
                DrawQubeGizmos(setTransform,Color.white.SetAlpha(.5f));
            }
        }
    }
}
#endif