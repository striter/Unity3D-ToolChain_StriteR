using System;
using System.Linq.Extensions;
using TechToys.CharacterControl.InverseKinematics;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace TechToys.CharacterControl
{
    public class CharacterControl_Character : MonoBehaviour , ICharacterControl
    {
        public float m_Speed = 5f;
        public float m_SprintMultiplier = 2f;

        public float3 m_DesireRotation;
        public float m_DesireSpeed;

        public AInverseKinematic[] m_InverseKinematics;
        
        [Header("Damping")] 
        public Damper m_SpeedDamper = Damper.kDefault;
        public Damper m_AngleDamper = Damper.kDefault;
        public void Initialize()
        {
            Reset();
            m_InverseKinematics = GetComponentsInChildren<AInverseKinematic>();
            m_InverseKinematics.Traversal(p=>p.Initialize());
        }

        public void Dispose()
        {
            m_InverseKinematics.Traversal(p=>p.UnInitialize());
        }

        private Animator m_Animator;
        private static readonly int kSpeed = Animator.StringToHash("fSpeed");
        public void Tick(float _deltaTime)
        {
            var input = CharacterControl_Input.Instance.m_Input;

            var move = input.move;
            var sprint = input.sprint;
            var aim = input.aim;

            var moving = move.sqrmagnitude() > 0f;
            
            var baseRotation = CharacterControl_Camera.Instance.m_CameraInput.euler.setX(0);
            m_DesireSpeed = 0f;
            if (moving)
                m_DesireSpeed = m_Speed * (sprint ? m_SprintMultiplier : 1f);
            
            if(aim || moving)
                m_DesireRotation = baseRotation + new float3(0,-umath.getRadClockwise(move,kfloat2.up) * kmath.kRad2Deg,0);
            
            var backward = aim && move.y <= 0 && moving;
            if (backward)
            {
                m_DesireSpeed = -m_Speed;
                move = -move;
                m_DesireRotation = baseRotation + new float3(0,-umath.getRadClockwise(move,kfloat2.up) * kmath.kRad2Deg,0);
            }
            
            transform.rotation = quaternion.Euler(m_AngleDamper.TickAngle(_deltaTime, m_DesireRotation) * kmath.kDeg2Rad);
            
            m_Animator??= GetComponentInChildren<Animator>();
            
            var finalSpeed = m_SpeedDamper.Tick(_deltaTime,m_DesireSpeed);
            
            var finalPos = transform.position + transform.forward * finalSpeed * _deltaTime;
            if(NavMesh.SamplePosition(finalPos,out var hit,1f,NavMesh.AllAreas))
                transform.position = hit.position;
            
            m_Animator.SetFloat(kSpeed, finalSpeed / m_Speed);
        }

        
        public void LateTick(float _deltaTime)
        {
            m_InverseKinematics.Collect(p=>p.Valid).Traversal(p=>p.Tick(_deltaTime));
        }


        [InspectorButtonRuntime]
        public void Reset()
        {
            m_DesireSpeed = 0f;
            m_SpeedDamper.Initialize(m_DesireSpeed);
            m_DesireRotation = transform.eulerAngles;
            m_AngleDamper.Initialize(quaternion.Euler(m_DesireRotation * kmath.kDeg2Rad));
            m_InverseKinematics?.Traversal(p=>p.Reset());
        }
    }
}