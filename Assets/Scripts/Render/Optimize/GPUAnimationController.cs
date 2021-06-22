using System;
using UnityEngine;
namespace Rendering.Optimize
{
    [Serializable]
    public class GPUAnimationTimer
    {
        #region ShaderProperties
        static readonly int ID_FrameBegin = Shader.PropertyToID("_InstanceFrameBegin");
        static readonly int ID_FrameEnd = Shader.PropertyToID("_InstanceFrameEnd");
        static readonly int ID_FrameInterpolate = Shader.PropertyToID("_InstanceFrameInterpolate");
        #endregion
        public int m_AnimIndex { get; private set; }
        public float m_TimeElapsed { get; private set; }
        public AnimationInstanceParam m_Anim => m_Animations[m_AnimIndex];
        AnimationInstanceParam[] m_Animations;
        public void Setup(AnimationInstanceParam[] _params) { m_Animations = _params; }
        public void Reset()
        {
            m_AnimIndex = 0;
            m_TimeElapsed = 0;
        }

        public void SetTime(float _time) => m_TimeElapsed = _time;
        public void SetNormalizedTime(float _scale)
        {
            if (m_AnimIndex < 0 || m_AnimIndex >= m_Animations.Length)
                return;
            m_TimeElapsed = m_Animations[m_AnimIndex].m_Length * _scale;
        }

        public float GetNormalizedTime()
        {
            if (m_AnimIndex < 0 || m_AnimIndex >= m_Animations.Length)
                return 0f;
            return m_TimeElapsed / m_Animations[m_AnimIndex].m_Length;
        }
        public void SetAnimation(int _animIndex)
        {
            m_TimeElapsed = 0;
            if (_animIndex < 0 || _animIndex >= m_Animations.Length)
            {
                Debug.LogError("Invalid Animation Index Found:" + _animIndex);
                return;
            }
            m_AnimIndex = _animIndex;
        }

        public bool Tick(float _deltaTime, MaterialPropertyBlock _block, out int curFrame, out int nextFrame, out float framePassed, Action<string> _onEvents = null)
        {
            if (!Tick(_deltaTime, out curFrame, out nextFrame, out framePassed, _onEvents))
                return false;
            _block.SetInt(ID_FrameBegin, curFrame);
            _block.SetInt(ID_FrameEnd, nextFrame);
            _block.SetFloat(ID_FrameInterpolate, framePassed);
            return true;
        }
        public bool Tick(float _deltaTime, out int curFrame, out int nextFrame, out float framePassed, Action<string> _onEvents = null)
        {
            curFrame = 0;
            nextFrame = 0;
            framePassed = 0;
            if (m_AnimIndex < 0 || m_AnimIndex >= m_Animations.Length)
                return false;

            AnimationInstanceParam param = m_Animations[m_AnimIndex];
            if (_onEvents != null)
                TickEvents(param, m_TimeElapsed, _deltaTime, _onEvents);
            m_TimeElapsed += _deltaTime;

            if (param.m_Loop)
            {
                framePassed = (m_TimeElapsed % param.m_Length) * param.m_FrameRate;
                curFrame = Mathf.FloorToInt(framePassed) % param.m_FrameCount;
                nextFrame = (curFrame + 1) % param.m_FrameCount;
            }
            else
            {
                framePassed = Mathf.Min(param.m_Length, m_TimeElapsed) * param.m_FrameRate;
                curFrame = Mathf.Min(Mathf.FloorToInt(framePassed), param.m_FrameCount - 1);
                nextFrame = Mathf.Min(curFrame + 1, param.m_FrameCount - 1);
            }

            curFrame += param.m_FrameBegin;
            nextFrame += param.m_FrameBegin;
            framePassed %= 1;
            return true;
        }

