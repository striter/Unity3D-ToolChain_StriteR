using System;
using UnityEditor.Extensions.ScriptableObjectBundle;
using UnityEngine;

namespace UnityEditor.Extensions.AssetPipeline
{
    [Serializable, CreateAssetMenu(fileName = "AssetProcessRules", menuName = "AssetPostProcess/Process Rules", order = 0)]
    public class AssetProcessBundle : AScriptableObjectBundle
    {
        public bool m_Enable = true;
        public override Type GetBaseType() => typeof(AAssetProcess);
        public virtual void OnManualChange() { }
    }
}