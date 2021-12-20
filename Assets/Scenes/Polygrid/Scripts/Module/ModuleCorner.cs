using System.Collections;
using System.Collections.Generic;
using PolyGrid;
using TPool;
using UnityEngine;

namespace PolyGrid.Module
{
    public class ModuleCorner : PoolBehaviour<PolyID>
    {
        public PolyID Identity => m_PoolID;
        public ICorner m_Corner { get; private set; }
        public EModuleType m_Type { get; private set; }
        public ECornerStatus m_Status { get; private set; }
        public ModuleCorner Init(ICorner _corner,EModuleType _type)
        {
            transform.SetPositionAndRotation(_corner.Transform.position,_corner.Transform.rotation);
            m_Corner = _corner;
            m_Type = _type;
            return this;
        }

        public bool RefreshStatus(byte _chainMaxHeight)
        {
            var status=DModule.GetCornerStatus(m_PoolID.height, _chainMaxHeight);
            if (m_Status == status)
                return false;
            m_Status = status;
            return true;
        }
        

        public override void OnPoolRecycle()
        {
            base.OnPoolRecycle();
            m_Type = EModuleType.Invalid;
            m_Status = ECornerStatus.Invalid;
        }
    }
}