using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Extensions.ScriptableObjectBundle
{
    [Serializable, CreateAssetMenu(fileName = "AssetProcessRules", menuName = "AssetPostProcess/Process Rules", order = 0)]
    public class AssetPostProcessBundle : AScriptableObjectBundle
    {
        public bool m_Enable = true;
        public override Type GetBaseType() => typeof(AssetPostProcessRule);
    }
}