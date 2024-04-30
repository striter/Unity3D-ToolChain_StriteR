using System.Collections.Generic;
using UnityEditor.Extensions.EditorPath;
using UnityEditor.Extensions.ScriptableObjectBundle;
using UnityEngine;

namespace UnityEditor.Extensions.ScriptableObjectBundle
{
    public class AssetPostProcessRule : AScriptableObjectBundleElement
    {
        public virtual string pathFilter => string.Empty;

    }
    
}