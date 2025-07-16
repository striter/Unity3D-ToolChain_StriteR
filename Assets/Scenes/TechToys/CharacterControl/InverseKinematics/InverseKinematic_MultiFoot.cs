using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace TechToys.CharacterControl.InverseKinematics
{
    [ExecuteInEditMode]
    public class InverseKinematic_MultiFoot : AInverseKinematic
    {
        public Transform m_Root;
        public Transform m_Pelvis;
        public Damper m_PelvisPositionDamper = Damper.kDefault;
        public Damper m_PelvisRotationDamper = Damper.kDefault;
        public float m_VerticalOffset = 0f;

        private List<FootControlDamper> m_ControlPointDampers = new List<FootControlDamper>();
        public FootControlData m_Data = FootControlData.kDefault;
        public override bool Valid => m_ControlPointDampers is { Count: > 0 } && m_Pelvis != null && m_Root != null; 

        public override void Initialize()
        {
            m_Pelvis.transform.SetLocalPositionAndRotation(new Vector3(0f,m_VerticalOffset,0f),quaternion.identity);
            GetComponentsInChildren<InverseKinematic_SimpleIK>(false).Select(ik => new FootControlDamper(ik,m_Root)).FillList(m_ControlPointDampers);
            m_ControlPointDampers.Traversal(p=>p.Initialize(m_Data));
            Reset();
        }

        public override void UnInitialize()
        {
            m_ControlPointDampers.Clear();
        }

        int kTickIndex = 0;
        public override void Tick(float _deltaTime)
        {
            m_Pelvis.transform.SetLocalPositionAndRotation(new Vector3(0f,m_VerticalOffset,0f),quaternion.identity);
            var count = m_ControlPointDampers.Count;
            kTickIndex = (kTickIndex + 1) % m_ControlPointDampers.Count;
            for (var i = 0; i < count; i++)
            {
                var index = (i + kTickIndex) % count;
                var oppositeIndex = (index + count / 2) % count;
                m_ControlPointDampers[index].Tick(_deltaTime, !m_ControlPointDampers[oppositeIndex].Moving);
            }
            
            CollectPelvisPositionRotation(out var position, out var rotation);
            m_Pelvis.transform.SetPositionAndRotation(m_PelvisPositionDamper.Tick(_deltaTime, position),m_PelvisRotationDamper.Tick(_deltaTime, rotation));
          }

        void CollectPelvisPositionRotation(out float3 position,out quaternion rotation)
        {
            var positionAffectionWS = float3.zero;
            var normalWS = float3.zero;
            var count = m_ControlPointDampers.Count;
            for (var i = 0; i < count; i++)
            {
                positionAffectionWS += m_ControlPointDampers[i].PositionOffsetWS;
                normalWS += m_ControlPointDampers[i].RotationOffsetWS;
            }
            positionAffectionWS /= count;
            normalWS = normalWS.normalize();
            rotation = quaternion.LookRotation(math.cross(transform.right,normalWS),normalWS);
            position = (float3)m_Root.position + positionAffectionWS + normalWS * m_VerticalOffset;
        }
        
        [InspectorButton]
        public override void Reset()
        {
            if (!Valid)
                return;
            
            m_Pelvis.transform.SetLocalPositionAndRotation(new Vector3(0f,m_VerticalOffset,0f),quaternion.identity);
            m_ControlPointDampers.Traversal(p=>p.Reset());
            CollectPelvisPositionRotation(out var position, out var rotation);
            m_PelvisPositionDamper.Initialize(position);
            m_PelvisRotationDamper.Initialize(rotation);
        }

        private void OnDrawGizmos()
        {
            if (!Valid)
                return;
            
            Gizmos.matrix = Matrix4x4.identity;
            m_ControlPointDampers.Traversal(p=>p.DrawGizmos());
        }

        private void Awake() { if (!Application.isPlaying) Initialize(); }
        private void OnDestroy() { if (!Application.isPlaying) return; UnInitialize(); }
        private void Update() { if (!Application.isPlaying && Valid) Tick(UTime.deltaTime); }
        private void OnValidate()
        {
            if (Application.isPlaying)
                return;
            UnInitialize();
            Initialize();
        }
        
        [Serializable]
        public struct FootControlData
        {
            public float extrude;
            public CullingMask cullingMask;
            public RangeFloat topDownRayOffset;
            public float tolerance;
            public Damper damper;
            public static readonly FootControlData kDefault = new FootControlData
            {
                extrude = 0.2f,
                tolerance = 0.05f, 
                damper = Damper.kDefault,
                cullingMask = -1,
                topDownRayOffset = new RangeFloat(-.2f, 0.4f),
            }; 
        }
        
        [Serializable]
        public class FootControlDamper
        {
            [field : SerializeField]public InverseKinematic_SimpleIK m_IK { get; private set; }
            public bool Moving => m_Data.damper.velocity.sqrmagnitude() > 0.01f;
            private float3 m_AnchorPosition;
            private FootControlData m_Data;
            private Transform m_Root;
            private RaycastHit m_HitInfo;
            public float3 PositionOffsetWS => CurrentPositionWS - DesirePositionWS;
            public float3 CurrentPositionWS =>  m_HitInfo.collider == null ? DesirePositionWS : m_HitInfo.point;
            public float3 RotationOffsetWS => m_HitInfo.collider == null ? kfloat3.up : m_HitInfo.normal;
            public float3 DesirePositionWS => m_Root.localToWorldMatrix.MultiplyPoint(m_RootPositionLS.setY(0f)) + m_Root.localToWorldMatrix.MultiplyVector(m_ExtrudeDirectionLS) * m_Data.extrude;
            public GLine HitDetectionTopDownRayWS => new (DesirePositionWS - (float3)m_Root.up * m_Data.topDownRayOffset.start, DesirePositionWS - (float3)m_Root.up *m_Data.topDownRayOffset.end);
            public GLine HitDetectionSideRayWS => new GLine(m_Root.localToWorldMatrix.MultiplyPoint(m_RootPositionLS),
                DesirePositionWS + (float3)m_Root.up * new Vector3(0f, m_RootPositionLS.y, 0f));
            private float3 m_RootPositionLS = float3.zero;
            private float3 m_ExtrudeDirectionLS = float3.zero;
            public FootControlDamper(InverseKinematic_SimpleIK _ik,Transform _root)
            {
                m_Root = _root;
                m_IK = _ik;

                m_RootPositionLS = _root.worldToLocalMatrix.MultiplyPoint(_ik.transform.position);
                m_ExtrudeDirectionLS = _root.worldToLocalMatrix.MultiplyVector(_ik.transform.right);
                m_AnchorPosition = m_IK.m_Evaluate;
            }

            public FootControlDamper Initialize(FootControlData _data)
            {
                m_Data = _data;
                return this;
            }
            public void Tick(float _deltaTime,bool _validMove)
            {
                if (_validMove)
                {
                    if (!Physics.Raycast(HitDetectionSideRayWS.start, HitDetectionSideRayWS.direction, out m_HitInfo, HitDetectionSideRayWS.length, m_Data.cullingMask))
                        Physics.Raycast(HitDetectionTopDownRayWS.start, HitDetectionTopDownRayWS.direction, out m_HitInfo, HitDetectionTopDownRayWS.length, m_Data.cullingMask);
                    
                    if (Vector3.Distance(m_AnchorPosition, CurrentPositionWS) > m_Data.tolerance)
                        m_AnchorPosition = CurrentPositionWS;
                }

                m_IK.m_Evaluate = m_Data.damper.Tick(_deltaTime,m_AnchorPosition);
            }

            public void Reset()
            {
                Tick(0f,true);
                m_IK.m_Evaluate = CurrentPositionWS;
                m_Data.damper.Initialize(CurrentPositionWS);
            }

            public void DrawGizmos()
            {
                Gizmos.color = Color.white.SetA(.3f);
                HitDetectionTopDownRayWS.DrawGizmos();
                HitDetectionSideRayWS.DrawGizmos();
                Gizmos.color = Color.white;
                var extrudePositionWS = m_Root.localToWorldMatrix.MultiplyPoint(m_RootPositionLS); 
                Gizmos.DrawWireSphere(extrudePositionWS,.05f);
                Gizmos.DrawLine(extrudePositionWS,DesirePositionWS);
                Gizmos.DrawWireSphere(DesirePositionWS,0.05f);
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(m_AnchorPosition,0.05f);
                Gizmos.DrawLine(DesirePositionWS,m_AnchorPosition);
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(CurrentPositionWS,0.05f);
                Gizmos.DrawLine(CurrentPositionWS,m_AnchorPosition);
            }
        }
    
    }
    
}