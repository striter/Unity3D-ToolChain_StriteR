using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using UnityEngine;

namespace UnityEditor.Extensions.ScriptableObjectBundle
{
    [Serializable]
    public abstract class AScriptableObjectBundle : ScriptableObject
    {
        [ScriptableObjectEdit] public List<AScriptableObjectBundleElement> m_Objects;
        public abstract Type GetBaseType();

        private bool m_Dirty = false;
        private void OnEnable() => EditorApplication.update += Tick;
        private void OnDisable() => EditorApplication.update -= Tick;
        void Tick()
        {
            if (!m_Dirty)
                return;
            if (m_Objects.Any(p => p == null))
                return;
            
            m_Dirty = false;
            UEAsset.ClearSubAssets(this);
            foreach (var (index, so) in m_Objects.LoopIndex())
            {
                var name = so.m_Title;
                if (string.IsNullOrEmpty(name))
                    name = so.GetType().Name;
                so.name = $"{index}_{name}";
            }
            
            UEAsset.CreateOrReplaceSubAsset(this, m_Objects);
            EditorUtility.SetDirty(this);
        }
        public void SetBundleDirty() => m_Dirty = true;
    }
}