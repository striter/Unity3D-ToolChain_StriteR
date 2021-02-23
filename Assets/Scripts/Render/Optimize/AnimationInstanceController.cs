using System;
using UnityEngine;
namespace Rendering.Optimize
{

    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class AnimationInstanceController : MonoBehaviour
    {
        #region ShaderProperties
        static readonly int ID_AnimationTex = Shader.PropertyToID("_InstanceAnimationTex");
        static readonly int ID_FrameBegin = Shader.PropertyToID("_InstanceFrameBegin");
        static readonly int ID_FrameEnd = Shader.PropertyToID("_InstanceFrameEnd");
        static readonly int ID_FrameInterpolate = Shader.PropertyToID("_InstanceFrameInterpolate");
        #endregion
        public AnimationInstanceData m_Data;
        public int m_CurrentAnimIndex { get; private set; }
        public float m_TimeElapsed { get; private set; }
        public bool m_Playing => m_CurrentAnimIndex < m_Data.m_Animations.Length && m_CurrentAnimIndex >= 0;
        public AnimationInstanceParam m_CurrentAnim => m_Data.m_Animations[m_CurrentAnimIndex];
        public MeshFilter m_MeshFilter { get; private set; }
        public MeshRenderer m_MeshRenderer { get; private set; }
        Action<string> OnAnimEvent;

        protected void Awake() => OnValidate();
        public void OnValidate()
        {
            if (!m_Data)
                return;
            m_MeshFilter = GetComponent<MeshFilter>();
            m_MeshRenderer = GetComponent<MeshRenderer>();

            m_MeshFilter.sharedMesh =m_Data.m_InstancedMesh;
            m_MeshRenderer.sharedMaterial.SetTexture(ID_AnimationTex,m_Data.m_AnimationAtlas);
        }
        public AnimationInstanceController Init( Action<string> _OnAnimEvent = null)
        {
            if (!m_Data)
                throw new Exception("Invalid Data Found Of:" + gameObject);

            OnValidate();
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
            {
                Debug.LogError("Invalid Animation Index Found:" + _animIndex);
                return this;
            }

            m_CurrentAnimIndex = _animIndex;
            return this;
        }
        public void SetTime(float _time) => m_TimeElapsed = _time;
        public void SetTimeScale(float _scale)
        {
            if (m_CurrentAnimIndex < 0 || m_CurrentAnimIndex >= m_Data.m_Animations.Length)
                return;
            m_TimeElapsed = m_Data.m_Animations[m_CurrentAnimIndex].m_Length * _scale;
        }
         
        public float GetScale()
        {
            if (m_CurrentAnimIndex < 0 || m_CurrentAnimIndex >= m_Data.m_Animations.Length)
                return 0f;
            return m_TimeElapsed / m_Data.m_Animations[m_CurrentAnimIndex].m_Length;
        }
        public void Tick(float _deltaTime,MaterialPropertyBlock _block)
        {
            if (m_CurrentAnimIndex < 0 || m_CurrentAnimIndex >= m_Data.m_Animations.Length)
                return;

            AnimationInstanceParam param = m_Data.m_Animations[m_CurrentAnimIndex];
            TickEvents(param, m_TimeElapsed, _deltaTime);
            m_TimeElapsed += _deltaTime;

            float framePassed;
            int curFrame;
            int nextFrame;
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
            _block.SetInt(ID_FrameBegin, curFrame);
            _block.SetInt(ID_FrameEnd, nextFrame);
            _block.SetFloat(ID_FrameInterpolate, framePassed);
            TickBones(curFrame, nextFrame, framePassed);
        }
        #region Events
        void TickEvents(AnimationInstanceParam _clip, float _timeElapsed, float _deltaTime)
        {
            if (OnAnimEvent == null)
                return;
            float lastFrame = _timeElapsed * _clip.m_FrameRate;
            float nextFrame = lastFrame + _deltaTime * _clip.m_FrameRate;

            float checkOffset = _clip.m_Loop ? _clip.m_FrameCount * Mathf.Floor((nextFrame / _clip.m_FrameCount)) : 0;
            _clip.m_Events.Traversal(animEvent => {
                float frameCheck = checkOffset + animEvent.m_EventFrame;
                if (lastFrame < frameCheck && frameCheck <= nextFrame)
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
