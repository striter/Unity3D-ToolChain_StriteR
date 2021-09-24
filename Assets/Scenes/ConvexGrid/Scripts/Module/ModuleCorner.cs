using System.Collections;
using System.Collections.Generic;
using ConvexGrid;
using TPool;
using UnityEngine;

namespace ConvexGrid
{
    public class ModuleCorner : PoolBehaviour<PileID>
    {
        public ICorner m_Corner { get; private set; }
        public EModuleType m_Type { get; private set; }

        public ModuleCorner Init(ICorner _corner,EModuleType _type)
        {
            transform.SetPositionAndRotation(_corner.Transform.position,_corner.Transform.rotation);
            m_Corner = _corner;
            m_Type = _type;
            return this;
        }

        public override void OnPoolRecycle()
        {
            base.OnPoolRecycle();
            m_Type = EModuleType.Invalid;
        }
    }
}