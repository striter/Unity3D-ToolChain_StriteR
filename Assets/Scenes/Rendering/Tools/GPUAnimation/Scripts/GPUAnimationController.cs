using System;
using UnityEngine;
namespace Rendering.Optimize
{

    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class GPUAnimationController : MonoBehaviour
    {
        public GPUAnimationData m_Data;
        public MeshFilter m_MeshFilter { get; private set; }
        public MeshRenderer m_MeshRenderer { get; private set; }
        public Action<string> OnAnimEvent;
        public AnimationTicker m_Ticker { get; private set; }=  new AnimationTicker(); 

        protected void OnValidate() => Init();

        public void Init()
        {
            m_MeshFilter = GetComponent<MeshFilter>();
            m_MeshRenderer = GetComponent<MeshRenderer>();
            if (!m_Data || m_MeshRenderer.sharedMaterial == null)
                return;
            m_Ticker.Setup(m_Data.m_AnimationClips);
            m_MeshFilter.sharedMesh = m_Data.m_BakedMesh;
            m_Data.ApplyMaterial(m_MeshRenderer.sharedMaterial);
            InitExposeBones();
        }

        [InspectorButton]
        public GPUAnimationController SetAnimation(int _animIndex)
        {
            m_Ticker.SetAnimation(_animIndex);
            return this;
        }

        public void Tick(float _deltaTime)
        {
            if (!m_Ticker.Tick(Time.deltaTime,out var output,OnAnimEvent))
                return;
            var block = new MaterialPropertyBlock();
            output.ApplyPropertyBlock(block);
            m_MeshRenderer.SetPropertyBlock(block);
            TickExposeBones(output);
        }
        
        public void SetTime(float _time) => m_Ticker.SetTime(_time);
        public void SetTimeScale(float _scale) => m_Ticker.SetNormalizedTime(_scale);
        public float GetScale() => m_Ticker.GetNormalizedTime();
        
        #region ExposeBones
        Transform m_ExposeBoneParent;
        Transform[] m_ExposeBones;
        void InitExposeBones()
        {
            var exposeBoners = m_Data.m_ExposeTransforms;
            if (exposeBoners==null||exposeBoners.Length <= 0)
                return;
            m_ExposeBoneParent = new GameObject("Bones") { hideFlags = HideFlags.DontSave }.transform;
            m_ExposeBoneParent.SetParent(transform);
            m_ExposeBoneParent.localPosition = Vector3.zero;
            m_ExposeBoneParent.localRotation = Quaternion.identity;
            m_ExposeBoneParent.localScale = Vector3.one;
            m_ExposeBones = new Transform[m_Data.m_ExposeTransforms.Length];
            for (int i = 0; i < m_Data.m_ExposeTransforms.Length; i++)
            {
                m_ExposeBones[i] = new GameObject(m_Data.m_ExposeTransforms[i].name) { hideFlags = HideFlags.DontSave }.transform;
                m_ExposeBones[i].SetParent(m_ExposeBoneParent);
            }
        }
        void TickExposeBones(AnimationTickerOutput _output)
        {
            if (m_Data.m_ExposeTransforms == null || m_Data.m_ExposeTransforms.Length <= 0)
                return;
            for (int i = 0; i < m_Data.m_ExposeTransforms.Length; i++)
            {
                int boneIndex = m_Data.m_ExposeTransforms[i].index;
                Matrix4x4 recordMatrix = new Matrix4x4();
                recordMatrix.SetRow(0, Vector4.Lerp(ReadAnimationTexture(boneIndex, 0, _output.cur), ReadAnimationTexture(boneIndex, 0, _output.next), _output.interpolate));
                recordMatrix.SetRow(1, Vector4.Lerp(ReadAnimationTexture(boneIndex, 1, _output.cur), ReadAnimationTexture(boneIndex, 1, _output.next), _output.interpolate));
                recordMatrix.SetRow(2, Vector4.Lerp(ReadAnimationTexture(boneIndex, 2, _output.cur), ReadAnimationTexture(boneIndex, 2, _output.next), _output.interpolate));
                recordMatrix.SetRow(3, new Vector4(0, 0, 0, 1));
                m_ExposeBones[i].transform.localPosition = recordMatrix.MultiplyPoint(m_Data.m_ExposeTransforms[i].position);
                m_ExposeBones[i].transform.localRotation = Quaternion.LookRotation(recordMatrix.MultiplyVector(m_Data.m_ExposeTransforms[i].direction));
            }
        }
        Vector4 ReadAnimationTexture(int boneIndex, int row, int frame)
        {
            var pixel = UGPUAnimation.GetTransformPixel(boneIndex, row, frame);
            return m_Data.m_BakeTexture.GetPixel(pixel.x,pixel.y);
        }
        #endregion
    }

}
