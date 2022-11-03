using System;
using System.Collections.Generic;
using PCG.Module.BOIDS.Bird;
using UnityEngine;

namespace PCG.Module.BOIDS
{
    public interface IBoidsState
    {
        void Begin(BoidsActor _actor);
        void End();
#if UNITY_EDITOR
        void DrawGizmosSelected(BoidsActor _actor);
#endif
    }
    public interface IStateSwitch<T>
    {
        bool TickStateSwitch(BoidsActor _actor, float _deltaTime, out T _nextState);
    }

    public interface IStateTransformApply
    {
        void TickTransform(BoidsActor _actor, IList<BoidsActor> _members, float _deltaTime, ref Vector3 _position, ref Quaternion _rotation);
    }
    public interface IStateTransformVelocity
    {
        float speed { get; }
        void TickVelocity(BoidsActor _actor, IList<BoidsActor> _members, float _deltaTime, ref Vector3 _direction);
    }

    public abstract class ABoidsBehaviour
    {
        protected BoidsActor m_Actor { get; private set; }
        public Vector3 m_Direction;
        protected Vector3 m_DesiredPosition;
        protected Quaternion m_DesiredRotation;
        public virtual void Initialize(BoidsActor _actor, FBoidsVertex _vertex)
        {
            m_Actor = _actor;
            m_Direction = _vertex.rotation * Vector3.forward;

            m_DesiredPosition = _vertex.position;
            m_DesiredRotation = _vertex.rotation;
        }

        public virtual void Recycle()
        {
            m_Actor = null;
        }

        public abstract void Tick(float _deltaTime, IList<BoidsActor> _members, out Vector3 _position, out Quaternion _rotation);

#if UNITY_EDITOR
        public abstract void DrawGizmosSelected();
#endif
    }

    public abstract class BoidsBehaviour<T> : ABoidsBehaviour where T : Enum
    {
        private readonly Ticker m_VelocityTicker = new Ticker(.05f);
        private IBoidsState m_States;
        private IStateSwitch<T> m_Switch;
        private IStateTransformVelocity m_TransformVelocity;
        private IStateTransformApply m_TransformApply;
        private T m_CurrentState;
        protected abstract IBoidsState CreateBehaviour(T _behaviourType);

        protected void SetBehaviour(T _behaviour)
        {
            if (m_States != null)
                m_States.End();

            m_States = null;
            m_Switch = null;
            m_TransformApply = null;
            m_TransformVelocity = null;

            m_CurrentState = _behaviour;
            m_States = CreateBehaviour(m_CurrentState);

            m_States.Begin(m_Actor);

            m_Switch = m_States as IStateSwitch<T>;

            m_TransformApply = m_States as IStateTransformApply;
            m_TransformVelocity = m_States as IStateTransformVelocity;
            if (m_TransformVelocity == null)
                m_Direction = Vector3.zero;

            m_VelocityTicker.Reset();
            m_VelocityTicker.Tick(m_VelocityTicker.m_Duration);
        }

        public override void Tick(float _deltaTime, IList<BoidsActor> _members, out Vector3 _position, out Quaternion _rotation)
        {
            TickVelocity(_deltaTime, _members);
            TickTransform(_deltaTime, _members);
            _position = m_DesiredPosition;
            _rotation = m_DesiredRotation;
            if (m_Switch != null && m_Switch.TickStateSwitch(m_Actor, _deltaTime, out T _nextState))
                SetBehaviour(_nextState);
        }
        void TickTransform(float _deltaTime, IList<BoidsActor> _flock)
        {
            if (m_TransformApply == null)
                return;
            m_TransformApply.TickTransform(m_Actor, _flock, _deltaTime, ref m_DesiredPosition, ref m_DesiredRotation);
        }

