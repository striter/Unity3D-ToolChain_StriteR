using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Boids.Behaviours
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
        public Startle<T> Init(BoidsStartleConfig _config,BoidsFlockingConfig _flockingConfig,T _chillBehaviour)
        {
            m_StartleConfig = _config;
            m_FlockingConfig = _flockingConfig;
            m_ChillBehaviour = _chillBehaviour;
            return this;
        }
        
        public void Begin(BoidsActor _actor)
        {
            m_StartleDirection = URandom.RandomVector3();
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
        private T m_TiredBehaviour;
        
        public Flying<T> Init(BoidsFlyingConfig _config,BoidsFlockingConfig _flocking,BoidsEvadeConfig _evade,T _tiredBehaviour)
        {
            m_Config = _config;
            m_FlockingConfig = _flocking;
            m_EvadeConfig = _evade;
            m_TiredBehaviour = _tiredBehaviour;
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

        public bool TickStateSwitch(BoidsActor _actor, float _deltaTime,out T _tiredBehaviour)
        {
            _tiredBehaviour = m_TiredBehaviour;
            
            m_TiringCounter.Tick(_deltaTime);
            if (m_TiringCounter.m_Counting)
                return false;
            
            if (!_actor.m_Target.TryLanding())
                return false;
            
            return true;
        }

        public void TickVelocity(BoidsActor _actor, ABoidsBehaviour _behaviour, IEnumerable<ABoidsBehaviour> _flock, float _deltaTime,
            ref Vector3 _velocity)
        {
            Vector3 originOffset = _behaviour.Position - _actor.m_Target.m_Destination;
            float originDistance = originOffset.sqrMagnitude;

            if (originDistance > m_Config.sqrBorder)
                _velocity += -originOffset.normalized * (_deltaTime * m_Config.borderDamping);

            _velocity += Vector3.up * (Mathf.Sign(m_Config.maintainHeight - originOffset.y) * _deltaTime * m_Config.heightDamping);

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
            Gizmos_Extend.DrawString(Vector3.up*.2f,$"{m_TiringCounter.m_TimeLeft:F1}");
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
        private readonly Counter m_HoverCounter=new Counter();
        private T m_NextBehaviour;
        private float m_Clockwise;
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
            
            Vector3 centerOffset = _actor.m_Behaviour.Position - _actor.m_Target.m_Destination;
            Vector3 hoverDirection = new Vector3(centerOffset.x,0f, centerOffset.z).normalized;
            m_Clockwise = Mathf.Sign(Vector3.Dot(Vector3.Cross(Vector3.right,hoverDirection),centerOffset));
            m_HoverCounter.Set(m_Config.duration.Random());
        }

        public void End()
        {
        }
        public bool TickStateSwitch(BoidsActor _actor, float _deltaTime,out T _nextBehaviour)
        {
            m_HoverCounter.Tick(_deltaTime);
            _nextBehaviour = m_NextBehaviour;
            return !m_HoverCounter.m_Counting;
        }
        public void TickVelocity(BoidsActor _actor, ABoidsBehaviour _behaviour, IEnumerable<ABoidsBehaviour> _flock, float _deltaTime,
            ref Vector3 _velocity)
        {
            Vector3 _destination = _actor.m_Target.m_Destination;
            
            Vector3 centerOffset = _behaviour.Position - _destination;
            Vector3 hoverDirection = new Vector3(centerOffset.x,0f, centerOffset.z).normalized;
            Vector3 hoverTangent = Vector3.Cross(Vector3.up,hoverDirection)*m_Clockwise;
    
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
            Gizmos_Extend.DrawString(Vector3.up*.2f,$"Time:{m_HoverCounter.m_TimeLeft:F1}, CW:{m_Clockwise}");
        }

    }

    public class TryLanding<T> : IBoidsState, IStateTransformVelocity, IStateSwitch<T>
    {
        private BoidsTryLandingConfig m_Config;
        public float speed => m_Config.speed;
        private T m_NextBehaviour;
        private T m_PreBehaviour;

        public TryLanding<T> Init(BoidsTryLandingConfig _config,BoidsEvadeConfig _evade,T _nextBehaviour,T _preBehaviour)
        {
            m_Config = _config;
            m_NextBehaviour = _nextBehaviour;
            m_PreBehaviour = _preBehaviour;
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
            _velocity += (_actor.m_Target.m_Destination - _actor.m_Behaviour.Position).normalized * (_deltaTime * m_Config.damping);
        }
        public bool TickStateSwitch(BoidsActor _actor, float _deltaTime,out T _nextBehaviour)
        {
            _nextBehaviour = default;
            Vector3 offset = (_actor.m_Behaviour.Position - _actor.m_Target.m_Destination);
            if (offset.sqrMagnitude > m_Config.distanceBias)
                return false;
            
            if (Vector3.Dot(offset,_actor.m_Target.m_Up) < .3f)
            {
                _nextBehaviour = m_PreBehaviour;
                return true;
            }
            
            _nextBehaviour = m_NextBehaviour;
            return true;
        }
        public void DrawGizmosSelected()
        {
        }
    }

    public class HoverLanding<T> : IBoidsState, IStateTransformApply,IStateTransformVelocity, IStateSwitch<T>
    {
        private BoidsHoverLandingConfig m_Config;
        private T m_NextBehaviour;
        public float speed => Mathf.Lerp(m_Config.speed * .5f, m_Config.speed, interpolate);
        private float interpolate;
        private float distance;

        private Counter velocityCounter = new Counter(1f);
        private Vector3 startVelocity;

        public HoverLanding<T> Spawn(BoidsHoverLandingConfig _config,T _nextBehaviour)
        {
            m_Config = _config;
            m_NextBehaviour = _nextBehaviour;
            return this;
        }
        public void Begin(BoidsActor _actor)
        {
            _actor.m_Animation.SetAnimation(m_Config.anim);
            distance = (_actor.m_Behaviour.Position - _actor.m_Target.m_Destination).magnitude;
            interpolate = 1f;
            startVelocity = _actor.m_Behaviour.Velocity;
            velocityCounter.Replay();
        }

        public void End()
        {
        }


        public void TickVelocity(BoidsActor _actor, ABoidsBehaviour _behaviour, IEnumerable<ABoidsBehaviour> _flock, float _deltaTime,
            ref Vector3 _velocity)
        {
            velocityCounter.Tick(_deltaTime);
            _velocity = Vector3.Lerp(startVelocity,
                (_actor.m_Target.m_Destination - _actor.m_Behaviour.Position).normalized,
                velocityCounter.m_TimeElapsedScale);
        }

        public bool TickStateSwitch(BoidsActor _actor, float _deltaTime, out T _nextState)
        {
            interpolate = (_actor.m_Behaviour.Position - _actor.m_Target.m_Destination).magnitude /distance;
            _nextState = m_NextBehaviour;
            return interpolate<=0.01;
        }
        
        public void TickTransform(BoidsActor _actor, ABoidsBehaviour _behaviour, IEnumerable<ABoidsBehaviour> _flock, float _deltaTime,
            ref Vector3 _position, ref Quaternion _rotation)
        {
            Vector3 direction = _actor.m_Target.m_Destination - _actor.m_Behaviour.Position;
            _rotation = Quaternion.Lerp(Quaternion.LookRotation(_behaviour.Velocity),  Quaternion.LookRotation(_actor.m_Target.m_Up,direction),interpolate);
        }


        public void DrawGizmosSelected()
        {
            Gizmos_Extend.DrawString(Vector3.up*.2f,$"{interpolate:F1}");
        }

    }
    
    public class Landing<T> : IBoidsState,IStateTransformApply,IStateSwitch<T>
    {
        private BoidsLandingConfig m_Config;
        private T m_NextBehaviour;

        private Vector3 m_LandPosition;
        private Vector3 m_LandDirection;
        private readonly Counter m_LandCounter = new Counter();
        public Landing<T> Init(BoidsLandingConfig _config,T _nextBehaviour)
        {
            m_Config = _config;
            m_NextBehaviour = _nextBehaviour;
            return this;
        }
        public void Begin(BoidsActor _actor)
        {
            _actor.m_Animation.SetAnimation(m_Config.anim);
            m_LandCounter.Set(m_Config.duration.Random());
            
            m_LandDirection = (_actor.m_Target.m_Destination - _actor.m_Behaviour.Position);
            m_LandPosition = _actor.m_Behaviour.Position;
            m_LandDirection = m_LandDirection.normalized;
        }
        public void End()
        {
            
        }
        public void TickTransform(BoidsActor _actor, ABoidsBehaviour _behaviour, IEnumerable<ABoidsBehaviour> _flock, float _deltaTime,
            ref Vector3 _position, ref Quaternion _rotation)
        {
            m_LandCounter.Tick(_deltaTime);
            float scale=m_LandCounter.m_TimeElapsedScale;
            _position = Vector3.Lerp(m_LandPosition,_actor.m_Target.m_Destination,scale);
            _rotation = Quaternion.Lerp(Quaternion.LookRotation(m_LandDirection,_actor.m_Target.m_Up),  Quaternion.LookRotation(_actor.m_Target.m_Up,-m_LandDirection),scale*.5f);
        }
        public bool TickStateSwitch(BoidsActor _actor, float _deltaTime,out T _nextBehaviour)
        {
            _nextBehaviour=m_NextBehaviour;
            if (m_LandCounter.m_Counting)
                return false;
            return true;
        }
        public void DrawGizmosSelected()
        {
        }
    }


    public class Perching : IBoidsState,IStateTransformApply
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
        private Quaternion m_Rotation;
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
                    Vector2 randomDirection = URandom.RandomVector2();
                    m_RandomDirection = randomDirection.x*_actor.m_Target.m_Right+randomDirection.y*_actor.m_Target.m_Forward;
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
                    _rotation = Quaternion.Lerp(_rotation, Quaternion.LookRotation(flocking,_actor.m_Target.m_Up),_deltaTime*5f);

                    if(!m_StateCounter.m_Counting)
                        SetState(_actor,EPerchingState.Alert);
                }
                    break;
                case EPerchingState.Alert:
                {
                    TickRotate(_actor,_deltaTime,ref _rotation);
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
                    TickRotate(_actor,_deltaTime,ref _rotation);
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
        void TickRotate(BoidsActor _actor,float _deltaTime,ref Quaternion _rotation)
        {
            m_RotateCooldown.Tick(_deltaTime);
            if (m_RotateCooldown.m_Counting)
                return;

            _rotation = Quaternion.AngleAxis(rotateSpeed*_deltaTime,_actor.m_Target.m_Up)*_rotation;
            if (!m_RotateCounter.Tick(_deltaTime))
                return;
            ResetRotation();
        }


        public void DrawGizmosSelected()
        {
            Gizmos_Extend.DrawString(Vector3.up*.2f,$"{m_State} {m_StateCounter.m_TimeLeft:F1}");
        }
    }

    public class Idle : IBoidsState,IStateTransformApply
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