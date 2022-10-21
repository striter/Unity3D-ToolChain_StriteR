using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace PCG.Module.BOIDS.States
{
    public class Startle<T>:IBoidsState,IStateTransformVelocity,IStateSwitch<T> where T:Enum
    {
        public float speed => m_StartleConfig.speed;
        private Vector3 m_StartleDirection;
        private BoidsStartleConfig m_StartleConfig;
        private BoidsFlockingConfig m_FlockingConfig;
        private readonly Counter m_StartleCounter = new Counter();
        private readonly Counter m_ReactionCounter = new Counter();
        private T m_ChillBehaviour;
        public Startle(BoidsStartleConfig _config,BoidsFlockingConfig _flockingConfig,T _chillBehaviour)
        {
            m_StartleConfig = _config;
            m_FlockingConfig = _flockingConfig;
            m_ChillBehaviour = _chillBehaviour;
        }
        
        public void Begin(BoidsActor _actor)
        {
            m_StartleDirection = URandom.RandomDirection();
            m_StartleDirection *= Mathf.Sign(Vector3.Dot(m_StartleDirection,   _actor.m_Target.m_Up));
            m_StartleCounter.Set(m_StartleConfig.duration.Random());
            m_ReactionCounter.Set(m_StartleConfig.reaction.Random());
        }
        
        public void End()
        {
        }
        
        public bool TickStateSwitch(BoidsActor _actor, float _deltaTime,out T _nextBehaviour)
        {
            _nextBehaviour = m_ChillBehaviour;
            if (m_ReactionCounter.m_Playing)
                return false;
            
            m_StartleCounter.Tick(_deltaTime);
            return !m_StartleCounter.m_Playing;
        }
        
        public virtual void TickVelocity(BoidsActor _actor, IList<BoidsActor> _members, float _deltaTime,
            ref Vector3 _velocity)
        {
            if (m_ReactionCounter.m_Playing)       //Do nothing during reaction time
            {
                if(m_ReactionCounter.Tick(_deltaTime))
                    _actor.m_Animation.SetAnimation(m_StartleConfig.animName);

                return;
            }
            
            _velocity += this.TickFlocking(_actor,_members, _deltaTime,m_FlockingConfig);
            // _velocity += this.TickRandom(_actor,_deltaTime);
            _velocity += m_StartleDirection * (_deltaTime * m_StartleConfig.damping);
        }
        
#if UNITY_EDITOR
        public void DrawGizmosSelected(BoidsActor _actor)
        {
            Gizmos_Extend.DrawString(Vector3.up*.2f,$"{m_ReactionCounter:F1}");
        }
#endif
    }

    public class FloatFlocking : Floating
    {
        private BoidsFlockingConfig m_FlockingConfig;
        public FloatFlocking(BoidsFloatingConfig _floatingConfig, BoidsFlockingConfig _flockingConfig)
        {
            base.Init(_floatingConfig);
            m_FlockingConfig = _flockingConfig;
        }

        public override void TickVelocity(BoidsActor _actor, IList<BoidsActor> _members, float _deltaTime, ref Vector3 _velocity)
        {
            base.TickVelocity(_actor, _members, _deltaTime, ref _velocity);
            _velocity += this.TickFlocking(_actor, _members, _deltaTime, m_FlockingConfig);
        }
    }

    public class Floating : IBoidsState,IStateTransformVelocity
    {
        private BoidsFloatingConfig m_Config;
        public float speed => m_Config.speed;

        public Floating Init(BoidsFloatingConfig _floatingConfig)
        {
            m_Config = _floatingConfig;
            return this;
        }
        
        public void Begin(BoidsActor _actor)
        {
            _actor.m_Animation.SetAnimation(m_Config.anim);
        }

        public void End()
        {
            
        }


        public virtual void TickVelocity(BoidsActor _actor, IList<BoidsActor> _members, float _deltaTime,
            ref Vector3 _velocity)
        {
            _velocity += this.TickConstrain(_actor, _deltaTime);
            _velocity += this.TickRandom(_actor,_deltaTime);
        }


        bool IsConstrained(Vector3 _offset)
        {
            switch (m_Config.constrain)
            {
                default: throw new InvalidEnumArgumentException();
                case EBoidsFloatingConstrain.Spherical:
                    return _offset.sqrMagnitude > m_Config.sqrRadius;
            }
        }
        
        Vector3 TickConstrain(BoidsActor _actor,float _deltaTime)
        {
            Vector3 offset =  _actor.m_Target.m_Destination - _actor.Position;
            if (!IsConstrained(offset))
                return Vector3.zero;
            return offset * (_deltaTime * m_Config.damping);
        }
        
        public void DrawGizmosSelected(BoidsActor _actor)
        {
        }
    }
    
    public class Idle : IBoidsState
    {
        private BoidsIdleConfig m_Config;
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

#if UNITY_EDITOR
        public void DrawGizmosSelected(BoidsActor _actor)
        {
        }
#endif
    }
}