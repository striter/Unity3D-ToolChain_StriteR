using System;
using UnityEngine;

namespace UnityEditor.Extensions.ScriptableObjectBundle
{
    [Serializable, CreateAssetMenu(fileName = "AssetProcessRules", menuName = "AssetPostProcess/Process Rules", order = 0)]
    public class AssetProcessRules : AScriptableObjectBundle
    {
        public override Type GetBaseType() => typeof(AssetPostProcessRule);
    }
}