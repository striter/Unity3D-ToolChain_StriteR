using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsTest
{
    public class GravityGunCharacter : PhysicsCharacterBase
    {
        public Transform m_Head;
        public CharacterController m_Body;
        public Transform m_GravityPoint;
        public float m_GravityGunAvailableDistance = 20f;
        public float m_GravityBurstForce = 2000f;
        public float m_GravitySuckForce = 500f;

        bool m_AltFiring;
        Rigidbody m_TargetObject;
        Timer m_GravityGunCooldown = new Timer(.35f);
        bool m_Sprinting;
        Timer m_JumpTimer = new Timer(.5f,true);
        void OnJump()
        {
            if (!m_Body.isGrounded)
                return;
            m_JumpTimer.Replay();
        }
        void OnSprint(bool _sprint) => m_Sprinting = _sprint;
        public override void OnTakeControl()
        {
            base.OnTakeControl();
            TPSCameraController.Instance.m_BindRoot = m_Head;
            TPSCameraController.Instance.m_BindPosOffset = Vector3.zero;
            TPSCameraController.Instance.m_MoveDamping = 0f;
            TPSCameraController.Instance.m_RotateDamping = 0f;
            PCInputManager.Instance.GetKeyBinding(enum_Binding.AltFire).Add(OnAltFire);
            PCInputManager.Instance.GetKeyBinding(enum_Binding.MainFire).Add(OnMainFire);
            PCInputManager.Instance.GetKeyBinding(enum_Binding.Jump).Add(OnJump);
            PCInputManager.Instance.GetKeyBinding(enum_Binding.Sprint).Add(OnSprint);
        }
        public override void OnRemoveControl()
        {
            base.OnRemoveControl();
            PCInputManager.Instance.GetKeyBinding(enum_Binding.AltFire).Remove(OnAltFire);
            PCInputManager.Instance.GetKeyBinding(enum_Binding.MainFire).Remove(OnMainFire);
            PCInputManager.Instance.GetKeyBinding(enum_Binding.Jump).Remove(OnJump);
            PCInputManager.Instance.GetKeyBinding(enum_Binding.Sprint).Remove(OnSprint);
        }

        public override void Tick(float _deltaTime)
        {
            m_JumpTimer.Tick(_deltaTime);
            m_Head.position = m_Body.transform.position + Vector3.up * .9f;
            if (m_TargetObject)
            {
                m_TargetObject.MovePosition(Vector3.Lerp(m_TargetObject.transform.position, m_GravityPoint.position, _deltaTime * 10f));
                m_TargetObject.velocity = Vector3.zero;
            }
            m_Head.rotation = TickRotation();
            m_Body.Move((TickMovement()*(m_Sprinting?3f:1f) + Vector3.up* Mathf.Lerp(-9.8f,9.8f, m_JumpTimer.m_TimeLeftScale))*_deltaTime);
        }
        public override void FixedTick(float _deltaTime)
        {
            if (m_TargetObject)
                return;

            m_GravityGunCooldown.Tick(_deltaTime);
            if (m_GravityGunCooldown.m_Timing||!m_AltFiring)
                return;

            if (!Physics.Raycast(m_Head.position, m_Head.forward, out RaycastHit _hit, float.MaxValue, -1) || !TargetInteractable(_hit.collider, out Rigidbody suckTarget))
                return;
            if(Vector3.Distance(m_GravityPoint.transform.position,suckTarget.transform.position)<m_GravityGunAvailableDistance)
            {
                FocusTarget(suckTarget);
                return;
            }

            suckTarget.AddForceAtPosition(-m_GravitySuckForce * (_hit.point - m_GravityPoint.transform.position).normalized,_hit.point);
        }

        void OnAltFire(bool altFire)
        {
            if (altFire&&m_TargetObject)
            {
                m_GravityGunCooldown.Replay();
                ReleaseTarget();
                return;
            }
            m_AltFiring=altFire;
        }

        void OnMainFire()
        {
            ReleaseTarget();
            m_GravityGunCooldown.Replay();
            Rigidbody burstTarget = m_TargetObject;
            Vector3 hitPosition = m_TargetObject?m_TargetObject.transform.position:Vector3.zero;
            if(burstTarget==null)
            {
                if (!Physics.Raycast(m_Head.position, m_Head.forward, out RaycastHit _hit, m_GravityGunAvailableDistance, -1) || !TargetInteractable(_hit.collider, out burstTarget))
                    return;
                hitPosition = burstTarget.position;
            }
            burstTarget.AddForceAtPosition(burstTarget.mass*m_Head.forward*m_GravityBurstForce, hitPosition, ForceMode.Force);
        }

        void FocusTarget(Rigidbody _target)
        {
            ReleaseTarget();
            m_TargetObject = _target;
            m_TargetObject.useGravity = false;
        }
        void ReleaseTarget()
        {
            if (!m_TargetObject)
                return;

            m_TargetObject.useGravity = true;
            m_TargetObject = null;
        }

        bool TargetInteractable(Collider _target,out Rigidbody _rigidbody)
        {
            _rigidbody = _target.GetComponent<Rigidbody>();
            return _rigidbody != null && _rigidbody.mass < 20f&&!_rigidbody.isKinematic;
        }

    }
}