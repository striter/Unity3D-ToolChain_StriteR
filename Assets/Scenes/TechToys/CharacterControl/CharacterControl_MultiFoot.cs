using System;
using System.Linq.Extensions;
using TechToys.CharacterControl.InverseKinematics;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace TechToys.CharacterControl
{
    [ExecuteInEditMode]
    public class CharacterControl_MultiFoot :  MonoBehaviour , ICharacterControlMgr
    {
        public float m_Speed = 5f;
        public float3 m_DesireRotation;
        public float m_DesireSpeed;
        [Header("Damping")] 
        public Damper m_SpeedDamper = Damper.kDefault;
        public Damper m_AngleDamper = Damper.kDefault;
        private AInverseKinematic[] m_InverseKinematics;
        public void Initialize()
        {        
            var finalPos = transform.position;
            if (NavMesh.SamplePosition(finalPos, out var hit, 3f, NavMesh.AllAreas))
                transform.position = hit.position;
            m_InverseKinematics = GetComponentsInChildren<AInverseKinematic>();
            m_InverseKinematics.Traversal(p=>p.Initialize());
        }

        public void Dispose()
        {
            m_InverseKinematics.Traversal(p=>p.UnInitialize());
            m_InverseKinematics = null;
        }

        public void Tick(float _deltaTime)
        {
            var input = CharacterControl_Input.Instance.m_Input;
            CharacterControl_Input.Instance.ClearInput();
            var move = input.move;
            var aim = input.aim;

            var moving = move.sqrmagnitude() > 0f;
            
            var baseRotation = CharacterControl_Camera.Instance.m_CameraInput.euler.setX(0);
            m_DesireSpeed = 0f;
            if (moving)
                m_DesireSpeed = m_Speed;
            
            if(aim)
                m_DesireRotation = baseRotation;
            
            
            var finalSpeed = m_SpeedDamper.Tick(_deltaTime,m_DesireSpeed);
            
            var rotation = quaternion.Euler(baseRotation * kmath.kDeg2Rad);
            var right = math.mul(rotation,kfloat3.right);
            var forward = math.mul(rotation,kfloat3.forward);

            var finalPos = transform.position + (Vector3)(forward * move.y + right * move.x) * finalSpeed * _deltaTime;
            if (NavMesh.SamplePosition(finalPos, out var hit, 3f, NavMesh.AllAreas))
                finalPos = hit.position;

            transform.position = finalPos;
            transform.rotation = quaternion.Euler(m_AngleDamper.TickAngle(_deltaTime, m_DesireRotation) * kmath.kDeg2Rad);
        }

        public void LateTick(float _deltaTime)
        {
            m_InverseKinematics.Traversal(p=>p.Tick(_deltaTime));
        }
    }
}