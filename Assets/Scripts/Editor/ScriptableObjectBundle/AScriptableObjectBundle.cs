using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Extensions.EditorExecutable
{
    [Serializable]
    public abstract class AScriptableObjectBundle : ScriptableObject
    {
        [ScriptableObjectEdit] public List<AScriptableObjectBundleElement> m_Objects;
        public abstract Type GetBaseType();
    }
}