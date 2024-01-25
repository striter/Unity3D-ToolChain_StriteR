using System;
using UnityEngine;

namespace UnityEditor.Extensions.AssetPipeline
{
    [Serializable, CreateAssetMenu(fileName = "AssetProcessRules", menuName = "Asset/Asset Process Rules", order = 0)]
    public class AssetProcessRules : ScriptableObject
    {
        public MeshProcessRules[] meshRules;
    }
}