using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsTest
{
    public class PhysicsManager : MonoBehaviour
    {
        public GravityGunCharacter m_GravityGunCharacter;
        public ActiveRagdollCharacter_Marionette m_marionetteCharacter;
        public ActiveRagdollCharacter_Human_Balance m_Human_Balance;
        public ActiveRagdollCharacter_Human_StaticAnimator m_Human_StaticAnimator;
        PhysicsCharacterBase m_CharacterBase;
        DynamicItemRepositon[] m_DynamicItems;
        class DynamicItemRepositon
        {
            public Rigidbody m_Rigidbody;
            public Vector3 m_startPos;
            public Quaternion m_startRot;
            public DynamicItemRepositon(Rigidbody _rigidbody)
            {
                m_Rigidbody = _rigidbody;
                m_startPos = m_Rigidbody.transform.position;
                m_startRot = m_Rigidbody.transform.rotation;
            }
            public void FixedUpdate()
            {
                if (m_Rigidbody.transform.position.y > -10f)
                    return;
                Reposition();
            }
            public void Reposition()
            {
                m_Rigidbody.transform.position = m_startPos;
                m_Rigidbody.transform.rotation = m_startRot;
                m_Rigidbody.velocity = Vector3.zero;
                m_Rigidbody.angularVelocity = Vector3.zero;
            }
        }
        private void Awake()
        {
            Rigidbody[] rigidbodies = transform.Find("Dynamic").GetComponentsInChildren<Rigidbody>();
            m_DynamicItems = new DynamicItemRepositon[rigidbodies.Length];
            for (int i = 0; i < rigidbodies.Length; i++)
                m_DynamicItems[i] = new DynamicItemRepositon(rigidbodies[i]);
        }
        private void Start()
        {
            UIT_TouchConsole.Init(consoleOn=>Cursor.lockState=consoleOn? CursorLockMode.Confined: CursorLockMode.Locked);
            UIT_TouchConsole.Header("Level");
            UIT_TouchConsole.Command("Graviry", KeyCode.F1).Button(() => SetCharacter(m_GravityGunCharacter));
            UIT_TouchConsole.Command("Marioentte", KeyCode.F2).Button(() => SetCharacter(m_marionetteCharacter));
            UIT_TouchConsole.Command("Human Static Animator", KeyCode.F3).Button(() => SetCharacter(m_Human_StaticAnimator));
            UIT_TouchConsole.Command("Human Balance", KeyCode.F4).Button(() => SetCharacter(m_Human_Balance));
            UIT_TouchConsole.Command("Reset All Items", KeyCode.F5).Button(() => m_DynamicItems.Traversal(dynamicItem => dynamicItem.Reposition()));

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
            m_DynamicItems.Traversal(dynamicItem => dynamicItem.FixedUpdate());
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

        protected Quaternion TickRotation()
        {
            m_Forward = Vector3.forward.RotateDirectionClockwise(Vector3.up, m_Yaw);
            m_Right = Vector3.right.RotateDirectionClockwise(Vector3.up, m_Yaw);
            m_Up = Vector3.up.RotateDirectionClockwise(Vector3.forward, m_Pitch);
            return Quaternion.Euler(m_Pitch, m_Yaw, 0);
        }
        protected Vector3 TickMovement()=> (m_Forward * m_MoveDelta.y + m_Right * m_MoveDelta.x).normalized * m_MoveSpeed;
    }

    public static class PhysicsLayer
    {
        public static readonly int I_ItemMask = 1 << LayerMask.NameToLayer("Default");
    }

}