using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsTest
{
    public class PhysicsManager : MonoBehaviour
    {
        public GravityGunCharacter m_GravityGunCharacter;
        public ActiveRagdollCharacter_Marionette m_marionetteCharacter;
        public ActiveRagdollCharacter m_HFFCharacter;
        PhysicsCharacterBase m_CharacterBase;
        private void Start()
        {
            UIT_TouchConsole.Instance.InitConsole(consoleOn=>Cursor.lockState=consoleOn? CursorLockMode.Confined: CursorLockMode.Locked);
            UIT_TouchConsole.Instance.AddConsoleBinding().Set("Graviry", KeyCode.F1).Button(() => SetCharacter(m_GravityGunCharacter));
            UIT_TouchConsole.Instance.AddConsoleBinding().Set("Marioentte", KeyCode.F2).Button(() => SetCharacter(m_marionetteCharacter));
            UIT_TouchConsole.Instance.AddConsoleBinding().Set("Human Fall Flat", KeyCode.F3).Button(() => SetCharacter(m_HFFCharacter));
            UIT_TouchConsole.Instance.AddConsoleBinding().Set("None").Button(() => SetCharacter(null));
            SetCharacter(m_GravityGunCharacter);
            Cursor.lockState = CursorLockMode.Locked;
        }
        void SetCharacter(PhysicsCharacterBase _character)
        {
            if (m_CharacterBase)
                m_CharacterBase.OnRemoveControl();

            m_CharacterBase = _character;

            if (m_CharacterBase)
                m_CharacterBase.OnTakeControl();
        }

        private void Update()
        {
            if (!m_CharacterBase)
                return;
            m_CharacterBase.Tick(Time.deltaTime);
        }
        private void LateUpdate()
        {
            if (!m_CharacterBase)
                return;
            m_CharacterBase.FixedTick(Time.fixedDeltaTime);
        }
    }

    public abstract class PhysicsCharacterBase : MonoBehaviour
    {
        public float m_MoveSpeed = 10f;
        public float m_RotateSpeed = 1f;
        public virtual void OnTakeControl()
        {
            PCInputManager.Instance.OnMovementDelta = OnMove;
            PCInputManager.Instance.OnRotateDelta = OnRotate;
        }
        public virtual void OnRemoveControl()
        {
            PCInputManager.Instance.OnMovementDelta = null;
            PCInputManager.Instance.OnRotateDelta = null;
        }
        public abstract void Tick(float _deltaTime);
        public abstract void FixedTick(float _deltaTime);
        protected Vector2 m_MoveDelta { get; private set; }
        protected float m_Pitch, m_Yaw;
        protected Vector3 m_Forward { get; private set; }
        protected Vector3 m_Up { get; private set; }
        protected Vector3 m_Right { get; private set; }
        void OnMove(Vector2 _delta) => m_MoveDelta = _delta;
        void OnRotate(Vector2 _delta)
        {
            _delta *= m_RotateSpeed;
            m_Yaw += _delta.x;
            m_Pitch -= _delta.y;
            m_Pitch = Mathf.Clamp(m_Pitch, -75f, 75f);
        }

        protected void TickMovement(out Quaternion targetRotation,out Vector3 targetMovement)
        {
            m_Forward = Vector3.forward.RotateDirectionClockwise(Vector3.up, m_Yaw);
            m_Right = Vector3.right.RotateDirectionClockwise(Vector3.up, m_Yaw);
            m_Up = Vector3.up.RotateDirectionClockwise(Vector3.forward, m_Pitch);
            targetRotation= Quaternion.Euler(m_Pitch, m_Yaw, 0);
            targetMovement = (m_Forward * m_MoveDelta.y + m_Right * m_MoveDelta.x).normalized*m_MoveSpeed;
        }
    }

    public static class PhysicsLayer
    {
        public static readonly int I_ItemMask = 1 << LayerMask.NameToLayer("Default");
    }

}