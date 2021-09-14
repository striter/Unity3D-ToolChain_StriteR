using System;
using System.Collections;
using System.Collections.Generic;
using Geometry.Voxel;
using TPool;
using UnityEngine;

namespace ConvexGrid
{
    public class ModuleManager : MonoBehaviour,IConvexGridControl
    {
        public ConvexMeshData m_Data;
        public TObjectPoolMono<PileID, ModuleContainer> m_Containers { get; set; }
        public void Init(Transform _transform)
        {
            m_Containers = new TObjectPoolMono<PileID, ModuleContainer>(_transform.Find("Modules/Container"));
        }

        public void Clear()
        {
            m_Containers.Clear();
        }
        
        public void SpawnModules(IModuleCollector _module)
        {
            // if(m_Containers.Contains(_module.m_Identity))
            //     return;
            Debug.Log(_module.m_Identity.gridID+" "+_module.m_Identity.height);
            m_Containers.Spawn(_module.m_Identity).Init(_module);
        }
        public void RecycleModules(PileID _moduleID)
        {
            // if (!m_Containers.Contains(_moduleID))
            //     return;
            m_Containers.Recycle(_moduleID);
        }
        
        public void Tick(float _deltaTime)
        {
        }

        public void OnSelectVertex(ConvexVertex _vertex, byte _height)
        {
        }

        public void OnAreaConstruct(ConvexArea _area)
        {
        }


        public void ValidateModules(IEnumerable<PileID> _moduleID)
        {
            foreach (var module in _moduleID)
                m_Containers.Get(module).ModuleValidate(m_Data);
        }
        
        #region Gizmos

        public bool m_Gizmos;
        void OnDrawGizmos()
        {
            if (!m_Gizmos||m_Containers==null)
                return;

            Gizmos.color = Color.white.SetAlpha(.5f);
            foreach (ModuleContainer moduleContainer in m_Containers)
            {
                Gizmos.matrix = moduleContainer.transform.localToWorldMatrix;
                Gizmos.DrawWireSphere(Vector3.zero,.3f);
            }
        }
        #endregion
    }
}
