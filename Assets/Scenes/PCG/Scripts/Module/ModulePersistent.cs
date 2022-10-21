
using System;
using System.Collections.Generic;
using PCG.Module;
using TDataPersistent;

namespace PCG
{
    using static PCGDefines<int>;
    [Serializable]
    public struct ModuleInput
    {
        public PCGID origin;
        public int type;
    }
    
    public class ModulePersistent:CDataSave<ModulePersistent>
    {
        public override bool DataCrypt() => false;
        public List<ModuleInput> m_Modules;
    }
}