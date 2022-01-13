using System;
using System.Collections.Generic;
using UnityEngine;

namespace Boids
{
    public interface IBoidsState
    {
        void Begin(BoidsActor _actor);
        void End();
        #if UNITY_EDITOR
        void DrawGizmosSelected(BoidsActor _actor);
        #endif
    }
    public interface IStateTransformApply
    {
        void TickTransform(BoidsActor _actor,IEnumerable<BoidsActor> _flock, float _deltaTime,ref Vector3 _position,ref Quaternion _rotation);
    }
    public interface IStateTransformVelocity
    {
        float speed { get; }
        void TickVelocity(BoidsActor _actor, IEnumerable<BoidsActor> _flock, float _deltaTime,ref Vector3 _velocity);
    }
    
    public interface IStateSwitch<T>
    {
        bool TickStateSwitch(BoidsActor _actor, float _deltaTime,out T _nextState);
    }

    public abstract class ABoidsBehaviour
    {
        protected BoidsActor m_Actor { get; private set; }
        public Vector3 m_Velocity;
        protected Vector3 m_DesiredPosition;
        protected Quaternion m_DesiredRotation;
        public virtual void Spawn(BoidsActor _actor,Matrix4x4 _landing)
        {
            m_Actor = _actor;
            m_Velocity = _landing.MultiplyVector(URandom.RandomVector3());
            
            m_DesiredPosition = _landing.MultiplyPoint(Vector3.zero);
            m_DesiredRotation = _landing.rotation;
        }

        public virtual void Recycle()
        {
            m_Actor = null;
        }

        public abstract void Tick(float _deltaTime, IEnumerable<BoidsActor> _flock,out Vector3 _position,out  Quaternion _rotation);

#if UNITY_EDITOR
        public abstract void DrawGizmosSelected();
#endif
    }
    
    public abstract class BoidsBehaviour<T> : ABoidsBehaviour where T:Enum 
    {
        private readonly Ticker m_VelocityTicker = new Ticker(.05f);
        private IBoidsState m_State;
        private IStateSwitch<T> m_Switch;
        private IStateTransformVelocity m_TransformVelocity;
        private IStateTransformApply m_TransformApply;
        private T m_CurrentState;
        protected abstract IBoidsState SpawnBehaviour(T _behaviourType);
        protected abstract void RecycleBehaviour(T _behaviourType,IBoidsState _state);

        protected void SetBehaviour(T _behaviour)
        {
            if (m_State != null)
            {
                m_State.End();
                RecycleBehaviour(m_CurrentState,m_State);
            }

            m_State = null;
            m_Switch = null;
            m_TransformApply = null;
            m_TransformVelocity = null;

            m_CurrentState = _behaviour;
            m_State = SpawnBehaviour(m_CurrentState);
            
            m_State.Begin(m_Actor);

            m_Switch=m_State as IStateSwitch<T>;
            m_TransformApply = m_State as IStateTransformApply;
            m_TransformVelocity = m_State as IStateTransformVelocity;
            if (m_TransformVelocity == null)
                m_Velocity = Vector3.zero;
            
            m_VelocityTicker.Reset();
            m_VelocityTicker.Tick(m_VelocityTicker.m_Duration);
        }
        
        public override void Tick(float _deltaTime,IEnumerable<BoidsActor> _flock,out Vector3 _position,out Quaternion _rotation)
        {
            if (m_Switch != null && m_Switch.TickStateSwitch(m_Actor, _deltaTime,out T _nextState))
                SetBehaviour(_nextState);
            
            TickVelocity(_deltaTime,_flock);
            TickTransform(_deltaTime,_flock);
            _position = m_DesiredPosition;
            _rotation = m_DesiredRotation;
        }
        void TickTransform(float _deltaTime,IEnumerable<BoidsActor> _flock)
        {
            if (m_TransformApply == null)
                return;
            m_TransformApply.TickTransform(m_Actor,_flock,_deltaTime,ref m_DesiredPosition,ref m_DesiredRotation);
        }
        
        void TickVelocity(float _deltaTime,IEnumerable<BoidsActor> _flock)
        {
            if (m_TransformVelocity == null)
                return;
            
            if (m_VelocityTicker.Tick(_deltaTime))
            {
                float behaviourDeltaTime = m_VelocityTicker.m_Duration;
                m_TransformVelocity.TickVelocity(m_Actor,_flock,behaviourDeltaTime,ref m_Velocity);
            }
            
            m_DesiredPosition += m_Velocity * ( m_TransformVelocity.speed * _deltaTime);
            if (m_Velocity.sqrMagnitude > 0)
            {
                Vector3 direction = m_Velocity.normalized;
                float speed = Mathf.Clamp01(m_Velocity.magnitude);
                m_Velocity = direction * speed;
                m_DesiredRotation = Quaternion.LookRotation(direction,Vector3.up);
            }
        }
        
#if UNITY_EDITOR
        public override void DrawGizmosSelected()
        {
            Gizmos.color = Color.black;
            Gizmos.matrix = Matrix4x4.TRS(m_DesiredPosition,m_DesiredRotation,Vector3.one);
            Gizmos_Extend.DrawString(Vector3.up*.1f,m_CurrentState.ToString());
            m_State.DrawGizmosSelected(m_Actor);
        }
#endif
    }

    
    public static class BoidsBehaviour_Extend
    {
        public static Vector3 TickFlocking(this IBoidsState _state,BoidsActor _actor,IEnumerable<BoidsActor> _flock, float _deltaTime,BoidsFlockingConfig _config) 
        {
            Vector3 separation = Vector3.zero;
            Vector3 com = Vector3.zero;
            Vector3 alignment = Vector3.zero;
            int localBehaviourCount = 0;
            foreach (var flockActor in _flock)
            {
                if(flockActor.m_Identity==_actor.m_Identity)
                    continue;
                if((flockActor.Position-_actor.Position).sqrMagnitude>_config.sqrVisualizeRange)
                    continue;
                localBehaviourCount++;
                com += flockActor.Position;
                alignment += flockActor.Velocity;
                
                Vector3 offset = flockActor.Position - _actor.Position;
                float sqrDistance = offset.sqrMagnitude;
                if(sqrDistance>_config.sqrSeparationDistance)
                    continue;
                separation -= offset;
            }
            Vector3 final = Vector3.zero;
            if (localBehaviourCount > 0)
            {
                com /= localBehaviourCount;
                final +=  (com - _actor.Position) * (_deltaTime * _config.cohesionDamping);
            }
            final += separation * (_deltaTime * _config.separateDamping);
            final +=  alignment * (_deltaTime * _config.alignmentDamping);
            return final;
        }

        public static Vector3 TickRandom(this IBoidsState _state,BoidsActor _actor,float _deltaTime)
        {
            return URandom.RandomUnitSphere()*_deltaTime;
        }
        
        public static Vector3 TickEvading(this IBoidsState _state,BoidsActor _actor,float _deltaTime,BoidsEvadeConfig _config)
        {
            float distance = _config.evadeDistance;
            if (!Physics.Raycast(new Ray(_actor.Position,_actor.Rotation*Vector3.forward),out var hitInfo,distance,int.MaxValue))
                return Vector3.zero;

            Vector3 normal = hitInfo.normal;
            return normal * (_deltaTime * _config.evadeDamping);
        }
    }
}
