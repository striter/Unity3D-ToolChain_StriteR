using System.Collections;
using System.Collections.Generic;
using Runtime.TouchTracker;
using UnityEngine;

namespace Examples.PhysicsScenes
{
    [System.Serializable]
    public class ArmCombination
    {
        public Rigidbody m_ForeArm;
        public Rigidbody m_Arm;
        public Rigidbody m_Hand;
        public bool m_Aiming;
        public FixedJoint m_Grabing;
        public void OnAiming(bool _aiming,Vector2 _pos) => m_Aiming = _aiming;
        public void ArmTick(Vector3 armPos,ref Vector3 hipsForce)
        {
            m_ForeArm.useGravity = !m_Aiming;
            m_Arm.useGravity = !m_Aiming;
            m_Hand.useGravity = !m_Aiming;
            if(!m_Aiming)
            {
                if (m_Grabing)
                {
                    GameObject.Destroy(m_Grabing);
                    m_Grabing = null;
                }
                return;
            }

            if (m_Grabing)
                return;

            Vector3 offset = armPos - m_ForeArm.position;
            Vector3 strength = .8f / offset.magnitude * offset * 30f;
            m_ForeArm.AddForce(strength, ForceMode.Acceleration);
            hipsForce -= strength;
            if (!GrabCheck(out Vector3 _grabPoint, out Rigidbody _grabBody))
                return;
            m_Grabing = m_Hand.gameObject.AddComponent<FixedJoint>();
            m_Grabing.connectedBody = _grabBody;
            //m_Grabing.anchor = m_LeftHand.transform.worldToLocalMatrix.MultiplyPoint(_grabPoint);
            //m_Grabing.connectedAnchor = _grabBody?_grabBody.transform.worldToLocalMatrix.MultiplyPoint(_grabPoint):m_Grabing.anchor;
        }

        public bool GrabCheck(out Vector3 _grabPoint, out Rigidbody _grabBody)
        {
            var handPos = m_Hand.transform.position + m_Hand.transform.up * .15f;
            var casts = UnityEngine.Physics.OverlapSphere(handPos, .05f, PhysicsLayer.I_ItemMask);
            _grabPoint = Vector3.zero;
            _grabBody = null;
            foreach (var cast in casts)
            {
                _grabPoint = cast.ClosestPoint(handPos);
                _grabBody = cast.GetComponent<Rigidbody>();
                return true;
            }
            return false;
        }

        public void OnDrawGizmos()
        {
            if (!m_Aiming)
                return;

            Gizmos.color = m_Grabing ? Color.green : Color.red;
            Gizmos.DrawWireSphere(m_Hand.transform.position + m_Hand.transform.up * .15f,.05f);
        }
    }
    public class ActiveRagdollCharacter_Marionette : ActiveRagdollCharacter_ThirdPerson
    {
        public bool m_LostConcious;
        public Rigidbody m_BodyString;
        public Rigidbody m_Hips;
        public ArmCombination m_LeftArmCombine, m_RightArmCombine;
        public Rigidbody m_LeftThigh, m_RightThigh;
        public float m_VerticalOffset = 1.8f;
        public float m_StringStrength = 200f;
        public float m_WalkAngularSpeed = 15f;
        bool m_Sprinting;
        float m_ConciousTimeElapsed;

        
        public override void OnTakeControl()
        {
            base.OnTakeControl();
            m_LeftThigh.maxAngularVelocity = 90f;
            m_RightThigh.maxAngularVelocity = 90f;
            TouchConsole.InitButton(ETouchConsoleButton.Main).onPress = m_LeftArmCombine.OnAiming;
            TouchConsole.InitButton(ETouchConsoleButton.Alt).onPress = m_RightArmCombine.OnAiming;
            TouchConsole.InitButton(ETouchConsoleButton.Special1).onPress = (_press,_pos) => m_Sprinting = _press;
            TouchConsole.InitButton(ETouchConsoleButton.Special2).onClick = ()=>m_LostConcious = !m_LostConcious;
        }
        public override void OnRemoveControl()
        {
            base.OnRemoveControl();
        }

        protected override void Tick(float _deltaTime, ref List<TrackData> _data)
        {
            base.Tick(_deltaTime, ref _data);
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
            if (UnityEngine.Physics.Raycast(m_Hips.position, Vector3.down, out RaycastHit _hit, 10f, PhysicsLayer.I_ItemMask))
            {
                float posOffset = m_CameraFollow.position.y - _hit.point.y;
                if (posOffset < m_VerticalOffset)
                    m_BodyString.AddForce(Vector3.up * m_StringStrength*100f*Time.deltaTime, ForceMode.Force);
                Debug.DrawRay(_hit.point, Vector3.up * m_VerticalOffset, Color.red);
            }
            m_Hips.MoveRotation(Quaternion.Slerp(m_Hips.rotation, Quaternion.Euler(m_Pitch, m_Yaw, 0), 20f * _deltaTime));
            float moveSpeed = m_WalkAngularSpeed * m_MoveDelta.y * (m_Sprinting ? 2f : 1f);

            float moveFrequency = 2f * (m_Sprinting ? 2f : 1f);
            m_LeftThigh.angularVelocity = m_Right * Mathf.Sin(m_ConciousTimeElapsed * moveFrequency * Mathf.PI) * moveSpeed;
            m_RightThigh.angularVelocity = m_Right * Mathf.Sin((m_ConciousTimeElapsed * moveFrequency + 1f) * Mathf.PI) * moveSpeed;

            Vector3 hipsForce = m_Forward * moveSpeed;
            Vector3 armPos = m_CameraAttach.position + m_CameraAttach.forward;
            m_LeftArmCombine.ArmTick(armPos + m_CameraAttach.right * -.1f, ref hipsForce);
            m_RightArmCombine.ArmTick(armPos + m_CameraAttach.right * .1f, ref hipsForce);
            m_Hips.AddForce(hipsForce*100f*Time.deltaTime, ForceMode.Force);
        }

#if UNITY_EDITOR
        public bool m_Gizmos = true;
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;

            Gizmos.color = Color.green;
            Gizmos.DrawLine(m_Hips.transform.position, m_Hips.transform.position + (Vector3)m_Up * 1f);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(m_Hips.transform.position, m_Hips.transform.position + (Vector3)m_Forward * 1f);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(m_Hips.transform.position, m_Hips.transform.position + (Vector3)m_Right * 1f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(m_BodyString.transform.position, .1f);

            m_LeftArmCombine.OnDrawGizmos();
            m_RightArmCombine.OnDrawGizmos();
        }
#endif
    }
}