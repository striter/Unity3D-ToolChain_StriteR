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
        LineRenderer m_GravityLine;
        readonly Counter m_GravityGunCounter = new Counter(.35f);
        public override void OnTakeControl(TPSCameraController _controller)
        {
            base.OnTakeControl(_controller);
            _controller.m_BindRoot = m_Head;
            _controller.m_BindPosOffset = Vector3.zero;
            _controller.m_MoveDamping = 0f;
            _controller.m_RotateDamping = 0f;
            m_GravityLine = m_GravityPoint.GetComponent<LineRenderer>();
        }
        public override void OnRemoveControl()
        {
            base.OnRemoveControl();
        }
        private void Update()
        {
            m_GravityLine.enabled = m_TargetObject;
            if (!m_TargetObject)
                return;
            m_GravityLine.SetPosition(0, m_GravityPoint.position);
            m_GravityLine.SetPosition(1, m_TargetObject.position);
        }
        public override void Tick(float _deltaTime)
        {
            m_Head.position = m_Body.transform.position + Vector3.up * .9f;
            if (m_TargetObject)
            {
                m_TargetObject.MovePosition(Vector3.Lerp(m_TargetObject.transform.position, m_GravityPoint.position, _deltaTime * 10f));
                m_TargetObject.velocity = Vector3.zero;
            }
            m_Head.rotation = TickRotation();
            m_Body.Move(TickMovement());
        }
        public override void FixedTick(float _deltaTime)
        {
            if (m_TargetObject)
                return;

            m_GravityGunCounter.Tick(_deltaTime);
            if (m_GravityGunCounter.m_Playing||!m_AltFiring)
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
                m_GravityGunCounter.Replay();
                ReleaseTarget();
                return;
            }
            m_AltFiring=altFire;
        }

        void OnMainFire()
        {
            ReleaseTarget();
            m_GravityGunCounter.Replay();
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