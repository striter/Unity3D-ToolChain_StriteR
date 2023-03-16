using TechToys.ThePlanet.Module.Cluster;

#if UNITY_EDITOR
namespace TechToys.ThePlanet.Baking
{
    using System.Collections.Generic;
    using Module.Prop;
    using UnityEngine;
    public class ModuleDecorationCollector : MonoBehaviour
    {
        public int m_Density;
        public bool m_MaskRightAvailability = false;
        public void Import(EClusterType _clusterType,Mesh[] _importMeshes)
        {
            int width = -4;
            foreach (var qubeByte in UModulePropByte.IterateDecorationBytes(_clusterType))
            {
                var decorationModel = new GameObject().transform;
                decorationModel.gameObject.AddComponent<ModulePropPossibilitySetBaker>().Import(_importMeshes,qubeByte);
                decorationModel.SetParent(transform);
                decorationModel.localScale=Vector3.one*2f;
                decorationModel.localPosition = Vector3.right * 3 * width++;
            }
        }
        
        public ModuleDecorationSet Export(List<Mesh> _meshLibrary,ref List<Material> _materialLibrary)
        {
            uint[] masks = new uint[3];
            var meshBaker = GetComponentsInChildren<ModulePropPossibilitySetBaker>();
            ModulePossibilitySet[] propSets = new ModulePossibilitySet[meshBaker.Length];
            for (int i = 0; i < meshBaker.Length; i++)
            {
                propSets[i] = meshBaker[i].Export(_meshLibrary, _materialLibrary);
                if(propSets[i].possibilities.Length==0)
                    continue;

                int maskIndex = i / 32;
                masks[maskIndex] += (uint)(1 << i);
            }
            
            ModuleDecorationSet data = new ModuleDecorationSet
            {
                density = m_Density,
                masks = masks,
                propSets = propSets,
                maskRightAvailablity = m_MaskRightAvailability,
            };
            return data;
        }
    }
}
#endif