using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshRenderer),typeof(MeshFilter))]
public class AnimationInstanceController : MonoBehaviour
{
    public AnimationInstanceData m_Data;
    public int m_CurrentAnimIndex { get; private set; }
    public float m_TimeElapsed { get; private set; }
    MeshRenderer m_MeshRenderer;
    MeshFilter m_MeshFilter;
    MaterialPropertyBlock m_PropertyBlock;
    public AnimationInstanceController Init()
    {
        m_PropertyBlock = new MaterialPropertyBlock();
        m_MeshRenderer = GetComponent<MeshRenderer>();
        m_MeshFilter = GetComponent<MeshFilter>();

        m_CurrentAnimIndex = -1;
        m_TimeElapsed = 0f;
        if (!m_Data)
            throw new System.Exception("Invalid Data Found Of:"+gameObject);
         
        m_MeshFilter.sharedMesh = m_Data.m_InstanceMesh;
        m_MeshRenderer.sharedMaterial.SetTexture("_AnimTex",m_Data.m_AnimationAtlas);
        m_MeshRenderer.SetPropertyBlock(m_PropertyBlock);
        return this;
    }

    public void SetAnimation(int _animIndex,float _scale=0)
    {
        if (m_CurrentAnimIndex == _animIndex)
            return;

        if (_animIndex < 0 || _animIndex >= m_Data.m_AnimationParams.Length)
            return;

        m_CurrentAnimIndex = _animIndex;
        m_TimeElapsed = m_Data.m_AnimationParams[m_CurrentAnimIndex].m_Length*_scale;
    }
    public void Replay()
    {
        m_TimeElapsed = 0f;
    }

    #region ShaderProperties
    static readonly int ID_BeginFrame = Shader.PropertyToID("_BeginFrame");
    static readonly int ID_EndFrame = Shader.PropertyToID("_EndFrame");
    static readonly int ID_FrameLerp = Shader.PropertyToID("_FrameLerp");
    #endregion
    public void Tick(float _deltaTime)
    {
        if (m_CurrentAnimIndex < 0 || m_CurrentAnimIndex >= m_Data.m_AnimationParams.Length)
            return;

        m_TimeElapsed += _deltaTime;

        AnimationInstanceParam param = m_Data.m_AnimationParams[m_CurrentAnimIndex];
        float framePassed;
        int curFrame;
        int nextFrame;
        if(param.m_Loop)
        {
            framePassed= (m_TimeElapsed%param.m_Length)*param.m_FrameRate;
            curFrame = Mathf.FloorToInt(framePassed)%param.m_FrameCount;
            nextFrame = (curFrame + 1)%param.m_FrameCount;
        }
        else
        {
            framePassed = Mathf.Min(param.m_Length, m_TimeElapsed)*param.m_FrameRate;
            curFrame = Mathf.Min(Mathf.FloorToInt(framePassed),param.m_FrameCount-1);
            nextFrame = Mathf.Min( curFrame + 1, param.m_FrameCount - 1);
        }

        curFrame += param.m_FrameBegin;
        nextFrame += param.m_FrameBegin;
        framePassed %= 1;


        m_PropertyBlock.SetInt(ID_BeginFrame, curFrame);
        m_PropertyBlock.SetInt(ID_EndFrame, nextFrame);
        m_PropertyBlock.SetFloat(ID_FrameLerp,framePassed);
        m_MeshRenderer.SetPropertyBlock(m_PropertyBlock);
    }
#if UNITY_EDITOR
    public static bool m_DrawGizmos=false;
    private void OnValidate()
    {
        if (!m_Data)
            return;
        Init();
    }
    private void OnDrawGizmos()
    {
        if (!m_DrawGizmos||!m_Data)
            return;
        Gizmos.color = Color.white;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube( m_Data.m_InstanceMesh.bounds.center,m_Data.m_InstanceMesh.bounds.size);
    }
#endif
}
