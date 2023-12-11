using System;
using TPool;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class DrawShapes : MonoBehaviour
{
    public ComputeShader m_Shader;
    [Clamp(1)] public uint m_ThreadCount = 10;
    public Color m_CircleColor = KColor.kOrange;
    public Color m_ClearColor = Color.grey;
    private RawImage m_RawImage;
    private RenderTexture m_Texture;
    private int m_ClearKernel, m_DrawKernel;
    private uint kDrawKernelXCount;

    [Serializable]
    struct CircleData
    {
        public float2 origin;
        public float2 velocity;
        public float radius;
        public static readonly int kStride = (2 + 2 + 1) * sizeof(float);
    }

    [SerializeField,HideInInspector] CircleData[] m_Circles;
    private ComputeBuffer m_CircleBuffer;

    private float2[] m_Results;
    private ComputeBuffer m_CircleResults;
    private ObjectPoolComponent<Image> m_Images;

    private bool Available => Application.isPlaying && m_Shader;
    
    private void Awake()
    {
        if (!Available)
            return;
        
        m_RawImage = transform.GetComponentInChildren<RawImage>();
        m_Texture = RenderTexture.GetTemporary(new RenderTextureDescriptor()
        {
            width = Screen.width,
            height = Screen.height,
            volumeDepth = 1,
            dimension = TextureDimension.Tex2D,
            colorFormat = RenderTextureFormat.ARGB32,
            enableRandomWrite = true,
            msaaSamples = 1,
        });
        m_RawImage.texture = m_Texture;
        m_Images = new ObjectPoolComponent<Image>(transform.Find("Image"));
        
        OnValidate();
    }

    private void OnDestroy()
    {
        if (!Available)
            return;
        RenderTexture.ReleaseTemporary(m_Texture);
        m_RawImage.texture = null;
        m_Texture = null;
        OnDispose();
    }

    void OnDispose()
    {
        if(m_CircleBuffer!=null)
            m_CircleBuffer.Dispose();
        if(m_CircleResults!=null)
            m_CircleResults.Dispose();

        m_CircleBuffer = null;
        m_CircleResults = null;
    }

    private void OnValidate()
    {
        if (!Available)
            return;

        if (m_Images == null)
            return;
        
        OnDispose();
        m_ClearKernel = m_Shader.FindKernel("Clear");
        m_DrawKernel = m_Shader.FindKernel("Draw");
        
        m_Shader.SetVector("_ClearColor",m_ClearColor);
        m_Shader.SetVector("_CircleColor",m_CircleColor);
        m_Shader.SetFloat("_Count",m_ThreadCount);

        m_Shader.GetKernelThreadGroupSizes(m_DrawKernel,out kDrawKernelXCount,out _,out _);
        
        m_Circles = new CircleData[m_ThreadCount * kDrawKernelXCount];
        for (int i = 0; i < m_Circles.Length; i++)
        {
            CircleData data = new CircleData();
            data.origin = URandom.Random2DQuad() * new float2(Screen.width, Screen.height);
            data.velocity = URandom.Random2DSphere() * 30f;
            data.radius = 10f + URandom.Random01() * 30f;
            m_Circles[i] = data;
        }

        m_CircleBuffer = new ComputeBuffer(m_Circles.Length, CircleData.kStride);
        m_CircleBuffer.SetData(m_Circles);

        m_Results = new float2[m_Circles.Length];
        m_CircleResults = new ComputeBuffer(m_Circles.Length, 2 * sizeof(float));
        // m_CircleResults.SetData(m_Results);
        m_Images.Clear();
        for (int i = 0; i < m_Circles.Length; i++)
            m_Images.Spawn();
    }

    void Update()
    {
        if (!Available)
            return;
        
        m_Shader.SetVector("_Resolution",new Vector4(Screen.width,Screen.height));
        m_Shader.SetTexture(m_ClearKernel,"Result",m_Texture);
        m_Shader.Dispatch(m_ClearKernel,Screen.width/8,Screen.height/8,1);
        m_Shader.SetTexture(m_DrawKernel,"Result",m_Texture);
        m_Shader.SetBuffer(m_DrawKernel,"_Circles",m_CircleBuffer);
        m_Shader.SetBuffer(m_DrawKernel,"_CircleResults",m_CircleResults);
        m_Shader.Dispatch(m_DrawKernel,(int)m_ThreadCount,1,1);
        m_CircleResults.GetData(m_Results);
        for (int i = 0; i < m_Circles.Length; i++)
        {
            m_Images[i].rectTransform.anchorMin = m_Results[i];
            m_Images[i].rectTransform.anchorMax = m_Results[i];
            m_Images[i].rectTransform.anchoredPosition = Vector2.zero;
        }

    }
}
