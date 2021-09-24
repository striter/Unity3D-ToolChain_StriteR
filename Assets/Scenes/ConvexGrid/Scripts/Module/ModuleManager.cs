using System;
using System.Collections;
using System.Collections.Generic;
using Geometry.Voxel;
using Procedural;
using TPool;
using UnityEngine;

namespace ConvexGrid
{
    public class ModuleManager : MonoBehaviour,IConvexGridControl
    {
        public EModuleType m_SpawnModule;
        public ModuleRuntimeData m_Data;
        public TObjectPoolMono<PileID, ModuleVoxel> m_Voxels { get; private set; }
        public TObjectPoolMono<PileID, ModuleCorner> m_Corners { get; private set; }
        public void Init(Transform _transform)
        {
            m_Voxels = new TObjectPoolMono<PileID, ModuleVoxel>(_transform.Find("Modules/Voxel/Item"));
            m_Corners = new TObjectPoolMono<PileID, ModuleCorner>(_transform.Find("Modules/Corner/Item"));
        }
        public void Clear()
        {
            m_Voxels.Clear();
            m_Corners.Clear();
        }

        public void SpawnCorners(ICorner _corner)=> m_Corners.Spawn(_corner.Identity).Init(_corner,m_SpawnModule);
        public void RecycleCorners(PileID _cornerID)=>m_Corners.Recycle(_cornerID);
        public void SpawnModules(IVoxel _module)=>m_Voxels.Spawn(_module.Identity).Init(_module);
        public void RecycleModules(PileID _moduleID)=>m_Voxels.Recycle(_moduleID);
        
        public void Tick(float _deltaTime)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
                m_SpawnModule = m_SpawnModule.Next();
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
                m_Voxels.Get(module).ModuleValidate(m_Data,m_Corners.m_Dic);
        }
        
        #if UNITY_EDITOR
        #region Gizmos

        public bool m_Gizmos;
        [MFoldout(nameof(m_Gizmos),true)] public bool m_ShapeGizmos;
        void OnDrawGizmos()
        {
            if (!m_Gizmos||m_Voxels==null)
                return;

            Gizmos.color = Color.white.SetAlpha(.5f);
            foreach (ModuleVoxel moduleContainer in m_Voxels)
            {
                Gizmos.matrix = moduleContainer.transform.localToWorldMatrix;
                Gizmos.DrawWireSphere(Vector3.zero,.3f);
                if(m_ShapeGizmos)
                    foreach (var quad in moduleContainer.m_Voxel.CornerShapeLS)
                        Gizmos_Extend.DrawLinesConcat(quad.Iterate(p=>((Coord)p).ToPosition()));
            }
        }
        #endregion
        #endif
    }
}
