﻿using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace OPhysics
{
    #region Physics Simluator
    public abstract class PhysicsSimulatorBase<T> where T : MonoBehaviour
    {
        public Vector3 m_Origin { get; protected set; }
        public Vector3 m_Direction { get; protected set; }
        public float m_TimeElapsed { get; protected set; }
        public bool m_SelfSimulate { get; protected set; }
        public abstract void Tick(float deltaTime);
        public abstract Vector3 GetSimulatedPosition(float elapsedTime);
    }
    public class CapsuleCastPSimulatorBase<T> : PhysicsSimulatorBase<T> where T : MonoBehaviour
    {
        protected Vector3 m_PrePosition { get; private set; }
        public Vector3 m_Position { get; private set; }
        public Quaternion m_Rotation { get; private set; }
        float m_castHeight;
        float m_castRadius;
        int m_LayerMask;

        Func<RaycastHit, T, bool> OnTargetHitBreak;
        Predicate<T> CheckCanHitTarget;
        public CapsuleCastPSimulatorBase(float _height, float _radius, int _layerMask, Func<RaycastHit, T, bool> OnTargetHitBreak, Predicate<T> CheckCanHitTarget)
        {
            m_SelfSimulate = true;
            m_TimeElapsed = 0f;
            m_castHeight = _height;
            m_castRadius = _radius;
            m_LayerMask = _layerMask;
            this.OnTargetHitBreak = OnTargetHitBreak;
            this.CheckCanHitTarget = CheckCanHitTarget;
        }
        protected CapsuleCastPSimulatorBase<T> Play(Vector3 _position, Vector3 _direction) => SetPhysics(_position, _direction, 0f);
        public CapsuleCastPSimulatorBase<T> SetPhysics(Vector3 origin, Vector3 direction, float elapsedTime)
        {
            m_SelfSimulate = true;
            m_Origin = origin;
            m_PrePosition = m_Origin;
            m_Position = m_Origin;
            m_Direction = direction;
            m_TimeElapsed = elapsedTime;
            return this;
        }
        public virtual Vector4 GetRelativeParams() => Vector4.zero;
        public virtual void SetRelativeParam(Vector4 param) { }
        public override void Tick(float deltaTime)
        {
            if (!m_SelfSimulate || deltaTime == 0) return;
            m_PrePosition = GetSimulatedPosition(m_TimeElapsed);
            m_TimeElapsed += deltaTime;
            m_Position = GetSimulatedPosition(m_TimeElapsed);
            Vector3 direction = (m_Position - m_PrePosition).normalized;
            float distance = Vector3.Distance(m_Position, m_PrePosition);
            distance = distance > m_castHeight ? distance : m_castHeight;
            OnTargetCheck(deltaTime, Physics.SphereCastAll(new Ray(m_PrePosition, direction), m_castRadius, distance, m_LayerMask));
            m_PrePosition = m_Position;
            m_Rotation = Quaternion.LookRotation(direction);
        }
        public void Stop() => m_SelfSimulate = false;
        public override Vector3 GetSimulatedPosition(float elapsedTime)
        {
            Debug.Log("Override This Please");
            return Vector3.zero;
        }
        protected void OnTargetCheck(float deltaTime, RaycastHit[] castHits)
        {
            for (int i = 0; i < castHits.Length; i++)
            {
                T temp = castHits[i].collider.GetComponent<T>();
                if (CheckCanHitTarget(temp) && OnTargetHit(deltaTime, castHits[i], temp))
                    break;
            }
        }
        public virtual bool OnTargetHit(float deltaTime, RaycastHit hit, T template) => OnTargetHitBreak(hit, template);

    }

    public class AccelerationPSimulator<T> : CapsuleCastPSimulatorBase<T> where T : MonoBehaviour
    {
        protected Vector3 m_HorizontalDirection, m_VerticalDirection;
        protected float m_horizontalSpeed, m_horizontalAcceleration;
        public AccelerationPSimulator(float _height, float _radius, int _hitLayer, Func<RaycastHit, T, bool> _onTargetHit, Predicate<T> _CanHitTarget) : base(_height, _radius, _hitLayer, _onTargetHit, _CanHitTarget)
        {
        }

        public void Play(Vector3 position, Vector3 _horizontalDirection, Vector3 _verticalDirection, float _horizontalSpeed, float _horizontalAcceleration)
        {
            base.Play(position, _horizontalDirection);
            m_HorizontalDirection = _horizontalDirection;
            m_VerticalDirection = _verticalDirection;
            m_horizontalSpeed = _horizontalSpeed;
            m_horizontalAcceleration = _horizontalAcceleration;
        }
        public override Vector3 GetSimulatedPosition(float elapsedTime)
        {
            Vector3 horizontalShift = Vector3.zero;
            if (!(m_horizontalSpeed > 0 && m_horizontalAcceleration < 0))
            {
                float aboveZeroTime = m_horizontalSpeed / Mathf.Abs(m_horizontalAcceleration);

                horizontalShift += m_HorizontalDirection * PhysicsExpressions.AccelerationSpeedShift(m_horizontalSpeed, m_horizontalAcceleration, elapsedTime > aboveZeroTime ? aboveZeroTime : elapsedTime);
            }

            Vector3 targetPos = m_Origin + horizontalShift;
            return targetPos;
        }
    }

    public class SpeedDirectionPSimulator<T> : CapsuleCastPSimulatorBase<T> where T : MonoBehaviour
    {
        protected float m_Speed { get; private set; }
        public SpeedDirectionPSimulator(float _height, float _radius, int _hitLayer, Func<RaycastHit, T, bool> _onTargetHit, Predicate<T> _canHitTarget) : base(_height, _radius, _hitLayer, _onTargetHit, _canHitTarget)
        {
        }
        public void Play(Vector3 _startPos, Vector3 _direction, float _speed)
        {
            base.Play(_startPos, _direction);
            m_Speed = _speed;
        }
        public override Vector4 GetRelativeParams() => new Vector4(m_Speed, 0);
        public override void SetRelativeParam(Vector4 param)
        {
            m_Speed = param.x;
        }
        public override Vector3 GetSimulatedPosition(float elapsedTime) => m_Origin + m_Direction * PhysicsExpressions.SpeedShift(m_Speed, elapsedTime);
    }

    public class LerpPSimulator<T> : CapsuleCastPSimulatorBase<T> where T : MonoBehaviour
    {
        bool b_lerpFinished;
        Action OnLerpFinished;
        Vector3 m_endPos;
        float f_totalTime;
        public LerpPSimulator(Action _OnLerpFinished, float _height, float _radius, int _hitLayer, Func<RaycastHit, T, bool> _onTargetHit, Predicate<T> _canHitTarget) : base(_height, _radius, _hitLayer, _onTargetHit, _canHitTarget)
        {
            OnLerpFinished = _OnLerpFinished;
        }
        public void Play(Vector3 _startPos, Vector3 _endPos, float _duration)
        {
            base.Play(_startPos, _endPos - _startPos);
            b_lerpFinished = false;
            m_endPos = _endPos;
            f_totalTime = _duration;
        }
        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);
            if (!b_lerpFinished && m_TimeElapsed > f_totalTime)
            {
                OnLerpFinished?.Invoke();
                b_lerpFinished = true;
            }
        }
        public override Vector3 GetSimulatedPosition(float elapsedTime) => b_lerpFinished ? m_endPos : Vector3.Lerp(m_Origin, m_endPos, elapsedTime / f_totalTime);
    }


    public class ParacurveBouncePSimulator<T> : CapsuleCastPSimulatorBase<T> where T : MonoBehaviour
    {
        public float m_SpeedMultiplier { get; private set; }
        public float m_HorizontalSpeed { get; private set; }
        public float m_VerticalAcceleration { get; private set; }

        bool m_bounceOnHit;
        float m_bounceSpeedMultiply;
        public ParacurveBouncePSimulator(float _height, float _radius, bool bounce, float _bounceSpeedMultiply, int _hitLayer, Func<RaycastHit, T, bool> _onBounceHit, Predicate<T> _OnBounceCheck) : base(_height, _radius, _hitLayer, _onBounceHit, _OnBounceCheck)
        {
            m_bounceOnHit = bounce;
            m_bounceSpeedMultiply = _bounceSpeedMultiply;
        }

        public void Play(Vector3 _startPos, Vector3 _endPos, float _angle, float _horiSpeed)
        {
            base.Play(_startPos, Vector3.zero);
            var horiDirection = umath.GetXZLookDirection(_startPos, _endPos);
            var horiRight = horiDirection.rotateCW(kfloat3.up, 90);
            m_Direction = horiDirection.rotateCW(horiRight, -_angle);
            m_HorizontalSpeed = _horiSpeed / Mathf.Cos(_angle * Mathf.Deg2Rad);
            m_SpeedMultiplier = 1f;
            float horiDistance = Vector3.Distance(_startPos, _endPos);
            float duration = horiDistance / _horiSpeed;
            float vertiDistance = Mathf.Tan(_angle * Mathf.Deg2Rad) * horiDistance;
            m_VerticalAcceleration = PhysicsExpressions.GetAcceleration(0, vertiDistance, duration);
        }

        public override Vector4 GetRelativeParams() => new Vector4(m_HorizontalSpeed, m_VerticalAcceleration, m_SpeedMultiplier);
        public override void SetRelativeParam(Vector4 param)
        {
            m_HorizontalSpeed = param.x;
            m_VerticalAcceleration = param.y;
            m_SpeedMultiplier = param.z;
        }

        public override bool OnTargetHit(float deltaTime, RaycastHit hit, T template)
        {
            if (!m_bounceOnHit)
                return base.OnTargetHit(deltaTime, hit, template);

            m_SpeedMultiplier -= .05f;
            m_SpeedMultiplier *= m_bounceSpeedMultiply;
            if (m_SpeedMultiplier < .05f)
                return true;
            Vector3 normal = hit.point == Vector3.zero ? Vector3.up : hit.normal;
            SetPhysics(m_PrePosition, Vector3.Reflect(m_Direction, normal), 0);
            return true;
        }
        public override Vector3 GetSimulatedPosition(float elapsedTime) => m_Origin + m_Direction * m_HorizontalSpeed * m_SpeedMultiplier * elapsedTime + Vector3.down * m_VerticalAcceleration * elapsedTime * elapsedTime;
    }

    public class ReflectBouncePSimulator<T> : SpeedDirectionPSimulator<T> where T : MonoBehaviour
    {
        Func<Vector3, Vector3, Vector3> OnBounceDirection;
        int m_BounceTimesLeft = 0;
        public ReflectBouncePSimulator(float _height, float _radius, int bounceTime, Func<Vector3, Vector3, Vector3> _OnBounceDirection, int _hitLayer, Func<RaycastHit, T, bool> _onTargetHit, Predicate<T> _canHitTarget) : base(_height, _radius, _hitLayer, _onTargetHit, _canHitTarget)
        {
            m_BounceTimesLeft = bounceTime;
            OnBounceDirection = _OnBounceDirection;
        }

        public override bool OnTargetHit(float deltaTime, RaycastHit hit, T template)
        {
            bool hitBreak = base.OnTargetHit(deltaTime, hit, template);
            if (hitBreak)
            {
                m_BounceTimesLeft--;
                SetPhysics(m_PrePosition, OnBounceDirection == null ? Vector3.Normalize(Vector3.Reflect(m_Direction, hit.normal)) : OnBounceDirection(m_Direction, hit.normal), m_TimeElapsed);
                if (m_BounceTimesLeft <= 0)
                    Stop();
            }
            return hitBreak;
        }

    }

    public static class PhysicsExpressions
    {
        public static float AccelerationSpeedShift(float speed, float acceleration, float elapsedTime)        //All M/S  s=vt+a*t^2/2?
        {
            return SpeedShift(speed, elapsedTime) + acceleration * Mathf.Pow(elapsedTime, 2) / 2;
        }
        public static float GetAcceleration(float startSpeed, float distance, float duration)
        {
            return (distance - SpeedShift(startSpeed, duration)) / Mathf.Pow(duration, 2);
        }
        public static float SpeedShift(float speed, float elapsedTime)      //M/s s=vt
        {
            return speed * elapsedTime;
        }
    }
    #endregion
    public class ConfigurableJoint_Helper
    {
        public ConfigurableJoint m_ConfigurableJoint { get; private set; }
        Vector3 m_InvStartPosition;
        Quaternion m_InvStartLocalRotation;
        Quaternion m_LocalToJointSpace;
        Quaternion m_InvLocalToJointSpace;
        public ConfigurableJoint_Helper(ConfigurableJoint _joint)
        {
            m_ConfigurableJoint = _joint;
            OnValidate();
        }

        public void OnValidate()
        {
            Vector3 right = m_ConfigurableJoint.axis.normalized;
            Vector3 up = m_ConfigurableJoint.secondaryAxis.normalized;
            Vector3 forward = Vector3.Cross(right, up).normalized;

            m_InvLocalToJointSpace = Quaternion.LookRotation(forward, up);
            m_LocalToJointSpace = Quaternion.Inverse(m_InvLocalToJointSpace);
            m_InvStartLocalRotation = Quaternion.Inverse(m_ConfigurableJoint.transform.localRotation);
            m_InvStartPosition = -m_ConfigurableJoint.connectedBody.transform.localPosition - m_ConfigurableJoint.connectedAnchor;
        }

        public void SetTargetPosition(Vector3 _localPosition)
        {
            m_ConfigurableJoint.targetPosition = m_LocalToJointSpace * (_localPosition + m_InvStartPosition);
        }
        public void SetTargetRotation(Quaternion _localRotation)
        {
            m_ConfigurableJoint.targetRotation = m_InvLocalToJointSpace * _localRotation * m_InvStartLocalRotation * m_LocalToJointSpace;
        }
    }
}