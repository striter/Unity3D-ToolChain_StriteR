using System.Collections.Generic;
using System.Linq.Extensions;
using Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace Examples.Tools.Optimize.GPUSkinning
{
    [RequireComponent(typeof(MeshFilter)),ExecuteInEditMode]
    public class GPUSkinningBehaviour : MonoBehaviour
    {
        public GPUSkinningData m_Data;
        public Transform m_RootBone;
        private MeshRenderer m_MeshRenderer;
        private MeshFilter m_MeshFilter;
        private MaterialPropertyBlock m_Block;
        private List<Transform> m_Bones = new();
        private GraphicsBuffer m_Buffer;
        [Readonly] public GBox m_Bounds = GBox.kZero;
        [Readonly] public List<float4x4> m_SamplingMatrices = new List<float4x4>();
        private static readonly int kBoneMatricesID = Shader.PropertyToID("_BoneMatrices");

        private void OnValidate() => Validate();
        
        private bool Validate()
        {
            if (m_RootBone == null || m_Data == null || !m_Data.Valid())
                return false;
            
            m_MeshFilter ??= GetComponent<MeshFilter>();
            if (m_MeshFilter == null)
                return false;
            if (m_MeshFilter.sharedMesh != m_Data.m_Mesh)
                m_MeshFilter.sharedMesh = m_Data.m_Mesh;
            m_Block ??= new MaterialPropertyBlock();
            m_Buffer ??= new GraphicsBuffer(GraphicsBuffer.Target.Structured,m_Data.m_Bones.Count, sizeof(float) * 16);
            if (m_Bones.Count != m_Data.m_Bones.Count)
            {
                m_Buffer.Dispose();
                m_Buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured,m_Data.m_Bones.Count, sizeof(float) * 16);
                m_Bones.Clear();
                for (var i = 0; i < m_Data.m_Bones.Count; i++)
                {
                    var bone = m_Data.m_Bones[i];
                    var boneTransform = m_RootBone.Find(bone.relativePath);
                    if (boneTransform == null)
                    {
                        m_Bones.Clear();
                        Debug.LogError($"{nameof(GPUSkinningBehaviour)}Bone Not Found:" + bone.relativePath);
                        return false;
                    }
                    m_Bones.Add(boneTransform);
                }
            }
            
            m_MeshRenderer = m_MeshFilter.GetComponent<MeshRenderer>();
            m_MeshRenderer.SetPropertyBlock(m_Block);
            
            return m_MeshRenderer != null;
        }
        private void OnDestroy()
        {
            m_Buffer.Dispose();
            m_Buffer = null;
        }
        private void LateUpdate()
        {
            if (!Validate())
                return;
            
            var transformCount = m_Bones.Count;
            
            m_SamplingMatrices.Resize(transformCount);
            var bounds = new GBox(m_Bones[0].position,0f);
            for (var i = 0; i < transformCount; i++)
            {
                var boneData = m_Data.m_Bones[i];
                var bindPose = boneData.bindPose;
                var bindPoseToBoneAnimated = transform.worldToLocalMatrix * m_Bones[i].localToWorldMatrix * bindPose;
                m_SamplingMatrices[i] = bindPoseToBoneAnimated;
                var boneBounds = boneData.bounds;
                bounds = bounds.Encapsulate(new GSphere(m_Bones[i].transform.localToWorldMatrix.MultiplyPoint(boneBounds.center), ((float3)m_Bones[i].lossyScale).maxElement() * boneBounds.radius));
            }

            m_Bounds = bounds;
            m_MeshRenderer.bounds = m_Bounds;
            m_Buffer.SetData(m_SamplingMatrices);
            m_Block.SetBuffer(kBoneMatricesID,m_Buffer);
            m_MeshRenderer.SetPropertyBlock(m_Block);
        }

        public bool m_BoundsGizmos;
        private void OnDrawGizmos()
        {
            if (!m_BoundsGizmos)
                return;
            if (!Validate())
                return;

            Gizmos.matrix = Matrix4x4.identity;
            m_Bounds.DrawGizmos();
            for (var i = 0; i < m_Bones.Count; i++)
            {
                var boneBounds = m_Data.m_Bones[i].bounds;
                new GSphere(m_Bones[i].transform.localToWorldMatrix.MultiplyPoint(boneBounds.center), ((float3)m_Bones[i].lossyScale).maxElement() * boneBounds.radius).DrawGizmos();
            }
        }
    }
}