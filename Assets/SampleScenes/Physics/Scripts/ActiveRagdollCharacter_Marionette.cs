using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsTest
{
    public class ActiveRagdollCharacter_Marionette : PhysicsCharacterBase
    {
        public bool m_LostConcious;
        public Transform m_HeadEnd;
        public Transform m_CameraAttach;
        public Rigidbody m_BodyString;
        public Rigidbody m_Hips;
        public Rigidbody m_LeftThigh, m_RightThigh;
        public Rigidbody m_LeftArm, m_RightArm;
        public FixedJoint m_LeftHand, m_RightHand;
        public float m_VerticalOffset = 1.8f;
        public float m_StringStrength = 200f;
        public float m_WalkAngularSpeed = 15f;
        bool m_Sprinting;
        bool m_LeftAim, m_RightAim;
        bool m_LeftGrabing, m_RightGrabing;
        float m_ConciousTimeElapsed;
        void OnLostConcious() => m_LostConcious = !m_LostConcious;
        void OnSprint(bool _sprint) => m_Sprinting = _sprint;
        void OnLeftAim(bool _leftAim) => m_LeftAim = _leftAim;
        void OnRightAim(bool _rightAim) => m_RightAim = _rightAim;
        public override void OnTakeControl()
        {
            base.OnTakeControl();
            TPSCameraController.Instance.m_BindRoot = m_CameraAttach;
            TPSCameraController.Instance.m_BindPosOffset = new Vector3(0, .5f, -4f);
            TPSCameraController.Instance.m_MoveDamping = .2f;
            TPSCameraController.Instance.m_RotateDamping = .2f;
            PCInputManager.Instance.GetKeyBinding(enum_Binding.Jump).Add(OnLostConcious);
            PCInputManager.Instance.GetKeyBinding(enum_Binding.Sprint).Add(OnSprint);
            PCInputManager.Instance.GetKeyBinding(enum_Binding.MainFire).Add(OnLeftAim);
            PCInputManager.Instance.GetKeyBinding(enum_Binding.AltFire).Add(OnRightAim);
            m_LeftThigh.maxAngularVelocity = 90f;
            m_RightThigh.maxAngularVelocity = 90f;
        }
        public override void OnRemoveControl()
        {
            base.OnRemoveControl();
            PCInputManager.Instance.GetKeyBinding(enum_Binding.Jump).Remove(OnLostConcious);
            PCInputManager.Instance.GetKeyBinding(enum_Binding.Sprint).Remove(OnSprint);
            PCInputManager.Instance.GetKeyBinding(enum_Binding.MainFire).Remove(OnLeftAim);
            PCInputManager.Instance.GetKeyBinding(enum_Binding.AltFire).Remove(OnRightAim);
        }

        public override void Tick(float _deltaTime)
        {
            TickMovement(out Quaternion targetRotation, out Vector3 targetMovement);
            m_CameraAttach.transform.position = m_HeadEnd.position;
            m_CameraAttach.transform.rotation = targetRotation;

            if (m_LostConcious)
                return;

            m_ConciousTimeElapsed += _deltaTime;
            if (m_MoveDelta.y == 0)
            {
                m_ConciousTimeElapsed = 0;
                return;
            }
            m_ConciousTimeElapsed += 1;
        }
        public override void FixedTick(float _deltaTime)
        {
            if (m_LostConcious)
                return;
            if (Physics.Raycast(m_Hips.position, Vector3.down, out RaycastHit _hit, 10f, PhysicsLayer.I_ItemMask))
            {
                float posOffset = m_HeadEnd.position.y - _hit.point.y;
                if (posOffset < m_VerticalOffset)
                    m_BodyString.AddForce(Vector3.up * m_StringStrength, ForceMode.Force);
                Debug.DrawRay(_hit.point, Vector3.up * m_VerticalOffset, Color.red);
            }
            m_Hips.MoveRotation(Quaternion.Slerp(m_Hips.rotation, Quaternion.Euler(m_Pitch, m_Yaw, 0), 20f * _deltaTime));
            float moveSpeed = m_WalkAngularSpeed * m_MoveDelta.y * (m_Sprinting ? 2f : 1f);

            float moveFrequency = 2f * (m_Sprinting ? 2f : 1f);
            m_LeftThigh.angularVelocity = m_Right * Mathf.Sin(m_ConciousTimeElapsed * moveFrequency * Mathf.PI) * moveSpeed;
            m_RightThigh.angularVelocity = m_Right * Mathf.Sin((m_ConciousTimeElapsed * moveFrequency + 1f) * Mathf.PI) * moveSpeed;

            Vector3 hipsForce = m_Forward * moveSpeed;
            float armStretch = .8f;
            Vector3 armPos = m_CameraAttach.position + m_CameraAttach.forward;
            m_LeftArm.useGravity = !m_LeftArm;
            if (m_LeftAim)
            {
                if(!m_LeftGrabing)
                {
                    Vector3 offset = armPos + m_CameraAttach.right * -.1f - m_LeftArm.position;
                    Vector3 strength = armStretch / offset.magnitude * offset * 30f;
                    m_LeftArm.AddForce(strength, ForceMode.Acceleration);
                    hipsForce -= strength;
                    if(GrabCheck(m_LeftHand.transform.position,out Vector3 _grabPoint,out Rigidbody _grabBody))
                    {
                        m_LeftHand.transform.position = _grabPoint;
                        m_LeftHand.connectedBody = _grabBody;
                        m_LeftHand.transform.SetActivate(false);
                    }
                }
            }
            else
            {
                m_LeftGrabing = false;
                m_LeftHand.transform.SetActivate(false);
            }

            m_Hips.AddForce(hipsForce, ForceMode.Force);
        }

        bool GrabCheck(Vector3 handPos, out Vector3 _grabPoint,out Rigidbody _grabBody)
        {
            Collider[] casts=Physics.OverlapSphere(handPos,.2f,PhysicsLayer.I_ItemMask);
            _grabPoint = Vector3.zero;
            _grabBody = null;
            foreach(var cast in casts)
            {
                _grabPoint= cast.ClosestPoint(handPos);
                _grabBody = cast.GetComponent<Rigidbody>();
                return true;
            }
            return false;
        }

#if UNITY_EDITOR
        public bool m_Gizmos = true;
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;

            Gizmos.color = Color.green;
            Gizmos.DrawLine(m_Hips.transform.position, m_Hips.transform.position + m_Up * 1f);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(m_Hips.transform.position, m_Hips.transform.position + m_Forward * 1f);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(m_Hips.transform.position, m_Hips.transform.position + m_Right * 1f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(m_BodyString.transform.position, .1f);
        }
#endif
    }
}