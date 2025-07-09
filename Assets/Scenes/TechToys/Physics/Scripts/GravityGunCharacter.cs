﻿using System.Collections;
using System.Collections.Generic;
using Runtime.TouchTracker;
using UnityEngine;

namespace Examples.PhysicsScenes
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
        LineRenderer gravityALine;
        Counter m_GravityGunCounter = new Counter(.35f);
        public override void OnTakeControl()
        {
            base.OnTakeControl();
            gravityALine = m_GravityPoint.GetComponent<LineRenderer>();
            TouchConsole.InitButton(ETouchConsoleButton.Main).onClick = OnMainFire;
            TouchConsole.InitButton(ETouchConsoleButton.Alt).onPress = OnAltFire;
        }
        public override void OnRemoveControl()
        {
            base.OnRemoveControl();
        }
        private void Update()
        {
            gravityALine.enabled = m_TargetObject;
            if (!m_TargetObject)
                return;
            gravityALine.SetPosition(0, m_GravityPoint.position);
            gravityALine.SetPosition(1, m_TargetObject.position);
        }

        protected override void Tick(float _deltaTime, ref List<TrackData> _data)
        {
            if (m_TargetObject)
            {
                m_TargetObject.MovePosition(Vector3.Lerp(m_TargetObject.transform.position, m_GravityPoint.position, _deltaTime * 10f));
                m_TargetObject.velocity = Vector3.zero;
            }
            m_Head.rotation = TickRotation();
            m_Body.Move(TickMovement()*_deltaTime);
            m_Head.position = m_Body.transform.position + Vector3.up * .9f;
        }

        public override void FixedTick(float _deltaTime)
        {
            if (m_TargetObject)
                return;

            m_GravityGunCounter.Tick(_deltaTime);
            if (m_GravityGunCounter.Playing||!m_AltFiring)
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

        void OnAltFire(bool _altFire,Vector2 _pos)
        {
            if (_altFire && m_TargetObject)
            {
                m_GravityGunCounter.Replay();
                ReleaseTarget();
                return;
            }
            m_AltFiring=_altFire;
        }

        void OnMainFire()
        {
            ReleaseTarget();
            m_GravityGunCounter.Replay();
            Rigidbody burstTarget = m_TargetObject;
            Vector3 hitPosition = m_TargetObject?m_TargetObject.transform.position:Vector3.zero;
            if(burstTarget==null)
            {
                if (!UnityEngine.Physics.Raycast(m_Head.position, m_Head.forward, out RaycastHit _hit, m_GravityGunAvailableDistance, -1) || !TargetInteractable(_hit.collider, out burstTarget))
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