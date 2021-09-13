using System;
using System.Collections;
using System.Collections.Generic;
using TPool;
using UnityEngine;

namespace  ConvexGrid
{
    public class ModuleContainer : PoolBehaviour<int>
    {
        private Mesh m_Mesh;
        public override void OnPoolInit(Action<int> _DoRecycle)
        {
            base.OnPoolInit(_DoRecycle);
            m_Mesh = new Mesh() {hideFlags = HideFlags.HideAndDontSave};
            m_Mesh.MarkDynamic();
            GetComponent<MeshFilter>().sharedMesh = m_Mesh;
        }

        public void Markup(VoxelCornerRelation _relation)
        {
            
        }
    }   
}
