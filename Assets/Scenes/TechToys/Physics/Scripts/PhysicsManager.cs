﻿using System.Collections;
using System.Collections.Generic;
using System.Linq.Extensions;
using CameraController;
using CameraController.Demo;
using UnityEngine;
using Runtime.TouchTracker;
using Unity.Mathematics;

namespace Examples.PhysicsScenes
{
    public class PhysicsManager : MonoBehaviour
    {
        public GravityGunCharacter m_GravityGunCharacter;
        public ActiveRagdollCharacter_Marionette m_marionetteCharacter;
        public ActiveRagdollCharacter_Human_Balance m_Human_Balance;
        public ActiveRagdollCharacter_Human_StaticAnimator m_Human_StaticAnimator;
        PhysicsCharacterBase m_CharacterBase;
        DynamicItemRepositon[] m_DynamicItems;
        public FCameraControllerCore m_Controller = new();
        public FCameraControllerSimple.Input m_Input = new();

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
            m_Input.camera = GetComponentInChildren<Camera>();
            Rigidbody[] rigidbodies = transform.Find("Dynamic").GetComponentsInChildren<Rigidbody>();
            m_DynamicItems = new DynamicItemRepositon[rigidbodies.Length];
            for (int i = 0; i < rigidbodies.Length; i++)
                m_DynamicItems[i] = new DynamicItemRepositon(rigidbodies[i]);
        }
        private void Start()
        {
            TouchConsole.InitDefaultCommands();
            TouchConsole.NewPage("Level");
            TouchConsole.Command("Gravity", KeyCode.F1).Button(() => SetCharacter(m_GravityGunCharacter));
            TouchConsole.Command("Marionette", KeyCode.F2).Button(() => SetCharacter(m_marionetteCharacter));
            TouchConsole.Command("Human Static Animator", KeyCode.F3).Button(() => SetCharacter(m_Human_StaticAnimator));
            TouchConsole.Command("Human Balance", KeyCode.F4).Button(() => SetCharacter(m_Human_Balance));
            TouchConsole.Command("Reset All Items", KeyCode.F5).Button(() => m_DynamicItems.Traversal(dynamicItem => dynamicItem.Reposition()));

            SetCharacter(m_marionetteCharacter);
        }
        void SetCharacter(PhysicsCharacterBase _character)
        {
            if (m_CharacterBase)
                m_CharacterBase.OnRemoveControl();

            m_CharacterBase = _character;

            if (m_CharacterBase)
            {
                _character.OnTakeControl();
                m_Input.anchor = _character.transform.Find("CameraAttach");
                m_Input.controller = _character.m_CameraController;
            }
        }

        private void Update()
        {
            if (!m_CharacterBase)
                return;
            m_CharacterBase.Tick(Time.deltaTime);
            m_Input.euler = new float3(m_CharacterBase.m_Pitch, m_CharacterBase.m_Yaw, 0);
        }
        
        private void LateUpdate()
        {
            m_Controller.Tick(Time.unscaledDeltaTime,ref m_Input);
            m_DynamicItems.Traversal(dynamicItem => dynamicItem.FixedUpdate());
            if (!m_CharacterBase)
                return;
            m_CharacterBase.FixedTick(Time.fixedDeltaTime);
        }
    }

    public abstract class PhysicsCharacterBase : MonoBehaviour
    {
        public ACameraController m_CameraController;
        public float m_MoveSpeed = 10f;
        public float m_RotateSpeed = 1f;

        public virtual void OnTakeControl()
        {
            
        }
        public virtual void OnRemoveControl()
        {
            TouchConsole.ClearButtons();
        }
        public abstract void FixedTick(float _deltaTime);
        protected Vector2 m_MoveDelta { get; private set; }
        public float m_Pitch, m_Yaw;
        protected float3 m_Forward { get; private set; }
        protected float3 m_Up { get; private set; }
        protected float3 m_Right { get; private set; }
        
        public void Tick(float _deltaTime)
        {
            var trackers=TouchTracker.Execute(Time.unscaledTime,true);
            Tick(_deltaTime,ref trackers);
            trackers.Joystick_Stationary(
                (position,active)=>{ TouchConsole.DoSetJoystick(position,active);if(!active) m_MoveDelta=Vector2.zero; },
                (normalized)=>{m_MoveDelta = normalized;TouchConsole.DoTrackJoystick(normalized);},
                TouchConsole.kJoystickRange,
                TouchConsole.kJoystickRadius,
                true);

            var delta =  trackers.Input_ScreenMove(TouchConsole.kScreenDeltaRange);
            delta *= m_RotateSpeed;
            m_Yaw += delta.x;
            m_Pitch -= delta.y;
            m_Pitch = Mathf.Clamp(m_Pitch, -75f, 75f);
        }

        protected abstract void Tick(float _deltaTime, ref List<TrackData> _data);
        protected Quaternion TickRotation()
        {
            m_Forward = kfloat3.forward.rotateCW(Vector3.up, m_Yaw * kmath.kDeg2Rad);
            m_Right = kfloat3.right.rotateCW(Vector3.up, m_Yaw * kmath.kDeg2Rad);
            m_Up = kfloat3.up.rotateCW(Vector3.forward, m_Pitch * kmath.kDeg2Rad);
            return Quaternion.Euler(m_Pitch, m_Yaw, 0);
        }
        protected Vector3 TickMovement()=> kfloat3.down*9.8f+(m_Forward * m_MoveDelta.y + m_Right * m_MoveDelta.x).normalize() * m_MoveSpeed;
    }

    public static class PhysicsLayer
    {
        public static readonly int I_ItemMask = 1 << LayerMask.NameToLayer("Default");
    }

}