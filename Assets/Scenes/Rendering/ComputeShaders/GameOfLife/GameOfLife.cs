using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Examples.Rendering.ComputeShaders.GameOfLife
{
    //&https://iquilezles.org/articles/gameoflife/
    public class GameOfLife : MonoBehaviour
    {
        public ComputeShader m_ComputeShader;
        [Range(0,1)] public float m_Threshold;
        private RenderTexture m_Buffer0,m_Buffer1;
        private int m_ResetKernel, m_GenerationKernel;
        private bool m_UseBuffer0;
        private RawImage m_RawImage;
        private void Start()
        {
            TouchConsole.Command("Reset").Button(Reset);
            m_RawImage = transform.GetComponentInChildren<RawImage>();
            m_Buffer0 = RenderTexture.GetTemporary(1920,1080,0,RenderTextureFormat.R8);
            m_Buffer0.enableRandomWrite = true;
            m_Buffer0.Create();
            m_Buffer1 = RenderTexture.GetTemporary(1920,1080,0,RenderTextureFormat.R8);
            m_Buffer1.enableRandomWrite = true;
            m_Buffer1.Create();

            m_ResetKernel = m_ComputeShader.FindKernel("Reset");
            m_GenerationKernel = m_ComputeShader.FindKernel("Generation");
            Reset();
        }

        private void Reset()
        {
            m_ComputeShader.SetTexture(m_ResetKernel,"_Result",m_Buffer0);
            m_ComputeShader.SetFloat("_Threshold",m_Threshold);
            m_ComputeShader.Dispatch(m_ResetKernel,m_Buffer0.width/8,m_Buffer1.width/8,1);
            m_RawImage.texture = m_UseBuffer0 ? m_Buffer0 : m_Buffer1;
            m_UseBuffer0 = false;
        }

        private void Update()
        {
            RenderTexture preTexture = m_UseBuffer0 ? m_Buffer1 : m_Buffer0;
            RenderTexture curTexture = m_UseBuffer0 ? m_Buffer0 : m_Buffer1;
            
            m_ComputeShader.SetTexture(m_GenerationKernel,"_PreResult",preTexture);
            m_ComputeShader.SetTexture(m_GenerationKernel,"_Result",curTexture);

            m_ComputeShader.Dispatch(m_GenerationKernel,m_Buffer0.width/8,m_Buffer1.width/8,1);
            m_RawImage.texture = curTexture;
            m_UseBuffer0 = !m_UseBuffer0;
        }

        private void OnDestroy()
        {
            RenderTexture.ReleaseTemporary(m_Buffer0);
            RenderTexture.ReleaseTemporary(m_Buffer1);
        }
    }
}