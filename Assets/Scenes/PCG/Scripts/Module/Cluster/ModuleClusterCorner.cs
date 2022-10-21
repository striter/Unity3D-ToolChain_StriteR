using System.Collections.Generic;
using TPool;

namespace PCG.Module.Cluster
{
    using static  PCGDefines<int>;
    public class ModuleClusterCorner : PoolBehaviour<PCGID>
    {
        public EClusterStatus m_Status { get; private set; }
        public ICorner m_Corner { get; private set; }

        public ModuleClusterCorner Init(ICorner _corner)
        {
            m_Corner = _corner;
            transform.SyncPositionRotation(_corner.Transform);
            return this;
        }

        public void RefreshStatus(byte _minValue,byte _maxValue, Dictionary<PCGID,ModuleClusterCorner> _corners)
        {
            var clusterStatus = DModule.Collection.m_ModuleLibrary[Type].m_ClusterType;
            m_Status = this.CollectCornerStatus(clusterStatus, _minValue, _maxValue, _corners);
        }

        public override void OnPoolRecycle()
        {
            base.OnPoolRecycle();
            m_Status = EClusterStatus.Invalid;
        }

        public PCGID Identity => m_Corner.Identity;
        public int Type => m_Corner.m_Type;
        public IVertex Vertex => m_Corner.m_Vertex;
    }
}