
using System;
using System.Collections.Generic;
using PolyGrid.Module;
using TDataPersistent;

namespace PolyGrid
{
    [Serializable]
    public struct CornerData
    {
        public PolyID identity;
        public EModuleType type;
    }
    
    public class PersistentData:CDataSave<PersistentData>
    {
        public override bool DataCrypt() => false;
        public List<CornerData> m_CornerData;

        public void Record(IEnumerable<CornerData> _corners)
        {
            m_CornerData.Clear();
            m_CornerData.AddRange(_corners);
        }

        public override void Validate()
        {
            base.Validate();
            if (m_CornerData == null)
                m_CornerData = new List<CornerData>();
        }
    }
}