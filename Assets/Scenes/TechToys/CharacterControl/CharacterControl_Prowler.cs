using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using TechToys.CharacterControl.InverseKinematics;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace TechToys.CharacterControl
{
    //https://x.com/CodeerDev/status/1244995268065538048?s=20
    [ExecuteInEditMode]
    public class CharacterControl_Prowler :  MonoBehaviour , ICharacterControl
    {
        [Readonly] public List<InverseKinematic_SimpleIK> m_InverseKinematics;

        public float m_Extrude;
        public float m_Tolerance;
        public void Initialize()
        {
            m_InverseKinematics = GetComponentsInChildren<InverseKinematic_SimpleIK>().ToList();
        }

        public void Dispose()
        {
        }

        public void Tick(float _deltaTime)
        {
            var finalPos = transform.position;
            if(NavMesh.SamplePosition(finalPos,out var hit,1f,NavMesh.AllAreas))
                transform.position = hit.position;
            foreach (var ik in m_InverseKinematics)
            {
                var extrudePosition = ik.transform.position + ik.transform.right * m_Extrude;
                var nextPosition = ik.m_Evaluate;
                if (Physics.Raycast(extrudePosition, Vector3.down, out var hit2, 3f, -1))
                    nextPosition = hit2.point;
                if (Vector3.Distance(ik.m_Evaluate, nextPosition) > m_Tolerance)
                    ik.m_Evaluate = nextPosition;
            }
        }

        public void LateTick(float _deltaTime)
        {
            m_InverseKinematics.Traversal(p=>p.Tick(_deltaTime));
        }

        private void OnDrawGizmos()
        {
            Gizmos.matrix = Matrix4x4.identity;
            foreach (var ik in m_InverseKinematics)
            {
                var extrudePosition = ik.transform.position + ik.transform.right * m_Extrude;
                Gizmos.DrawSphere(extrudePosition,0.1f);
                var desirePosition = ik.m_Evaluate;
                if (Physics.Raycast(extrudePosition, Vector3.down, out var hit, 3f, -1))
                    desirePosition = hit.point;
                Gizmos.DrawLine(ik.m_Evaluate,desirePosition);
            }
        }

        private void Awake() { if (!Application.isPlaying) Initialize(); }
        private void OnDestroy() { if (!Application.isPlaying) return; Dispose(); }
        private void LateUpdate() { if(!Application.isPlaying) LateTick(UTime.deltaTime); }
        private void Update() { if (!Application.isPlaying) Tick(UTime.deltaTime); }
    }
}