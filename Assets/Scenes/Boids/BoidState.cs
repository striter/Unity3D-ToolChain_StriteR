using System;
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
        private readonly Counter m_StartleCounter = new Counter();
        private readonly Counter m_ReactionCounter = new Counter();
        public Startle<T> Init(BoidsStartleConfig _config,BoidsFlockingConfig _flockingConfig,T _nextBehaviour)
        {
            m_StartleConfig = _config;
            m_FlockingConfig = _flockingConfig;
            m_NextBehaviour = _nextBehaviour;
            return this;
        }
        
        public void Begin(BoidsActor _actor)
        {
            m_StartleDirection = URandom.RandomVector3();
            m_StartleDirection *= Mathf.Sign(Vector3.Dot(m_StartleDirection, Vector3.up));
            m_StartleCounter.Set(m_StartleConfig.duration.Random());
            m_ReactionCounter.Set(m_StartleConfig.reaction.Random());
        }
        
        public void End()
        {
        }


        public bool TickStateSwitch(BoidsActor _actor, float _deltaTime)
        {
            if (m_ReactionCounter.m_Counting)
                return false;
            
            m_StartleCounter.Tick(_deltaTime);
            return !m_StartleCounter.m_Counting;
        }
        
        public void TickVelocity(BoidsActor _actor, ABoidsBehaviour _behaviour, IEnumerable<ABoidsBehaviour> _flock, float _deltaTime,
            ref Vector3 _velocity)
        {
            if (m_ReactionCounter.m_Counting)
            {
                if(m_ReactionCounter.Tick(_deltaTime))
                    _actor.m_Animation.SetAnimation(m_StartleConfig.animName);

                return;
            }
            
            _velocity += _behaviour.TickFlocking(_flock, _deltaTime,m_FlockingConfig);
            _velocity += _behaviour.TickRandom(_deltaTime);
            _velocity += m_StartleDirection * (_deltaTime * m_StartleConfig.damping);
        }
        
        public void DrawGizmosSelected()
        {
            Gizmos_Extend.DrawString(Vector3.up*.2f,$"{m_ReactionCounter:F1}");
        }
    }

    public class Flying<T> : IBoidsState,IStateTransformVelocity,IStateSwitch<T> where T:Enum
    {
        public float speed => m_Config.speed;
        BoidsFlyingConfig m_Config;
        BoidsFlockingConfig m_FlockingConfig;
        BoidsEvadeConfig m_EvadeConfig;

        private readonly Counter m_TiringCounter = new Counter();
        private readonly Counter m_AnimationCounter = new Counter(1f);
        private readonly ValueChecker<bool> m_Gliding=new ValueChecker<bool>();
        
        public T m_NextBehaviour { get; private set; }
        public Flying<T> Init(BoidsFlyingConfig _config,BoidsFlockingConfig _flocking,BoidsEvadeConfig _evade,T _nextBehaviour)
        {
            m_Config = _config;
            m_FlockingConfig = _flocking;
            m_EvadeConfig = _evade;
            m_NextBehaviour = _nextBehaviour;
            return this;
        }
        
        public void Begin(BoidsActor _actor)
        {
            m_TiringCounter.Set(m_Config.tiringDuration.Random());
            m_Gliding.Check(true);
            _actor.m_Animation.SetAnimation(m_Config.flyAnim);
            m_AnimationCounter.Replay();
        }

        public void End()
        {
        }

        public bool TickStateSwitch(BoidsActor _actor, float _deltaTime)
        {
            m_TiringCounter.Tick(_deltaTime);
            return !m_TiringCounter.m_Counting;
        }

        public void TickVelocity(BoidsActor _actor, ABoidsBehaviour _behaviour, IEnumerable<ABoidsBehaviour> _flock, float _deltaTime,
            ref Vector3 _velocity)
        {
            Vector3 originOffset = _actor.m_Target.m_Destination - _behaviour.Position;
            float originDistance = originOffset.sqrMagnitude;

            if (originDistance > m_Config.sqrBorder)
                _velocity += originOffset.normalized * (_deltaTime * m_Config.borderDamping);

            float height = originOffset.y;
            _velocity += Vector3.up * (Mathf.Sign(height - m_Config.maintainHeight) * _deltaTime * m_Config.heightDamping);

            _velocity += _behaviour.TickFlocking(_flock,_deltaTime,m_FlockingConfig);
            _velocity += _behaviour.TickEvading(_deltaTime,m_EvadeConfig);
            
            m_AnimationCounter.Tick(_deltaTime);
            if (m_AnimationCounter.m_Counting)
                return;
            
            if (m_Gliding.Check(_velocity.y > 0))
            {
                _actor.m_Animation.SetAnimation(m_Gliding?m_Config.glideAnim:m_Config.flyAnim);
                m_AnimationCounter.Replay();
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
            Vector3 offset =  _actor.m_Target.m_Destination - _behaviour.Position;
            if (offset.sqrMagnitude > m_Config.sqrRadius) _velocity+= offset * (_deltaTime * m_Config.damping);
            
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

        private readonly Counter m_HoverCounter=new Counter();
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
            m_HoverCounter.Set(m_Config.duration.Random());
        }

        public void End()
        {
        }
        public bool TickStateSwitch(BoidsActor _actor, float _deltaTime)
        {
            m_HoverCounter.Tick(_deltaTime);
            return !m_HoverCounter.m_Counting;
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

        public void TickTransform(BoidsActor _actor, ABoidsBehaviour _behaviour, IEnumerable<ABoidsBehaviour> _flock, float _deltaTime,
            ref Vector3 _position, ref Quaternion _rotation)
        {
            float sqrDistance = speed*speed;
            Vector3 direction =  _actor.m_Target.m_Destination - _actor.m_Behaviour.Position;
            float scale = direction.sqrMagnitude / sqrDistance;
            _rotation = Quaternion.Lerp(Quaternion.LookRotation(direction,Vector3.up),  Quaternion.LookRotation(Vector3.up,-direction),scale*.75f);
        }

        public void DrawGizmosSelected()
        {
        }

    }


    public class Perching : IBoidsState,IStateTransformSetter
    {
        enum EPerchingState
        {
            Move,
            Alert,
            Idle,
            Relax,
        }

        private EPerchingState m_State;
        private BoidsPerchConfig m_Config;
        private BoidsFlockingConfig m_FlockingConfig;
        private Vector3 m_RandomDirection;
        
        private readonly Counter m_StateCounter = new Counter();
        private readonly Counter m_RotateCounter = new Counter();
        private readonly Counter m_RotateCooldown = new Counter();
        private float rotateSpeed;
        public Perching Init(BoidsPerchConfig _config,BoidsFlockingConfig _flockingConfig)
        {
            m_Config = _config;
            m_FlockingConfig = _flockingConfig;
            return this;
        }
        
        public void Begin(BoidsActor _actor)
        {
            SetState(_actor,EPerchingState.Move);
        }
        
        public void End()
        {
            
        }

        void SetState(BoidsActor _actor, EPerchingState _state)
        {
            m_State = _state;
            ResetRotation();
            switch (_state)
            {
                case EPerchingState.Move:
                {
                    m_RandomDirection = URandom.RandomVector3().SetY(0f).normalized;
                    _actor.m_Animation.SetAnimation(m_Config.moveAnim);
                    m_StateCounter.Set(m_Config.moveDuration.Random());
                }
                    break;
                case EPerchingState.Alert:
                {
                    _actor.m_Animation.SetAnimation(m_Config.alertAnim);
                    m_StateCounter.Set(m_Config.alertDuration.Random());
                }
                    break;
                case EPerchingState.Idle:
                {
                    _actor.m_Animation.SetAnimation(m_Config.idleAnim);
                    m_StateCounter.Set(m_Config.idleDuration.Random());
                }
                    break;
                case EPerchingState.Relax:
                {
                    _actor.m_Animation.SetAnimation(m_Config.relaxAnim);
                    m_StateCounter.Set(m_Config.relaxDuration.Random());
                }
                    break;
            }
        }
        
        public void TickTransform(BoidsActor _actor, ABoidsBehaviour _behaviour, IEnumerable<ABoidsBehaviour> _flock, float _deltaTime,
            ref Vector3 _position, ref Quaternion _rotation)
        {
            m_StateCounter.Tick(_deltaTime);
            switch (m_State)
            {
                case EPerchingState.Move:
                {
                    Vector3 flocking = _behaviour.TickFlocking(_flock,_deltaTime,m_FlockingConfig).SetY(0f);
                    if(flocking.sqrMagnitude < 0.001f)
                        flocking = m_RandomDirection * (m_Config.moveSpeed * _deltaTime);
                    _position += flocking;
                    _rotation = Quaternion.Lerp(_rotation, Quaternion.LookRotation(flocking,Vector3.up),_deltaTime*5f);

                    if(!m_StateCounter.m_Counting)
                        SetState(_actor,EPerchingState.Alert);
                }
                    break;
                case EPerchingState.Alert:
                {
                    TickRotate(_deltaTime,ref _rotation);
                    if (m_StateCounter.m_Counting)
                        return;
                    
                    if ( _behaviour.TickFlocking(_flock,_deltaTime,m_FlockingConfig).sqrMagnitude > float.Epsilon)
                    {
                        SetState(_actor,EPerchingState.Move);
                        return;
                    }
                    SetState(_actor,EPerchingState.Idle);
                }
                    break;
                case EPerchingState.Idle:
                {
                    TickRotate(_deltaTime,ref _rotation);
                    if (m_StateCounter.m_Counting)
                        return;
                    SetState(_actor,EPerchingState.Relax);
                }
                    break;
                case EPerchingState.Relax:
                {
                    if (m_StateCounter.m_Counting)
                        return;

                    if (URandom.Random01() <= m_Config.relaxedMovingPossibility)
                    {
                        SetState(_actor,EPerchingState.Move);
                        return;
                    }
                    
                    SetState(_actor,EPerchingState.Alert);
                }
                    break;
            }
        }

        void ResetRotation()
        {
            m_RotateCounter.Set(m_Config.rotateDuration.Random());
            m_RotateCooldown.Set(m_Config.rotateCooldown.Random());
            rotateSpeed = URandom.RandomSign()* m_Config.rotateSpeed.Random();
        }
        void TickRotate(float _deltaTime,ref Quaternion _rotation)
        {
            m_RotateCooldown.Tick(_deltaTime);
            if (m_RotateCooldown.m_Counting)
                return;

            _rotation *= Quaternion.Euler(0f,rotateSpeed*_deltaTime,0f);
            if (!m_RotateCounter.Tick(_deltaTime))
                return;
            ResetRotation();
        }


        public void DrawGizmosSelected()
        {
            Gizmos_Extend.DrawString(Vector3.up*.2f,$"{m_State} {m_StateCounter.m_TimeLeft:F1}");
        }
    }

    public class Idle : IBoidsState,IStateTransformSetter
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


        public void DrawGizmosSelected()
        {
        }

        public void TickTransform(BoidsActor _actor, ABoidsBehaviour _behaviour, IEnumerable<ABoidsBehaviour> _flock, float _deltaTime,
            ref Vector3 _position, ref Quaternion _rotation)
        {
        }
    }
}