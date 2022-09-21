using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rendering.Pipeline
{
    public enum EShellAmount
    {
       _4 = 4,
       _8 = 8,
       _16 = 16,
       _32 = 32,
       _64 = 64,
       _128 = 128,
       _256 = 256,
    }
    
    [ExecuteInEditMode,RequireComponent(typeof(MeshRenderer),typeof(MeshFilter))]
    public class ShellController : MonoBehaviour
    {
        public EShellAmount m_ShellCount = EShellAmount._32;
        public Material m_ShellMaterial;
        private Material[] m_Materials;

        private MeshRenderer m_MeshRenderer;
        private static readonly int ID_ShellIndex = Shader.PropertyToID("_ShellDelta");
        private void OnValidate()
        {
            OnDestroy();
            Awake();
        }
        private void Awake()
        {
            if (m_ShellMaterial == null)
                return;
            m_MeshRenderer=GetComponent<MeshRenderer>();
            var count = UEnum.GetValue(m_ShellCount);
            m_Materials = new Material[count];
            for (int i = count-1; i >=0; i--)        //Edge to skin so it matches early Z
            {
                m_Materials[i] = new Material(m_ShellMaterial)
                    { name = $"{m_ShellMaterial.name}  Temporal: {i}", hideFlags = HideFlags.HideAndDontSave };
                m_Materials[i].SetFloat(ID_ShellIndex,(float)i/count);
            }
            m_MeshRenderer.materials = m_Materials;
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (m_ShellMaterial == null)
                return;
            
            var count = UEnum.GetValue(m_ShellCount);
            for (int i = count-1; i >=0; i--)        //Edge to skin so it matches early Z
            {
                m_Materials[i].CopyPropertiesFromMaterial(m_ShellMaterial); 
                m_Materials[i].SetFloat(ID_ShellIndex,(float)i/count);
            }
        }
#endif

        private void OnDestroy()
        {
            if (m_Materials == null)
                return;
            
            for (int i = 0; i < m_Materials.Length; i++)
                GameObject.DestroyImmediate(m_Materials[i]);
            m_MeshRenderer.materials = Array.Empty<Material>();
            m_Materials = null;
        }
    }

}