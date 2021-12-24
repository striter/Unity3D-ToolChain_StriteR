using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Boids
{
    public interface IBoidsState
    {
        void Begin(BoidsActor _actor);
        void End();
        void DrawGizmosSelected();
    }

    public interface IStateTransformSetter
    {
        void TickTransform(BoidsActor _actor,ABoidsBehaviour _behaviour,IEnumerable<ABoidsBehaviour> _flock, float _deltaTime,ref Vector3 _position,ref Quaternion _rotation);
    }
    public interface IStateTransformVelocity
    {
        float speed { get; }
        void TickVelocity(BoidsActor _actor,ABoidsBehaviour _behaviour, IEnumerable<ABoidsBehaviour> _flock, float _deltaTime,ref Vector3 _velocity);
    }
    
    public interface IStateSwitch<T> where T:Enum
    {
        T m_NextBehaviour { get; }
        bool TickStateSwitch(BoidsActor _actor, float _deltaTime);
    }

    public abstract class ABoidsBehaviour
    {
        protected BoidsActor m_Actor;
        public int Identity => m_Actor.m_Identity;
        protected Vector3 m_Position;
        protected Quaternion m_Rotation;
        protected Vector3 m_Velocity;
        public ABoidsBehaviour Init(BoidsActor _actor)
        {
            m_Actor = _actor;
            return this;
        }

        public virtual void Spawn(Vector3 _position,Quaternion rotation)
        {
            m_Position = _position;
            m_Rotation = rotation;
        }

        public virtual void Recycle()
        {
            m_Actor = null;
        }

        public abstract void Tick(float _deltaTime, IEnumerable<BoidsActor> _flock, out Vector3 position, out Quaternion rotation);

        public virtual void DrawGizmosSelected()
        {
            Gizmos.DrawSphere(m_Position,.2f);
            Gizmos.DrawLine(m_Position,m_Position+m_Rotation*Vector3.forward*.4f);

            Gizmos.DrawLine(m_Position,m_Actor.m_Target.m_Destination);
            Gizmos.DrawWireSphere(m_Actor.m_Target.m_Destination,.5f);
            Gizmos.matrix = Matrix4x4.TRS(m_Position,m_Rotation,Vector3.one);
        }
        
        public Vector3 Position => m_Position;
        public Quaternion Rotation => m_Rotation;
        public Vector3 Velocity => m_Velocity;
    }
    
    public abstract class BoidsBehaviour<T> : ABoidsBehaviour where T:Enum 
    {
        private readonly Ticker m_VelocityTicker = new Ticker(.05f);
        private IBoidsState m_State;
        private IStateSwitch<T> m_Switch;
        private IStateTransformVelocity m_TransformVelocity;
        private IStateTransformSetter m_TransformSetter;
        
        public T m_StateType { get; private set; }
        protected abstract IBoidsState SpawnBehaviour(T _behaviourType);
        protected abstract void RecycleBehaviour(T _behaviourType,IBoidsState state);
        public void SetBehaviour(T _behaviour)
        {
            if (m_State != null)
            {
                m_State.End();
                RecycleBehaviour(m_StateType,m_State);
            }

            m_State = null;
            m_Switch = null;
            m_TransformSetter = null;
            m_TransformVelocity = null;

            m_StateType = _behaviour;
            m_State=SpawnBehaviour(m_StateType);
            m_State.Begin(m_Actor);

            m_Switch=m_State as IStateSwitch<T>;
            m_TransformSetter = m_State as IStateTransformSetter;
            m_TransformVelocity = m_State as IStateTransformVelocity;
            if (m_TransformVelocity == null)
                m_Velocity = Vector3.zero;
            
            m_VelocityTicker.Reset();
            m_VelocityTicker.Tick(m_VelocityTicker.m_Duration);
        }

        
        public override void Tick(float _deltaTime,IEnumerable<BoidsActor> _flock,out Vector3 _position,out Quaternion _rotation)
        {
            if (m_Switch != null && m_Switch.TickStateSwitch(m_Actor, _deltaTime))
                SetBehaviour((T)Enum.ToObject(typeof(T),m_Switch.m_NextBehaviour));
            
            TickTransform(_deltaTime,_flock);
            TickVelocity(_deltaTime,_flock);
            
            _position = m_Position;
            _rotation = m_Rotation;
        }

        void TickTransform(float _deltaTime,IEnumerable<BoidsActor> _flock)
        {
            if (m_TransformSetter == null)
                return;
            m_TransformSetter.TickTransform(m_Actor,this,_flock.Select(p=>p.m_Behaviour),_deltaTime,ref m_Position,ref m_Rotation);
        }
        
        void TickVelocity(float _deltaTime,IEnumerable<BoidsActor> _flock)
        {
            if (m_TransformVelocity == null)
                return;
            
            if (m_VelocityTicker.Tick(_deltaTime))
            {
                float behaviourDeltaTime = m_VelocityTicker.m_Duration;
                m_TransformVelocity.TickVelocity(m_Actor,this,_flock.Select(p=>p.m_Behaviour),behaviourDeltaTime,ref m_Velocity);
            }
            
            m_Position += m_Velocity * ( m_TransformVelocity.speed * _deltaTime);
            if (m_Velocity.sqrMagnitude > 0)
            {
                Vector3 direction = m_Velocity.normalized;
                float speed = Mathf.Clamp01(m_Velocity.magnitude);
                m_Velocity = direction * speed;
                m_Rotation = Quaternion.LookRotation(direction,Vector3.up);
            }
        }
        
        public override void DrawGizmosSelected()
        {
            Gizmos.matrix=Matrix4x4.identity;
            Gizmos.DrawSphere(m_Position,.1f);
            Gizmos.DrawLine(m_Position,m_Position+m_Rotation*Vector3.forward*.2f);

            Gizmos.DrawLine(m_Position,m_Actor.m_Target.m_Destination);
            Gizmos.DrawWireSphere(m_Actor.m_Target.m_Destination,.2f);
            Gizmos.matrix = Matrix4x4.TRS(m_Position,m_Rotation,Vector3.one);

            Gizmos_Extend.DrawString(Vector3.up*.1f,m_StateType.ToString());
            m_State.DrawGizmosSelected();
        }
    }

    
    public static class BoidsBehaviour_Extend
    {
        public static Vector3 TickFlocking(this ABoidsBehaviour _behaviour,IEnumerable<ABoidsBehaviour> _flock, float _deltaTime,BoidsFlockingConfig _config) 
        {
            Vector3 separation = Vector3.zero;
            Vector3 com = Vector3.zero;
            Vector3 alignment = Vector3.zero;
            int localBehaviourCount = 0;
            foreach (var flockActor in _flock)
            {
                if(flockActor.Identity==_behaviour.Identity)
                    continue;
                if((flockActor.Position-_behaviour.Position).sqrMagnitude>_config.sqrVisualizeRange)
                    continue;
                localBehaviourCount++;
                com += flockActor.Position;
                alignment += flockActor.Velocity;
                
                Vector3 offset = flockActor.Position - _behaviour.Position;
                float sqrDistance = offset.sqrMagnitude;
                if(sqrDistance>_config.sqrSeparationDistance)
                    continue;
                separation -= offset;
            }
            Vector3 final = Vector3.zero;
            if (localBehaviourCount > 0)
            {
                com /= localBehaviourCount;
                final +=  (com - _behaviour.Position) * (_deltaTime * _config.cohesionDamping);
            }
            final += separation * (_deltaTime * _config.separateDamping);
            final +=  alignment * (_deltaTime * _config.alignmentDamping);
            return final;
        }

        public static Vector3 TickRandom(this ABoidsBehaviour _behaviour,float _deltaTime)
        {
            return URandom.RandomUnitSphere()*_deltaTime;
        }
        
        public static Vector3 TickEvading(this ABoidsBehaviour _behaviour,float _deltaTime,BoidsEvadeConfig _config)
        {
            float distance = _config.evadeDistance;
            if (!Physics.Raycast(new Ray(_behaviour.Position,_behaviour.Rotation*Vector3.forward),out var hitInfo,distance,int.MaxValue))
                return Vector3.zero;

            Vector3 normal = hitInfo.normal;
            return normal * (_deltaTime * _config.evadeDamping);
        }
    }
}
