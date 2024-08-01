using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Extensions.ScriptableObjectBundle
{
    [Serializable, CreateAssetMenu(fileName = "AssetProcessRules", menuName = "AssetPostProcess/Process Rules", order = 0)]
    public class AssetPostProcessBundle : AScriptableObjectBundle
    {
        public bool m_Enable = true;
        public static List<AssetPostProcessBundle> kBundles { get; private set; } = new List<AssetPostProcessBundle>();
        private void Awake() => kBundles.Add(this);
        private void OnDestroy() =>  kBundles.Remove(this);
        public override Type GetBaseType() => typeof(AssetPostProcessRule);
    }
}