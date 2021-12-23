using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Boids
{
    public interface IBoidsBehaviour
    {
        void Begin(BoidsActor _actor);
        void Tick(BoidsActor _actor,float _deltaTime);
        void End();
        void DrawGizmosSelected();
    }

    public interface IBoidsTransformSetter
    {
        void TickTransform(BoidsActor _actor,float _deltaTime,ref Vector3 _position,ref Quaternion _rotation);
    }
    public interface IBoidsTransformVelocity
    {
        void TickVelocity(BoidsActor _actor, IEnumerable<BoidsActor> _flock, float _deltaTime,ref Vector3 _velocity);
    }
    

    public abstract class ABoidsBehaviourController
    {
        protected BoidsActor m_Actor;
        public IBoidsBehaviour m_Behaviour { get; set; }
        public IBoidsTransformVelocity TransformVelocity { get; set; }
        public IBoidsTransformSetter TransformSetter { get; set; }
        
        protected Vector3 m_Position;
        protected Quaternion m_Rotation;
        protected Vector3 m_Velocity;
        public abstract void Startle();
        public ABoidsBehaviourController Init(BoidsActor _actor)
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

        protected void Internal_SetBehaviour(IBoidsBehaviour _behaviour)
        {
            if (m_Behaviour != null)
                m_Behaviour.End();
            
            m_Behaviour = _behaviour;
            m_Behaviour.Begin(m_Actor);

            TransformSetter = m_Behaviour as IBoidsTransformSetter;
            TransformVelocity = m_Behaviour as IBoidsTransformVelocity;
            if (TransformVelocity == null)
                m_Velocity = Vector3.zero;
        }
        public virtual void DrawGizmosSelected()
        {
            Gizmos.DrawSphere(m_Position,.2f);
            Gizmos.DrawLine(m_Position,m_Position+m_Rotation*Vector3.forward*.4f);

            Gizmos.DrawLine(m_Position,m_Actor.m_Target.m_Destination);
            Gizmos.DrawWireSphere(m_Actor.m_Target.m_Destination,.5f);
            Gizmos.matrix = Matrix4x4.TRS(m_Position,m_Rotation,Vector3.one);
            
            m_Behaviour?.DrawGizmosSelected();
        }
        
        public Vector3 Position => m_Position;
        public Quaternion Rotation => m_Rotation;
        public Vector3 Velocity => m_Velocity;
    }
    
    public abstract class BoidsBehaviourController<T>:ABoidsBehaviourController where T: class, IBoidsBehaviour
    {
        private readonly Ticker m_VelocityTicker = new Ticker(.05f);
        public new T m_Behaviour { get; private set; }
        protected void SetBehaviour(T _behaviour)
        {
            m_Behaviour = _behaviour;
            Internal_SetBehaviour(m_Behaviour);
            m_VelocityTicker.Reset();
            m_VelocityTicker.Tick(m_VelocityTicker.m_Duration);
        }
        
        public override void Tick(float _deltaTime,IEnumerable<BoidsActor> _flock,out Vector3 _position,out Quaternion _rotation)
        {
            m_Behaviour.Tick(m_Actor,_deltaTime);
            TickTransform(_deltaTime);
            TickVelocity(_deltaTime,_flock);
            
            _position = m_Position;
            _rotation = m_Rotation;
        }

        void TickTransform(float _deltaTime)
        {
            if (TransformSetter == null)
                return;
            TransformSetter.TickTransform(m_Actor,_deltaTime,ref m_Position,ref m_Rotation);
        }
        
        void TickVelocity(float _deltaTime,IEnumerable<BoidsActor> _flock)
        {
            if (TransformVelocity == null)
                return;
            
            if (m_VelocityTicker.Tick(_deltaTime))
            {
                float behaviourDeltaTime = m_VelocityTicker.m_Duration;
                TransformVelocity.TickVelocity(m_Actor,_flock,behaviourDeltaTime,ref m_Velocity);
            }
            
            m_Position += m_Velocity * ( m_Actor.m_Config.speed * _deltaTime);
            if (m_Velocity.sqrMagnitude > 0)
            {
                Vector3 direction = m_Velocity.normalized;
                float speed = Mathf.Clamp01(m_Velocity.magnitude);
                m_Velocity = direction * speed;
                m_Rotation = Quaternion.Lerp(m_Rotation,Quaternion.LookRotation(direction,Vector3.up),_deltaTime*5f);
            }
        }
        
        public override void DrawGizmosSelected()
        {
            Gizmos.matrix=Matrix4x4.identity;
            Gizmos.DrawSphere(m_Position,.2f);
            Gizmos.DrawLine(m_Position,m_Position+m_Rotation*Vector3.forward*.4f);

            Gizmos.DrawLine(m_Position,m_Actor.m_Target.m_Destination);
            Gizmos.DrawWireSphere(m_Actor.m_Target.m_Destination,.5f);
            Gizmos.matrix = Matrix4x4.TRS(m_Position,m_Rotation,Vector3.one);

            m_Behaviour.DrawGizmosSelected();
        }
    }

    
    public static class BoidsBehaviour_Extend
    {        
        public static Vector3 TickFlocking(this IBoidsBehaviour _behaviour,BoidsActor _actor,IEnumerable<BoidsActor> _flock, float _deltaTime ) 
        {
            BoidFlockingConfig _config = _actor.m_Config.flockingConfig;
            Vector3 separation = Vector3.zero;
            Vector3 com = Vector3.zero;
            Vector3 alignment = Vector3.zero;
            int localBehaviourCount = 0;
            foreach (var flockActor in _flock)
            {
                if(flockActor.m_Identity==_actor.m_Identity)
                    continue;
                if((flockActor.m_Behaviour.Position-_actor.m_Behaviour.Position).sqrMagnitude>_config.sqrVisualizeRange)
                    continue;
                localBehaviourCount++;
                com += flockActor.m_Behaviour.Position;
                alignment += flockActor.m_Behaviour.Velocity;
                
                Vector3 offset = flockActor.m_Behaviour.Position - _actor.m_Behaviour.Position;
                float sqrDistance = offset.sqrMagnitude;
                if(sqrDistance>_config.sqrSeparationDistance)
                    continue;
                separation -= offset;
            }
            Vector3 final = Vector3.zero;
            if (localBehaviourCount > 0)
            {
                com /= localBehaviourCount;
                final +=  (com - _actor.m_Behaviour.Position) * (_deltaTime * _config.cohesionDamping);
            }
            final += separation * (_deltaTime * _config.separateDamping);
            final +=  alignment * (_deltaTime * _config.alignmentDamping);
            return final;
        }

        public static Vector3 TickHovering(this IBoidsBehaviour _behaviour,BoidsActor _actor, float _deltaTime)
        {
            BoidHoveringConfig _config = _actor.m_Config.hoveringConfig;
            Vector3 centerOffset = _actor.m_Behaviour.Position - _actor.m_Target.m_Destination;
            Vector3 direction = Vector3.zero;
            switch (_config.type)
            {
                case EBoidHovering.AroundHeight:      //Circle Around Center
                {
                    Vector3 hoverDirection = new Vector3(centerOffset.x,0f, centerOffset.z).normalized;
                    Vector3 hoverTangent = Vector3.Cross(hoverDirection,Vector3.up);
            
                    Vector3 hoverTowards = _actor.m_Target.m_Destination +  hoverDirection * _config.distance + Vector3.up * _config.height;
                    Vector3 hoverOffset = hoverTowards - _actor.m_Behaviour.Position;
            
                    float hoverElapse = hoverOffset.sqrMagnitude/(_config.distance*_config.height);
                    direction = (hoverOffset.normalized + hoverTangent)*hoverElapse;
                }
                break;
                case EBoidHovering.Disorder:    //Force Inside Center
                {
                    if (centerOffset.sqrMagnitude < _config.distance * _config.distance)
                        return Vector3.zero;
                    direction = -centerOffset;
                }
                break;
            }
            return direction * (_deltaTime * _config.damping);
        }

        public static Vector3 TickRandom(this IBoidsBehaviour _behaviour,BoidsActor _actor,float _deltaTime)
        {
            return URandom.RandomUnitSphere();
        }
        public static Vector3 TickHoverTowards(this IBoidsBehaviour _behaviour, BoidsActor _actor, float _deltaTime)
        {
            Vector3 centerOffset = _actor.m_Behaviour.Position - _actor.m_Target.m_Destination;

            Vector3 hoverDirection = new Vector3(centerOffset.x,0f, centerOffset.z).normalized;
            Vector3 hoverTangent = Vector3.Cross(hoverDirection,Vector3.up);
    
            Vector3 hoverTowards = _actor.m_Target.m_Destination +  hoverDirection * 2f + Vector3.up * 1f;
            Vector3 hoverOffset = hoverTowards - _actor.m_Behaviour.Position;
    
            Vector3 direction = (hoverOffset.normalized + hoverTangent);
            return direction * (_deltaTime * _actor.m_Config.hoveringConfig.damping);  
        }
        
        public static Vector3 TickFlapping(this IBoidsBehaviour _behaviour, BoidsActor _actor,float _deltaTime)
        {
            return Vector3.up * _deltaTime;
        }

        public static Vector3 TickAvoid(this IBoidsBehaviour _behaviour, BoidsActor _actor,float _deltaTime)
        {
            if (!_actor.m_Config.evade)
                return Vector3.zero;
            BoidEvadeConfig _config = _actor.m_Config.evadeConfig;
            float distance = _config.evadeDistance;
            if (!Physics.Raycast(new Ray(_actor.m_Behaviour.Position,_actor.m_Behaviour.Rotation*Vector3.forward),out var hitInfo,distance,int.MaxValue))
                return Vector3.zero;

            Vector3 normal = hitInfo.normal;
            return normal * (_deltaTime * _config.evadeDamping);
        }
    }
}
