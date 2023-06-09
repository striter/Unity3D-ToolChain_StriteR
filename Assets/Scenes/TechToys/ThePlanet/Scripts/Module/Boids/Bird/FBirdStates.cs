using System;
using System.Collections.Generic;
using TechToys.ThePlanet.Module.BOIDS.Bird;
using UnityEngine;

namespace TechToys.ThePlanet.Module.BOIDS.States.Bird
{
    public class Startle<T> : States.Startle<T> where T:Enum
    {
        BoidsFollowingConfig m_FollowingConfig;
        public Startle(BoidsStartleConfig _config,BoidsFlockingConfig _flockingConfig,BoidsFollowingConfig _followingConfig,T _chillBehaviour):base(_config,_flockingConfig,_chillBehaviour)
        {
            m_FollowingConfig = _followingConfig;
        }

        public override void TickVelocity(BoidsActor _actor, IList<BoidsActor> _members, float _deltaTime, ref Vector3 _velocity)
        {
            base.TickVelocity(_actor, _members, _deltaTime, ref _velocity);

            var target = (_actor.m_Target as FBirdTarget);
            if(!target.m_IsLeader)
                _velocity += this.TickFollowing(_actor,target.m_Leader,_deltaTime,m_FollowingConfig);
        }
    }

    public class Flying<T> : IBoidsState,IStateTransformVelocity,IStateSwitch<T> where T:Enum
    {
        public float speed => m_Config.speed;
        BoidsFlyingConfig m_Config;
        BoidsFlockingConfig m_FlockingConfig;
        BoidsFollowingConfig followingConfig;

        private readonly Counter m_TiringCounter = new Counter();
        private readonly Counter m_AnimationCounter = new Counter(1f);
        private readonly ValueChecker<bool> m_Gliding=new ValueChecker<bool>();
        private T m_TiredBehaviour;
        
        public Flying(BoidsFlyingConfig _config,BoidsFlockingConfig _flocking,BoidsFollowingConfig _following,T _tiredBehaviour)
        {
            m_Config = _config;
            m_FlockingConfig = _flocking;
            followingConfig = _following;
            m_TiredBehaviour = _tiredBehaviour;
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
            if (m_TiredBehaviour.Equals(UEnum.GetInvalid<T>())) 
             return false;
            
            m_TiringCounter.Tick(_deltaTime);
            if (m_TiringCounter.m_Playing)
                return false;

            if (!_actor.m_Target.PickAvailablePerching())
            {
                m_TiringCounter.Replay();
                return false;
            }
            
            return true;
        }

        public void TickVelocity(BoidsActor _actor, IList<BoidsActor> _members, float _deltaTime,
            ref Vector3 _velocity)
        {
            Vector3 up = _actor.Position.normalized;
            _velocity += this.TickMaintainHeight(_actor,m_Config.origin+up*m_Config.height-_actor.Position,up,_deltaTime,m_Config.heightDamping,out var upward);
            _velocity += this.TickFlocking(_actor,_members,_deltaTime,m_FlockingConfig);
            var target = (_actor.m_Target as FBirdTarget);
            if(!target.m_IsLeader)
                _velocity += this.TickFollowing(_actor,target.m_Leader,_deltaTime,followingConfig);
            
            m_AnimationCounter.Tick(_deltaTime);
            if (!m_AnimationCounter.m_Playing)
            {
                if (m_Gliding.Check(upward<0f))
                {
                    _actor.m_Animation.SetAnimation(m_Gliding?m_Config.glideAnim:m_Config.flyAnim);
                    m_AnimationCounter.Replay();
                }
            }
        }
        
#if UNITY_EDITOR
        public void DrawGizmosSelected(BoidsActor _actor)
        {
            UGizmos.DrawString(Vector3.up*.2f,$"{m_TiringCounter.m_TimeLeft:F1}");
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = Color.white;
            Gizmos.DrawLine(_actor.Position,m_Config.origin);
            Gizmos.color = Color.red;
            // Gizmos.DrawWireSphere(_actor.m_Target.m_Destination,m_Config.boderRange);
            Gizmos.DrawLine(_actor.Position,_actor.Position.normalized*(m_Config.height));
        }
#endif
    }

    public class PreLanding<T> : IBoidsState, IStateTransformVelocity, IStateSwitch<T> where T:Enum
    {
        public float speed => m_Config.speed;
        private T m_NextBehaviour;
        private BoidsPreLandingConfig m_Config;
        private BoidsFlockingConfig m_FlockConfig;
        private float m_Clockwise;
        private Vector3 hoverPosition;
        private Vector3 hoverTangent;
        private Vector3 hoverOffset;
        public PreLanding(BoidsPreLandingConfig _hoverConfig,BoidsFlockingConfig _flockConfig,T _nextBehaviour)
        {
            m_Config = _hoverConfig;
            m_FlockConfig = _flockConfig;
            m_NextBehaviour = _nextBehaviour;
        }