        void TickEvents(AnimationInstanceParam _clip, float _timeElapsed, float _deltaTime,Action<string> _onEvents)
        {
            float lastFrame = _timeElapsed * _clip.m_FrameRate;
            float nextFrame = lastFrame + _deltaTime * _clip.m_FrameRate;

            float checkOffset = _clip.m_Loop ? _clip.m_FrameCount * Mathf.Floor((nextFrame / _clip.m_FrameCount)) : 0;
            foreach (AnimationInstanceEvent animEvent in _clip.m_Events)
            {
                float frameCheck = checkOffset + animEvent.m_EventFrame;
                if (lastFrame < frameCheck && frameCheck <= nextFrame)
                    _onEvents(animEvent.m_EventIdentity);
            }
        }
    }

    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class GPUAnimationController : MonoBehaviour
    {
        #region ShaderProperties
        static readonly int ID_AnimationTex = Shader.PropertyToID("_InstanceAnimationTex");
        #endregion
        public GPUAnimationData m_Data;
        public GPUAnimationTimer m_Timer { get; private set; } = new GPUAnimationTimer(); 
        public MeshFilter m_MeshFilter { get; private set; }
        public MeshRenderer m_MeshRenderer { get; private set; }
        Action<string> OnAnimEvent;

        protected void Awake() => OnValidate();
        public void OnValidate()
        {
            if (!m_Data)
                return;
            m_Timer.Setup(m_Data.m_Animations);
            m_MeshFilter = GetComponent<MeshFilter>();
            m_MeshRenderer = GetComponent<MeshRenderer>();

            m_MeshFilter.sharedMesh =m_Data.m_InstancedMesh;
            m_MeshRenderer.sharedMaterial.SetTexture(ID_AnimationTex,m_Data.m_AnimationAtlas);
        }
        public GPUAnimationController Init( Action<string> _OnAnimEvent = null)
        {
            if (!m_Data)
                throw new Exception("Invalid Data Found Of:" + gameObject);

            OnValidate();
            m_Timer.Reset();
            InitBones();
            OnAnimEvent = _OnAnimEvent;
            return this;
        }
        public GPUAnimationController SetAnimation(int _animIndex)
        {
            m_Timer.SetAnimation(_animIndex);
            return this;
        }
        public void SetTime(float _time) => m_Timer.SetTime(_time);
        public void SetTimeScale(float _scale) => m_Timer.SetNormalizedTime(_scale);
        public float GetScale() => m_Timer.GetNormalizedTime();
        public void Tick(float _deltaTime,MaterialPropertyBlock _block)
        {
            if (!m_Timer.Tick(_deltaTime,_block,out int curFrame,out int nextFrame,out float framePassed,OnAnimEvent))
                return;
            
            TickBones(curFrame, nextFrame, framePassed);
        }
        #region Bones
        Transform m_BoneParent;
        Transform[] m_Bones;
        void InitBones()
        {
            if (m_Data.m_ExposeBones.Length <= 0)
                return;
            m_BoneParent = new GameObject("Bones") { hideFlags = HideFlags.DontSave }.transform;
            m_BoneParent.SetParent(transform);
            m_BoneParent.localPosition = Vector3.zero;
            m_BoneParent.localRotation = Quaternion.identity;
            m_BoneParent.localScale = Vector3.one;
            m_Bones = new Transform[m_Data.m_ExposeBones.Length];
            for (int i = 0; i < m_Data.m_ExposeBones.Length; i++)
            {
                m_Bones[i] = new GameObject(m_Data.m_ExposeBones[i].m_BoneName) { hideFlags = HideFlags.DontSave }.transform;
                m_Bones[i].SetParent(m_BoneParent);
            }
        }
        void TickBones(int curFrame, int nextFrame, float frameLerp)
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
                m_Bones[i].transform.localPosition = recordMatrix.MultiplyPoint(m_Data.m_ExposeBones[i].m_Position);
                m_Bones[i].transform.localRotation = Quaternion.LookRotation(recordMatrix.MultiplyVector(m_Data.m_ExposeBones[i].m_Direction));
            }
        }
        Vector4 ReadAnimationTexture(int boneIndex, int row, int frame)
        {
            return m_Data.m_AnimationAtlas.GetPixel(boneIndex * 3 + row, frame);
        }
        #endregion
    }

}