        void TickVelocity(float _deltaTime, IList<BoidsActor> _members)
        {
            if (m_TransformVelocity == null)
                return;

            if (m_VelocityTicker.Tick(_deltaTime))
            {
                float behaviourDeltaTime = m_VelocityTicker.m_Duration;
                m_TransformVelocity.TickVelocity(m_Actor, _members, behaviourDeltaTime, ref m_Direction);
            }

            m_DesiredPosition += m_Direction * (m_TransformVelocity.speed * _deltaTime);
            if (m_Direction.sqrMagnitude > 0)
            {
                Vector3 direction = m_Direction.normalized;
                float speed = Mathf.Clamp01(m_Direction.magnitude);
                m_Direction = direction * speed;
                m_DesiredRotation = Quaternion.LookRotation(direction, Vector3.up);
            }
        }

#if UNITY_EDITOR
        public override void DrawGizmosSelected()
        {
            Gizmos.color = Color.black;
            Gizmos.matrix = Matrix4x4.TRS(m_DesiredPosition, m_DesiredRotation, Vector3.one);
            Gizmos_Extend.DrawString(Vector3.up * .1f, m_CurrentState.ToString());
            m_States.DrawGizmosSelected(m_Actor);
        }
#endif
    }

    public static class BoidsBehaviour_Extend
    {
        public static Vector3 TickFlocking(this IBoidsState _state, BoidsActor _actor, IList<BoidsActor> _members, float _deltaTime, BoidsFlockingConfig _config)
        {
            Vector3 separation = Vector3.zero;
            Vector3 com = Vector3.zero;
            Vector3 alignment = Vector3.zero;
            int localBehaviourCount = 0;
            foreach (var flockActor in _members)
            {
                if (flockActor.m_Identity == _actor.m_Identity)
                    continue;
                if (_config.sqrVisualizeRange > 0 && (flockActor.Position - _actor.Position).sqrMagnitude > _config.sqrVisualizeRange)
                    continue;
                localBehaviourCount++;
                com += flockActor.Position;
                alignment += flockActor.Forward;

                Vector3 offset = flockActor.Position - _actor.Position;
                float sqrDistance = offset.sqrMagnitude;
                if (sqrDistance > _config.sqrSeparationDistance)
                    continue;
                separation -= offset;
            }
            Vector3 final = Vector3.zero;
            if (localBehaviourCount > 0)
            {
                com /= localBehaviourCount;
                final += (com - _actor.Position) * (_deltaTime * _config.cohesionDamping);
            }
            final += separation * (_deltaTime * _config.separateDamping);
            final += alignment * (_deltaTime * _config.alignmentDamping);
            return final;
        }

        public static Vector3 TickFollowing(this IBoidsState _state, BoidsActor _actor, IBoidsVertex _target, float _deltaTime, BoidsFollowingConfig _config)
        {
            Vector3 com = _target.Position;
            Vector3 alignment = _target.Forward;
            Vector3 final = Vector3.zero;
            final += (com - _actor.Position) * (_deltaTime * _config.cohesionDamping);
            final += alignment * (_deltaTime * _config.alignmentDamping);
            return final;
        }

        public static Vector3 TickMaintainHeight(this IBoidsState _state, BoidsActor _actor, Vector3 _offset,
            float _deltaTime, float _damping)
        {
            // Maintain Height
            Vector3 verticalDesire = Vector3.up * Mathf.Sign(_offset.y);
            if (Vector3.Dot(verticalDesire, _actor.Forward) < .7f)
                return verticalDesire * _damping * _deltaTime * Mathf.Abs(_offset.y) / 2f;
            return Vector3.zero;
        }

        public static Vector3 TickRandom(this IBoidsState _state, BoidsActor _actor, float _deltaTime)
        {
            return URandom.RandomSphere() * _deltaTime;
        }

        public static Vector3 TickEvading(this IBoidsState _state, BoidsActor _actor, float _deltaTime, BoidsEvadeConfig _config)
        {
            float distance = _config.evadeDistance;
            if (!Physics.Raycast(new Ray(_actor.Position, _actor.Rotation * Vector3.forward), out var hitInfo, distance, int.MaxValue))
                return Vector3.zero;

            Vector3 normal = hitInfo.normal;
            return normal * (_deltaTime * _config.evadeDamping);
        }
    }
}
