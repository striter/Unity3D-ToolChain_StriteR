using TechToys.ThePlanet.Module;

namespace TechToys.ThePlanet.Simplex
{
    public static class DSimplex
    {
        public static readonly OrientedModuleIndexer[] kIndexes;
        static DSimplex()
        {
            kIndexes = new OrientedModuleIndexer[byte.MaxValue + 1];
            int index = 0;
            for (int i = 0; i <= byte.MaxValue; i++)
            {
                if (i == 0)
                {
                    kIndexes[i] = OrientedModuleIndexer.Invalid;
                    continue;
                }
                
                var (baseByte,orientation) = UModuleByte.kByteOrientation[(byte) i];
                kIndexes[(byte) i] = new OrientedModuleIndexer(){srcByte = baseByte ,index = orientation > 0?kIndexes[baseByte].index: index++,orientation = orientation};
            }
        }
    }
}