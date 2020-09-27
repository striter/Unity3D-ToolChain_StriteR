using System;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshRenderer),typeof(MeshFilter))]
public class AnimationInstanceController : MonoBehaviour
{
    #region ShaderProperties
    static readonly int ID_AnimTex = Shader.PropertyToID("_AnimTex");
    static readonly int ID_BeginFrame = Shader.PropertyToID("_BeginFrame");
    static readonly int ID_EndFrame = Shader.PropertyToID("_EndFrame");
    static readonly int ID_FrameLerp = Shader.PropertyToID("_FrameLerp");
    #endregion
    public AnimationInstanceData m_Data;
    public int m_CurrentAnimIndex { get; private set; }
    public float m_TimeElapsed { get; private set; }
    MeshRenderer m_MeshRenderer;
    MeshFilter m_MeshFilter;
    MaterialPropertyBlock m_PropertyBlock;
    Texture2D m_AnimAtlas;
    Action<string> OnAnimEvent;
    public AnimationInstanceController Init(Action<string> _OnAnimEvent=null)
    {
        if (!m_Data)
            throw new System.Exception("Invalid Data Found Of:" + gameObject);

        m_PropertyBlock = new MaterialPropertyBlock();
        m_MeshRenderer = GetComponent<MeshRenderer>();
        m_MeshFilter = GetComponent<MeshFilter>();

        m_AnimAtlas = m_MeshRenderer.sharedMaterial.GetTexture(ID_AnimTex) as Texture2D;
        m_CurrentAnimIndex = -1;
        m_TimeElapsed = 0f;
        InitBones();
        OnAnimEvent = _OnAnimEvent;
        return this;
    }

    public AnimationInstanceController SetAnimation(int _animIndex)
    {
        m_TimeElapsed = 0;
        if (_animIndex < 0 || _animIndex >= m_Data.m_Animations.Length)
            throw new System.Exception("Invalid Animation Index Found:"+_animIndex);

        m_CurrentAnimIndex = _animIndex;
        return this;
    }
    public void SetTime(float _time) => m_TimeElapsed = _time;
    public void SetScale(float _scale)
    {
        if (m_CurrentAnimIndex < 0 || m_CurrentAnimIndex >= m_Data.m_Animations.Length)
            return;
        m_TimeElapsed = m_Data.m_Animations[m_CurrentAnimIndex].m_Length * _scale;
    }

    public void Tick(float _deltaTime)
    {
        if (m_CurrentAnimIndex < 0 || m_CurrentAnimIndex >= m_Data.m_Animations.Length)
            return;

        AnimationInstanceParam param = m_Data.m_Animations[m_CurrentAnimIndex];
        TickEvents(param.m_FrameBegin+m_TimeElapsed*param.m_FrameRate,_deltaTime*param.m_FrameRate);
        m_TimeElapsed += _deltaTime;

        float framePassed;
        int curFrame;
        int nextFrame;
        if (param.m_Loop)
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
        TickBones(curFrame, nextFrame, framePassed);
    }
    #region Events
    void TickEvents(float _preFrame,float _deltaFrame)
    {
        if (OnAnimEvent == null)
            return;
        float _nextFrame = _preFrame + _deltaFrame;

        m_Data.m_Events.Traversal(animEvent => {
            if (_preFrame < animEvent.m_EventFrame && animEvent.m_EventFrame <= _nextFrame)
                OnAnimEvent(animEvent.m_EventIdentity);
        });
    }
    #endregion
    #region Bones
    Transform m_BoneParent;
    Transform[] m_Bones;
    void InitBones()
    {
        if (m_Data.m_ExposeBones.Length <= 0)
            return;
        m_BoneParent = new GameObject("Bones").transform;
        m_BoneParent.SetParent(transform);
        m_BoneParent.localPosition = Vector3.zero;
        m_BoneParent.localRotation = Quaternion.identity;
        m_BoneParent.localScale = Vector3.one;

        m_Bones = new Transform[m_Data.m_ExposeBones.Length];
        for (int i = 0; i < m_Data.m_ExposeBones.Length; i++)
        {
            m_Bones[i] = new GameObject(m_Data.m_ExposeBones[i].m_BoneName).transform;
            m_Bones[i].SetParent(m_BoneParent);
        }
    }
    void TickBones(int curFrame,int nextFrame,float frameLerp)
    {
        if (m_Data.m_ExposeBones.Length <= 0)
            return;
        for (int i = 0; i < m_Data.m_ExposeBones.Length; i++)
        {
            int boneIndex = m_Data.m_ExposeBones[i].m_BoneIndex;
            Matrix4x4 recordMatrix = new Matrix4x4();
            recordMatrix.SetRow(0, Vector4.Lerp(ReadAnimationTexture(boneIndex, 0, curFrame), ReadAnimationTexture(boneIndex, 0, nextFrame), frameLerp));
            recordMatrix.SetRow(1, Vector4.Lerp(ReadAnimationTexture(boneIndex, 1, curFrame), ReadAnimationTexture(boneIndex, 1, nextFrame), frameLerp));
            recordMatrix.SetRow(2, Vector4.Lerp(ReadAnimationTexture(boneIndex, 2, curFrame), ReadAnimationTexture(boneIndex, 2, nextFrame), frameLerp));
            recordMatrix.SetRow(3, new Vector4(0, 0, 0, 1));
            m_Bones[i].transform.localPosition =  recordMatrix.MultiplyPoint(m_Data.m_ExposeBones[i].m_Position);
            m_Bones[i].transform.localRotation =Quaternion.LookRotation(recordMatrix.MultiplyVector( m_Data.m_ExposeBones[i].m_Direction));
        }
    }
     Vector4 ReadAnimationTexture(int boneIndex,int row,int frame)
    {
        return m_AnimAtlas.GetPixel(boneIndex*3+row,frame);
    }
    #endregion


#if UNITY_EDITOR
    public static bool m_DrawGizmos=false;
    private void OnDrawGizmos()
    {
        if (!m_DrawGizmos||!m_Data)
            return;
        Gizmos.color = Color.white;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube( m_MeshFilter.sharedMesh.bounds.center, m_MeshFilter.sharedMesh.bounds.size);
    }
#endif
}