        public void Begin(BoidsActor _actor)
        {
            TickHoverParameters(_actor);
            _actor.m_Animation.SetAnimation(m_Config.anim);
            Vector3 centerOffset = _actor.Position - _actor.m_Target.m_Destination;
            m_Clockwise = Mathf.Sign(Vector3.Dot( Vector3.Cross(_actor.m_Target.m_Up,centerOffset),_actor.Forward));
        }

        public void End()
        {
        }

        void TickHoverParameters(BoidsActor _actor)
        {
            Vector3 destination = _actor.m_Target.m_Destination + _actor.m_Target.m_Up * m_Config.height;
            Vector3 centerOffset = _actor.Position - destination;
            Vector3 hoverDirection = centerOffset.SetY(0f).normalized;
            hoverTangent = Vector3.Cross(_actor.m_Target.m_Up,hoverDirection)*m_Clockwise;
            hoverPosition = destination + hoverDirection * m_Config.distance;
            hoverOffset = hoverPosition - _actor.Position;
        }
        
        public bool TickStateSwitch(BoidsActor _actor, float _deltaTime,out T _nextBehaviour)
        {
            _nextBehaviour = m_NextBehaviour;
            return hoverOffset.sqrMagnitude < 1f;
        }
        
        public void TickVelocity(BoidsActor _actor, IList<BoidsActor> _members, float _deltaTime,
            ref Vector3 _velocity)
        {
            TickHoverParameters(_actor);
    
            Vector3 direction = (hoverOffset + hoverTangent)/2f;
            
            _velocity += direction.normalized * (_deltaTime * m_Config.damping);
            
            _velocity += this.TickFlocking(_actor,_members,_deltaTime,m_FlockConfig);
        }
        
#if UNITY_EDITOR
        public virtual void DrawGizmosSelected(BoidsActor _actor)
        {
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(_actor.Position,hoverPosition);
            Gizmos.DrawWireSphere(_actor.Position,.2f);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(hoverPosition,hoverPosition+hoverTangent*.2f);
            UGizmos.DrawString( _actor.Position+Vector3.up*.2f,$"CW:{m_Clockwise}");
        }
#endif
    }

    public class HoverLanding<T> : IBoidsState, IStateTransformApply,IStateTransformVelocity, IStateSwitch<T>
    {
        private BoidsLandingConfig m_Config;
        private T m_NextBehaviour;
        public float speed => Mathf.Lerp(m_Config.speed * .5f, m_Config.speed, m_Interpolation);
        private float m_Interpolation;
        private float m_Distance;

        private readonly Counter m_VelocityCounter = new Counter(3f);
        private Vector3 m_StartVelocity;

        public HoverLanding(BoidsLandingConfig _config,T _nextBehaviour)
        {
            m_Config = _config;
            m_NextBehaviour = _nextBehaviour;
        }
        public void Begin(BoidsActor _actor)
        {
            _actor.m_Animation.SetAnimation(m_Config.anim);
            m_Distance = (_actor.Position - _actor.m_Target.m_Destination).magnitude;
            m_Interpolation = 1f;
            m_StartVelocity = _actor.Forward;
            m_VelocityCounter.Replay();
        }

        public void End()
        {
        }


        public void TickVelocity(BoidsActor _actor, IList<BoidsActor> _members, float _deltaTime,
            ref Vector3 _velocity)
        {
            m_VelocityCounter.Tick(_deltaTime);
            _velocity = Vector3.Lerp(m_StartVelocity,
                (_actor.m_Target.m_Destination - _actor.Position).normalized,
                m_VelocityCounter.m_TimeElapsedScale);
        }

        public bool TickStateSwitch(BoidsActor _actor, float _deltaTime, out T _nextState)
        {
            m_Interpolation = (_actor.Position - _actor.m_Target.m_Destination).magnitude / m_Distance;
            _nextState = m_NextBehaviour;
            return m_Interpolation<=0.025f;
        }
        
