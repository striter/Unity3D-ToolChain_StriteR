using System;
using UnityEngine;
namespace PCG.Module.Prop
{
    [Serializable]
    public struct ModuleDecorationCollection
    {
        public bool Available => decorationSets != null && decorationSets.Length > 0;
        public ModuleDecorationSet[] decorationSets;
    }

    [Serializable]
    public struct ModuleDecorationSet
    {
        public int density;
        public uint[] masks;
        public ModulePossibilitySet[] propSets;
        public bool maskRightAvailablity;
    }

    [Serializable]
    public struct ModulePossibilitySet
    {
        public ModulePropSet[] possibilities;
    }

    [Serializable]
    public struct ModulePropSet
    {
        public ModulePropData[] props;
    }
    
    [Serializable]
    public struct ModulePropData
    {
        public EModulePropType type;
        public int meshIndex;
        public int[] embedMaterialIndex;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }
}