using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TPhysics
{
    #region Physics Simluator
    public abstract class PhysicsSimulator<T> where T : MonoBehaviour
    {
        protected Vector3 m_startPos;
        protected Vector3 m_Direction;
        public float m_simulateTime { get; protected set; }
        public bool m_simulating { get; protected set; }
        public abstract void Simulate(float deltaTime);
        public abstract Vector3 GetSimulatedPosition(float elapsedTime);
    }
    public class CapsuleCastPSimulator<T> : PhysicsSimulator<T> where T : MonoBehaviour
    {
        Transform transform;
        Vector3 m_prePos, m_curPos;
        protected float m_castHeight, m_castRadius;
        protected int m_hitLayer;
        protected Func<RaycastHit, T, bool> OnTargetHitBreak;
        protected Predicate<T> CanHitTarget;
        public CapsuleCastPSimulator(Transform _transform, Vector3 _startPos, Vector3 _direction, float _height, float _radius, int _hitLayer, Func<RaycastHit, T, bool> _onTargetHit, Predicate<T> _CanHitTarget)
        {
            m_simulateTime = 0f;
            transform = _transform;
            m_startPos = _startPos;
            m_prePos = m_startPos;
            m_curPos = m_startPos;
            transform.position = _startPos;
            m_Direction = _direction;
            m_castHeight = _height;
            m_castRadius = _radius;
            m_hitLayer = _hitLayer;
            OnTargetHitBreak = _onTargetHit;
            CanHitTarget = _CanHitTarget;
            m_simulating = true;
        }
        protected virtual Quaternion GetSimulateRotation(float deltaTime, Vector3 direction) => Quaternion.LookRotation(direction);
        public override void Simulate(float deltaTime)
        {
            if (!m_simulating || deltaTime == 0) return;
            m_prePos = GetSimulatedPosition(m_simulateTime);
            m_simulateTime += deltaTime;
            m_curPos = GetSimulatedPosition(m_simulateTime);
            Vector3 direction = (m_curPos - m_prePos).normalized;
            float distance = Vector3.Distance(m_curPos, m_prePos);
            distance = distance > m_castHeight ? distance : m_castHeight;
            OnTargetsHitBreak(deltaTime, Physics.SphereCastAll(new Ray(m_prePos, direction), m_castRadius, distance, m_hitLayer));
            m_prePos = m_curPos;
            transform.position = m_curPos;
            transform.rotation = GetSimulateRotation(deltaTime, direction);
        }
        public void Redirection(Vector3 direction)
        {
            m_startPos = m_prePos;
            m_Direction = direction;
            m_simulateTime = 0f;
        }
        public override Vector3 GetSimulatedPosition(float elapsedTime)
        {
            Debug.Log("Override This Please");
            return Vector3.zero;
        }
        protected void OnTargetsHitBreak(float deltaTime, RaycastHit[] castHits)
        {
            for (int i = 0; i < castHits.Length; i++)
            {
                T temp = castHits[i].collider.GetComponent<T>();
                if (CanHitTarget(temp) && OnTargetHit(deltaTime, castHits[i], temp))
                    break;
            }
        }
        public virtual bool OnTargetHit(float deltaTime, RaycastHit hit, T template) => OnTargetHitBreak(hit, template);

    }

    public class AccelerationPSimulator<T> : CapsuleCastPSimulator<T> where T : MonoBehaviour
    {
        protected Vector3 m_HorizontalDirection, m_VerticalDirection;
        protected float m_horizontalSpeed, m_horizontalAcceleration;
        public AccelerationPSimulator(Transform _transform, Vector3 _startPos, Vector3 _horizontalDirection, Vector3 _verticalDirection, float _horizontalSpeed, float _horizontalAcceleration, float _height, float _radius, int _hitLayer, Func<RaycastHit, T, bool> _onTargetHit, Predicate<T> _CanHitTarget) : base(_transform, _startPos, _horizontalDirection, _height, _radius, _hitLayer, _onTargetHit, _CanHitTarget)
        {
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

            Vector3 targetPos = m_startPos + horizontalShift;
            return targetPos;
        }
    }

    public class SpeedDirectionPSimulator<T> : CapsuleCastPSimulator<T> where T : MonoBehaviour
    {
        protected Vector3 m_VerticalDirection { get; private set; }
        protected float m_horizontalSpeed { get; private set; }
        public SpeedDirectionPSimulator(Transform _transform, Vector3 _startPos, Vector3 _horizontalDirection, Vector3 _verticalDirection, float _horizontalSpeed, float _height, float _radius, int _hitLayer, Func<RaycastHit, T, bool> _onTargetHit, Predicate<T> _canHitTarget) : base(_transform, _startPos, _horizontalDirection, _height, _radius, _hitLayer, _onTargetHit, _canHitTarget)
        {
            m_VerticalDirection = _verticalDirection.normalized;
            m_horizontalSpeed = _horizontalSpeed;
        }
        public override Vector3 GetSimulatedPosition(float elapsedTime) => m_startPos + m_Direction * PhysicsExpressions.SpeedShift(m_horizontalSpeed, elapsedTime);
    }

    public class LerpPSimulator<T> : CapsuleCastPSimulator<T> where T : MonoBehaviour
    {
        bool b_lerpFinished;
        Action OnLerpFinished;
        Vector3 m_endPos;
        float f_totalTime;
        public LerpPSimulator(Transform _transform, Vector3 _startPos, Vector3 _endPos, Action _OnLerpFinished, float _duration, float _height, float _radius, int _hitLayer, Func<RaycastHit, T, bool> _onTargetHit, Predicate<T> _canHitTarget) : base(_transform, _startPos, _endPos - _startPos, _height, _radius, _hitLayer, _onTargetHit, _canHitTarget)
        {
            m_endPos = _endPos;
            OnLerpFinished = _OnLerpFinished;
            f_totalTime = _duration;
            b_lerpFinished = false;
        }
        public override void Simulate(float deltaTime)
        {
            base.Simulate(deltaTime);
            if (!b_lerpFinished && m_simulateTime > f_totalTime)
            {
                OnLerpFinished?.Invoke();
                b_lerpFinished = true;
            }
        }
        public override Vector3 GetSimulatedPosition(float elapsedTime) => b_lerpFinished ? m_endPos : Vector3.Lerp(m_startPos, m_endPos, elapsedTime / f_totalTime);
    }


    public class ParacurveBouncePSimulator<T> : CapsuleCastPSimulator<T> where T : MonoBehaviour
    {
        float f_speed;
        float f_vertiAcceleration;
        bool b_randomRotation;
        bool b_bounceOnHit;
        float f_bounceSpeedMultiply;
        protected Vector3 v3_RotateEuler;
        protected Vector3 v3_RotateDirection;
        public ParacurveBouncePSimulator(Transform _transform, Vector3 _startPos, Vector3 _endPos, float _angle, float _horiSpeed, float _height, float _radius, bool randomRotation, int _hitLayer, bool bounce, float _bounceSpeedMultiply, Func<RaycastHit, T, bool> _onBounceHit, Predicate<T> _OnBounceCheck) : base(_transform, _startPos, Vector3.zero, _height, _radius, _hitLayer, _onBounceHit, _OnBounceCheck)
        {
            Vector3 horiDirection = TCommon.GetXZLookDirection(_startPos, _endPos);
            Vector3 horiRight = horiDirection.RotateDirectionClockwise(Vector3.up, 90);
            m_Direction = horiDirection.RotateDirectionClockwise(horiRight, -_angle);
            f_speed = _horiSpeed / Mathf.Cos(_angle * Mathf.Deg2Rad);
            float horiDistance = Vector3.Distance(_startPos, _endPos);
            float duration = horiDistance / _horiSpeed;
            float vertiDistance = Mathf.Tan(_angle * Mathf.Deg2Rad) * horiDistance;
            f_vertiAcceleration = PhysicsExpressions.GetAcceleration(0, vertiDistance, duration);
            b_randomRotation = randomRotation;
            b_bounceOnHit = bounce;
            f_bounceSpeedMultiply = _bounceSpeedMultiply;
            v3_RotateEuler = Quaternion.LookRotation(m_Direction).eulerAngles;
            v3_RotateDirection = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
        }


        protected override Quaternion GetSimulateRotation(float deltaTime, Vector3 direction)
        {
            if (!b_randomRotation)
                return base.GetSimulateRotation(deltaTime, direction);
            v3_RotateEuler += v3_RotateDirection * deltaTime * 300f;
            return Quaternion.Euler(v3_RotateEuler);
        }
        public override bool OnTargetHit(float deltaTime, RaycastHit hit, T template)
        {
            if (!b_bounceOnHit)
                return base.OnTargetHit(deltaTime, hit, template);

            f_speed -= .1f;
            f_speed *= f_bounceSpeedMultiply;
            if (f_speed < m_castRadius)
            {
                m_simulating = false;
                return true;
            }
            Vector3 normal = hit.point == Vector3.zero ? Vector3.up : hit.normal;
            Redirection(Vector3.Reflect(m_Direction, normal));
            return true;
        }

        public override Vector3 GetSimulatedPosition(float elapsedTime) => m_startPos + m_Direction * f_speed * elapsedTime + Vector3.down * f_vertiAcceleration * elapsedTime * elapsedTime;
    }

    public class ReflectBouncePSimulator<T> : SpeedDirectionPSimulator<T> where T : MonoBehaviour
    {
        Func<Vector3, Vector3, Vector3> OnBounceDirection;
        int m_BounceTimesLeft = 0;
        public ReflectBouncePSimulator(Transform _transform, Vector3 _startPos, Vector3 _horizontalDirection, Vector3 _verticalDirection, float _horizontalSpeed, float _height, float _radius, int bounceTime, Func<Vector3, Vector3, Vector3> _OnBounceDirection, int _hitLayer, Func<RaycastHit, T, bool> _onTargetHit, Predicate<T> _canHitTarget) : base(_transform, _startPos, _horizontalDirection, _verticalDirection, _horizontalSpeed, _height, _radius, _hitLayer, _onTargetHit, _canHitTarget)
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
                Redirection(OnBounceDirection == null ? Vector3.Normalize(Vector3.Reflect(m_Direction, hit.normal)) : OnBounceDirection(m_Direction, hit.normal));
                m_simulating = m_BounceTimesLeft > 0;
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

    #region Physics Cast
    public static class Physics_Extend
    {
        public static RaycastHit[] BoxCastAll(Vector3 position, Vector3 forward, Vector3 up, Vector3 boxBounds, int layerMask = -1)
        {
            float castBoxLength = .1f;
            return Physics.BoxCastAll(position + forward * castBoxLength / 2f, new Vector3(boxBounds.x / 2, boxBounds.y / 2, castBoxLength / 2), forward, Quaternion.LookRotation(forward, up), boxBounds.z - castBoxLength, layerMask);
        }

        public static RaycastHit[] TrapeziumCastAll(Vector3 position, Vector3 forward, Vector3 up, Vector4 trapeziumInfo, int layerMask = -1, int castCount = 8)
        {
            List<RaycastHit> hitsList = new List<RaycastHit>();
            float castLength = trapeziumInfo.z / castCount;
            for (int i = 0; i < castCount; i++)
            {
                Vector3 boxPos = position + forward * castLength * i;
                Vector3 boxInfo = new Vector3(trapeziumInfo.x + (trapeziumInfo.w - trapeziumInfo.x) * i / castCount, trapeziumInfo.y, castLength);
                RaycastHit[] hits = BoxCastAll(boxPos, forward, up, boxInfo, layerMask);
                for (int j = 0; j < hits.Length; j++)
                {
                    bool b_continue = false;

                    for (int k = 0; k < hitsList.Count; k++)
                        if (hitsList[k].collider == hits[j].collider)
                            b_continue = true;

                    if (b_continue)
                        continue;

                    hitsList.Add(hits[j]);
                }
            }
            return hitsList.ToArray();
        }
    }
    #endregion
}
