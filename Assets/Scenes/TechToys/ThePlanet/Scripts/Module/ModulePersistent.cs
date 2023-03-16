
using System;
using System.Collections.Generic;
using TechToys.ThePlanet.Module;
using TDataPersistent;

namespace TechToys.ThePlanet
{
    [Serializable]
    public struct ModuleInput
    {
        public PCGID origin;
        public int type;
    }
    
    [Serializable]
    public class ModulePersistent:CDataSave<ModulePersistent>
    {
        public override bool DataCrypt() => false;
        public List<ModuleInput> m_Modules;
    }
}