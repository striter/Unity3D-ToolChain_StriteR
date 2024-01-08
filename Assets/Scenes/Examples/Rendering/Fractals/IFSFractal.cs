using System;
using System.Linq;
using Rendering;
using Rendering.Pipeline;
using UnityEngine;
using Unity.Mathematics;

namespace Examples.Rendering.Fractals
{
    [Serializable]
    public struct IFSInput
    {
        public float2 center;
        public float angle;
        public float2 scale;
        public float timeScale;
        [ColorUsage(true,true)]public Color color;
        public static IFSInput kDefaullt = new IFSInput()
        {
            angle = 90f,
            center = .5f,
            scale = 1f,
        };
        
        public IFSOutput Output()
        {
            var matirx = float3x2_homogenous.TRS(center,(angle + timeScale* UTime.time)*kmath.kDeg2Rad,scale);
            return new IFSOutput()
            {
                matrix = matirx,
                contraction = math.determinant(new float2x2(matirx.c0,matirx.c1)),
                color = color.to4(),
            };
        }
    }

    public struct IFSOutput
    {
        public float3x2 matrix;
        public float contraction;
        public float4 color;
        
        public const int kSize = 11 * sizeof(float);
    }
    
    [ExecuteInEditMode]
    public class IFSFractal : MonoBehaviour
    {
        public IFSInput[] inputs = new[] { IFSInput.kDefaullt, };
        public int2 size = new int2(256, 256);        
        public ComputeShader m_Shader;
        private RenderTexture m_RenderTexture;
        
        private const int m_MaxFractalCount = 12;
        private ComputeBuffer m_Buffer;
        private int m_MainKernal;

        void Init()
        {
            Dispose();
            m_Buffer = new ComputeBuffer(m_MaxFractalCount, IFSOutput.kSize);
            m_MainKernal = m_Shader.FindKernel("Main");
            m_RenderTexture = RenderTexture.GetTemporary(size.x,size.y,0,RenderTextureFormat.ARGB32);
            m_RenderTexture.enableRandomWrite = true;
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetTexture("_MainTex",m_RenderTexture);
            GetComponent<MeshRenderer>().SetPropertyBlock(block);
        }

        void Dispose()
        {
            if (m_Buffer == null)
                return;
            
            m_Buffer.Release();
            m_Buffer = null;
            RenderTexture.ReleaseTemporary(m_RenderTexture);
        }


        private void Awake() => Init();

        private void OnValidate() => Init();

        private void Update()
        {
            UPipeline.ClearRenderTextureWithComputeShader(m_RenderTexture);
            
            m_Shader.SetTexture(m_MainKernal, "_Result",m_RenderTexture);
            m_Shader.SetVector( "_Result_ST",m_RenderTexture.GetTexelSizeParameters());
            for (int i = 0; i < inputs.Length; i++)
            {
                m_Buffer.SetData(new [] { inputs[i].Output() });
                m_Shader.SetBuffer(m_MainKernal,"_IFSBuffer",m_Buffer);
                
                m_Shader.Dispatch(m_MainKernal,m_RenderTexture.width/8,m_RenderTexture.height/8,1);
            }
        }

        private void OnDestroy() => Dispose();
    }

}