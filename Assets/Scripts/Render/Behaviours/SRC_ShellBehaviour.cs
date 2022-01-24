using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rendering.Pipeline
{
    [ExecuteInEditMode,RequireComponent(typeof(MeshRenderer),typeof(MeshFilter))]
    public class SRC_ShellBehaviour : MonoBehaviour
    {
        [Clamp(0,256)]public int m_ShellCount = 32;
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
            m_Materials = new Material[m_ShellCount];
            for (int i = m_ShellCount-1; i >=0; i--)        //Edge to skin so it matches early Z
            {
                m_Materials[i] = new Material(m_ShellMaterial)
                    { name = $"{m_ShellMaterial.name}  Temporal: {i}", hideFlags = HideFlags.HideAndDontSave };
                m_Materials[i].SetFloat(ID_ShellIndex,(float)i/m_ShellCount);
            }
            m_MeshRenderer.materials = m_Materials;
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (m_ShellMaterial == null)
                return;
            
            for (int i = m_ShellCount-1; i >=0; i--)        //Edge to skin so it matches early Z
            {
                m_Materials[i].CopyPropertiesFromMaterial(m_ShellMaterial); 
                m_Materials[i].SetFloat(ID_ShellIndex,(float)i/m_ShellCount);
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