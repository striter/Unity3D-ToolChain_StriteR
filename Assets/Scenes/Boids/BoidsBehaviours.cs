using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boids.Behaviours
{
    public class Startle<T>:IBoidsState,IStateTransformVelocity,IStateSwitch<T> where T:Enum
    {
        public float speed => m_StartleConfig.speed;
        private Vector3 m_StartleDirection;
        private BoidsStartleConfig m_StartleConfig;
        private BoidsFlockingConfig m_FlockingConfig;
        public T m_NextBehaviour { get; private set; }
        public readonly Timer m_StartleTimer = new Timer();
        public Startle<T> Init(BoidsStartleConfig _config,BoidsFlockingConfig _flockingConfig,T _nextBehaviour)
        {
            m_StartleConfig = _config;
            m_FlockingConfig = _flockingConfig;
            m_NextBehaviour = _nextBehaviour;
            return this;
        }
        
        public void Begin(BoidsActor _actor)
        {
            _actor.m_Animation.SetAnimation(m_StartleConfig.animName);
            m_StartleDirection = URandom.RandomVector3();
            m_StartleDirection *= Mathf.Sign(Vector3.Dot(m_StartleDirection, Vector3.up));
            m_StartleTimer.Set(m_StartleConfig.duration.Random());
        }
        
        public void End()
        {
        }


        public bool TickStateSwitch(BoidsActor _actor, float _deltaTime)
        {
            m_StartleTimer.Tick(_deltaTime);
            return !m_StartleTimer.m_Timing;
        }
        public void TickVelocity(BoidsActor _actor, ABoidsBehaviour _behaviour, IEnumerable<ABoidsBehaviour> _flock, float _deltaTime,
            ref Vector3 _velocity)
        {
            _velocity += _behaviour.TickFlocking(_flock, _deltaTime,m_FlockingConfig);
            _velocity += _behaviour.TickRandom(_deltaTime);
            _velocity += m_StartleDirection*_deltaTime*m_StartleConfig.damping;
        }
        
        public void DrawGizmosSelected()
        {
        }

    }

    public class Flying<T> : IBoidsState,IStateTransformVelocity,IStateSwitch<T> where T:Enum
    {
        public float speed => m_Config.speed;
        BoidsFlyingConfig m_Config;
        BoidsFlockingConfig m_FlockingConfig;
        BoidsEvadeConfig m_EvadeConfig;

        private Timer m_TiringTimer = new Timer();
        private readonly Timer m_AnimationTimer = new Timer(1f);
        private readonly ValueChecker<bool> m_Gliding=new ValueChecker<bool>();
        
        public T m_NextBehaviour { get; private set; }
        public Flying<T> Init(BoidsFlyingConfig _hovering,BoidsFlockingConfig _flocking,BoidsEvadeConfig _evade,T _nextBehaviour)
        {
            m_FlockingConfig = _flocking;
            m_Config = _hovering;
            m_EvadeConfig = _evade;
            m_NextBehaviour = _nextBehaviour;
            return this;
        }
        
        public void Begin(BoidsActor _actor)
        {
            m_TiringTimer.Set(m_Config.tiringDuration.Random());
            m_Gliding.Check(true);
            _actor.m_Animation.SetAnimation(m_Config.flyAnim);
            m_AnimationTimer.Replay();
        }

        public void End()
        {
        }

        public bool TickStateSwitch(BoidsActor _actor, float _deltaTime)
        {
            m_TiringTimer.Tick(_deltaTime);
            return !m_TiringTimer.m_Timing;
        }

        public void TickVelocity(BoidsActor _actor, ABoidsBehaviour _behaviour, IEnumerable<ABoidsBehaviour> _flock, float _deltaTime,
            ref Vector3 _velocity)
        {
            Vector3 offset = _actor.m_Target.m_Destination - _behaviour.Position;
            float sqrDistance = offset.sqrMagnitude;
            float height = offset.y;

            if (sqrDistance > m_Config.sqrBorder)
                _velocity += offset.normalized;

            _velocity += (height - m_Config.maintainHeight) / m_Config.maintainHeight * Vector3.up;
            
            _velocity += _behaviour.TickFlocking(_flock,_deltaTime,m_FlockingConfig);
            _velocity += _behaviour.TickEvading(_deltaTime,m_EvadeConfig);
            
            m_AnimationTimer.Tick(_deltaTime);
            if (m_AnimationTimer.m_Timing)
                return;
            
            if (m_Gliding.Check(_velocity.y > 0))
            {
                _actor.m_Animation.SetAnimation(m_Gliding?m_Config.glideAnim:m_Config.flyAnim);
                m_AnimationTimer.Replay();
            }
        }
        
        public void DrawGizmosSelected()
        {
        }

    }

    public class Floating : IBoidsState,IStateTransformVelocity
    {
        private BoidsFloatingConfig m_Config;
        private BoidsFlockingConfig m_FlockingConfig;
        public float speed => m_Config.speed;

        public Floating Init(BoidsFloatingConfig _floatingConfig, BoidsFlockingConfig _flockingConfig)
        {
            m_Config = _floatingConfig;
            m_FlockingConfig = _flockingConfig;
            return this;
        }
        public void Begin(BoidsActor _actor)
        {
            _actor.m_Animation.SetAnimation(m_Config.anim);
        }

        public void End()
        {
            
        }


        public void TickVelocity(BoidsActor _actor, ABoidsBehaviour _behaviour, IEnumerable<ABoidsBehaviour> _flock, float _deltaTime,
            ref Vector3 _velocity)
        {
            
            Vector3 offset = _behaviour.Position - _actor.m_Target.m_Destination;
            if (offset.sqrMagnitude > m_Config.sqrRadius) _velocity+= offset * _deltaTime * m_Config.damping;
            
            _velocity += _behaviour.TickFlocking(_flock,_deltaTime,m_FlockingConfig);
        }
        
        public void DrawGizmosSelected()
        {
        }
    }
    public class Hovering<T> : IBoidsState,IStateTransformVelocity,IStateSwitch<T> where T:Enum
    {
        private BoidsHoveringConfig m_Config;
        private BoidsFlockingConfig m_FlockConfig;
        private BoidsEvadeConfig m_EvadeConfig;
        public float speed => m_Config.speed;
        public T m_NextBehaviour { get; private set; }

        private readonly Timer m_HoverTimer=new Timer();
        public Hovering<T> Init(BoidsHoveringConfig _hoverConfig,BoidsFlockingConfig _flockConfig,BoidsEvadeConfig _evadeConfig,T _nextBehaviour) 
        {
            m_Config = _hoverConfig;
            m_FlockConfig = _flockConfig;
            m_EvadeConfig = _evadeConfig;
            m_NextBehaviour = _nextBehaviour;
            return this;
        }
        public void Begin(BoidsActor _actor)
        {
            _actor.m_Animation.SetAnimation(m_Config.flyAnim);
            m_HoverTimer.Set(m_Config.duration.Random());
        }

        public void End()
        {
        }
        public bool TickStateSwitch(BoidsActor _actor, float _deltaTime)
        {
            m_HoverTimer.Tick(_deltaTime);
            return !m_HoverTimer.m_Timing;
        }
        public void TickVelocity(BoidsActor _actor, ABoidsBehaviour _behaviour, IEnumerable<ABoidsBehaviour> _flock, float _deltaTime,
            ref Vector3 _velocity)
        {
            Vector3 _destination = _actor.m_Target.m_Destination;
            
            Vector3 centerOffset = _behaviour.Position - _destination;
            Vector3 hoverDirection = new Vector3(centerOffset.x,0f, centerOffset.z).normalized;
            Vector3 hoverTangent = Vector3.Cross(hoverDirection,Vector3.up);
    
            Vector3 hoverTowards = _destination +  hoverDirection * m_Config.distance + Vector3.up * m_Config.height;
            Vector3 hoverOffset = hoverTowards - _behaviour.Position;
    
            float hoverElapse = hoverOffset.sqrMagnitude/(m_Config.distance*m_Config.height);
            Vector3 direction = (hoverOffset.normalized + hoverTangent)*hoverElapse;
            
            _velocity += direction * (_deltaTime * m_Config.damping);
            
            _velocity += _behaviour.TickFlocking(_flock,_deltaTime,m_FlockConfig);
            _velocity += _behaviour.TickEvading(_deltaTime,m_EvadeConfig);
        }
        public void DrawGizmosSelected()
        {
        }

    }

    public class Landing<T> : IBoidsState,IStateTransformVelocity,IStateTransformSetter,IStateSwitch<T> where T:Enum
    {
        private BoidsLandingConfig m_Config;
        public float speed => m_Config.speed;
        public T m_NextBehaviour { get; private set; }
        public Landing<T> Init(BoidsLandingConfig _config,T _nextBehaviour)
        {
            m_Config = _config;
            m_NextBehaviour = _nextBehaviour;
            return this;
        }
        public bool TickStateSwitch(BoidsActor _actor, float _deltaTime)
        {
            var sqrDistance = (_actor.m_Behaviour.Position - _actor.m_Target.m_Destination).sqrMagnitude;
            return sqrDistance < m_Config.distanceBias;
        }
        public void Begin(BoidsActor _actor)
        {
            _actor.m_Animation.SetAnimation(m_Config.anim);
        }

        public void End()
        {
            
        }

        public void TickVelocity(BoidsActor _actor, ABoidsBehaviour _behaviour, IEnumerable<ABoidsBehaviour> _flock, float _deltaTime,
            ref Vector3 _velocity)
        {
            _velocity += (_actor.m_Target.m_Destination - _actor.m_Behaviour.Position).normalized * _deltaTime * m_Config.damping;
        }


        public void TickTransform(BoidsActor _actor, ABoidsBehaviour _behaviour, IEnumerable<BoidsActor> _flock, float _deltaTime,
            ref Vector3 _position, ref Quaternion _rotation)
        {
            float sqrDistance = speed*speed;
            Vector3 direction =  _actor.m_Target.m_Destination - _actor.m_Behaviour.Position;
            float scale = direction.sqrMagnitude / sqrDistance;
            _rotation = Quaternion.Lerp(Quaternion.LookRotation(direction,Vector3.up),  Quaternion.LookRotation(Vector3.up,-direction),scale);
        }

        public void DrawGizmosSelected()
        {
        }

    }


    public class Perching : IBoidsState,IStateTransformSetter
    {
        private readonly Timer m_Stand = new Timer(5f);
        private readonly Timer m_HalfStand = new Timer(5f);

        private BoidsPerchConfig m_PerchConfig;
        public Perching Init(BoidsPerchConfig _config)
        {
            m_PerchConfig = _config;
            return this;
        }
        public void Begin(BoidsActor _actor)
        {
            m_Stand.Replay();
            m_HalfStand.Replay();
            _actor.m_Animation.SetAnimation(m_PerchConfig.standAnim);
        }

        public void End()
        {
            
        }

        public void TickTransform(BoidsActor _actor, ABoidsBehaviour _behaviour, IEnumerable<BoidsActor> _flock, float _deltaTime,
            ref Vector3 _position, ref Quaternion _rotation)
        {
            _rotation = Quaternion.LookRotation(Vector3.forward,Vector3.up);

            if (TickMoving(_actor,_deltaTime))
                return;

            if (TickStanding(_actor, _deltaTime))
                return;
        }
        
        bool TickMoving(BoidsActor _actor, float _deltaTime)
        {
            if (!m_Stand.m_Timing)
                return false;

            m_Stand.Tick(_deltaTime);
            if(!m_Stand.m_Timing)
                _actor.m_Animation.SetAnimation(m_PerchConfig.standAnim);
            return true;
        }

        bool TickStanding(BoidsActor _actor,float _deltaTime)
        {
            if (!m_HalfStand.m_Timing)
                return false;
            m_HalfStand.Tick(_deltaTime);
            if (!m_HalfStand.m_Timing)
                _actor.m_Animation.SetAnimation(m_PerchConfig.standAnim);
            return true;
        }

        public void DrawGizmosSelected()
        {
        }

    }

    public class Idle : IBoidsState,IStateTransformSetter
    {
        public BoidsIdleConfig m_Config;
        public Idle Init(BoidsIdleConfig _config)
        {
            m_Config = _config;
            return this;
        }
        public void Begin(BoidsActor _actor)
        {
            _actor.m_Animation.SetAnimation(m_Config.anim);
        }

        public void End()
        {
        }


        public void DrawGizmosSelected()
        {
        }

        public void TickTransform(BoidsActor _actor, ABoidsBehaviour _behaviour, IEnumerable<BoidsActor> _flock, float _deltaTime,
            ref Vector3 _position, ref Quaternion _rotation)
        {
        }
    }
}