        public void TickTransform(BoidsActor _actor, IList<BoidsActor> _flock, float _deltaTime,
            ref Vector3 _position, ref Quaternion _rotation)
        {
            Vector3 direction = _actor.m_Target.m_Destination-_actor.Position;
            var rotation= Quaternion.Lerp(Quaternion.LookRotation(_actor.m_Target.m_Up,-m_StartVelocity),Quaternion.LookRotation(direction),m_VelocityCounter.m_TimeElapsedScale);
            _rotation = Quaternion.Lerp(_rotation,rotation,_deltaTime*20f);
        }


#if UNITY_EDITOR
        public void DrawGizmosSelected(BoidsActor _actor)
        {
            UGizmos.DrawString(Vector3.up*.2f,$"{m_Interpolation:F1} ${m_VelocityCounter.m_TimeElapsedScale:F1}");
        }
#endif
    }
    
    public class Perching : IBoidsState,IStateTransformApply,IStateSwitch<EBirdBehaviour>
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
        
        private readonly Counter m_StateCounter = new Counter();
        private readonly Counter m_RotateCounter = new Counter();
        private readonly Counter m_RotateCooldown = new Counter();
        private float m_Rotation,m_RotateSpeed=0f;
        private Vector3 m_Origin=Vector3.zero;
        private Vector3 m_MoveDirection;

        private Action<int> DoPoop;
        public Perching(BoidsPerchConfig _config,Action<int> _poop)
        {
            m_Config = _config;
            DoPoop = _poop;
        }
        
        public void Begin(BoidsActor _actor)
        {
            SetState(_actor,EPerchingState.Alert);
            m_Rotation = URandom.Random01() * 360f;
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
                    m_Origin = _actor.Position;
                    _actor.m_Target.PickAnotherSpot();
                    _actor.m_Animation.SetAnimation(m_Config.moveAnim);
                    m_StateCounter.Set(m_Config.moveDuration.Random());
                    m_MoveDirection =  _actor.m_Target.m_Destination - m_Origin;
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
        
        public void TickTransform(BoidsActor _actor, IList<BoidsActor> _members, float _deltaTime,
            ref Vector3 _position, ref Quaternion _rotation)
        {
            m_StateCounter.Tick(_deltaTime);
            switch (m_State)
            {
                case EPerchingState.Move:
                {
                    _position = Vector3.Lerp(m_Origin, _actor.m_Target.m_Destination, m_StateCounter.m_TimeElapsedScale);
                    _rotation = Quaternion.Lerp(_rotation,   Quaternion.LookRotation(m_MoveDirection,_actor.m_Target.m_Up),m_StateCounter.m_TimeElapsedScale);
                }
                    break;
                case EPerchingState.Alert:
                case EPerchingState.Idle:
                {
                    TickRotate(_actor,_deltaTime,ref _rotation);
                }
                    break;
                case EPerchingState.Relax:
                {
                    // Do Nothing
                }
                    break;
            }

            _rotation = _actor.m_Target.m_Rotation * Quaternion.Euler(0f,m_Rotation,0f);
        }
        public bool TickStateSwitch(BoidsActor _actor, float _deltaTime, out EBirdBehaviour _nextState)
        {
            _nextState = default;
            if (m_StateCounter.m_Playing)
                return false;
            
            switch (m_State)
            {
                case EPerchingState.Move:
                {
                    SetState(_actor,EPerchingState.Alert);
                }
                    break;
                case EPerchingState.Alert:
                {
                    var actionPossibility = URandom.Random01();
                    if (m_Config.alertMovePossibility > actionPossibility)
                    {
                        DoPoop(_actor.m_Identity);
                        SetState(_actor,EPerchingState.Move);
                        return false;
                    }
                    
                    SetState(_actor,EPerchingState.Idle);
                }
                    break;
                case EPerchingState.Idle:
                {
                    SetState(_actor,EPerchingState.Relax);
                }
                    break;
                case EPerchingState.Relax:
                {
                    SetState(_actor,EPerchingState.Alert);
                }
                    break;
            }
            return false;
        }
        
        void ResetRotation()
        {
            m_RotateCounter.Set(m_Config.rotateDuration.Random());
            m_RotateCooldown.Set(m_Config.rotateCooldown.Random());
            m_RotateSpeed = URandom.RandomSign()* m_Config.rotateSpeed.Random();
            
        }
        void TickRotate(BoidsActor _actor,float _deltaTime,ref Quaternion _rotation)
        {
            m_RotateCooldown.Tick(_deltaTime);
            if (m_RotateCooldown.m_Playing)
                return;

            m_Rotation += m_RotateSpeed * _deltaTime;
            if (!m_RotateCounter.TickTrigger(_deltaTime))
                return;
            ResetRotation();
        }


#if UNITY_EDITOR
        public void DrawGizmosSelected(BoidsActor _actor)
        {
            UGizmos.DrawString(Vector3.up*.2f,$"{m_State} {m_StateCounter.m_TimeLeft:F1}");
        }
#endif
    }
}