using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TPoolStatic;
using UnityEngine;

namespace  Boids
{
    public enum EBirdBehaviour
    {
        Invalid=-1,
        Startling=0,
        Flying=1,
        TryLanding=2,
        Landing=3,
        Perching=5,
    }

    public class BirdBehaviourController : BoidsBehaviourController<BirdBehaviourBase>
    {
        public override void Init(BoidsActor _actor, Vector3 _position)
        {
            base.Init(_actor, _position);
            SetBehaviour(EBirdBehaviour.Perching);
        }

        public override void Startle() => SetBehaviour(EBirdBehaviour.Startling);

        public override void Recycle()
        {
            base.Recycle();
            if(m_Behaviour!=null)
                Recycle(m_Behaviour);
        }

        public void SetBehaviour(EBirdBehaviour _behaviour)
        {
            if(m_Behaviour!=null)
                Recycle(m_Behaviour);
            SetBehaviour(Spawn(this,_behaviour));
        }

        static BirdBehaviourBase Spawn(BirdBehaviourController _controller, EBirdBehaviour _behaviour)
        {
            BirdBehaviourBase behaviourBase;
            switch (_behaviour)
            {
                default: throw new InvalidEnumArgumentException();
                case EBirdBehaviour.Startling: behaviourBase= TSPool<BirdStartleTransform>.Spawn(); break;
                case EBirdBehaviour.Flying: behaviourBase= TSPool<BirdFlyingBehaviour>.Spawn(); break;
                case EBirdBehaviour.TryLanding: behaviourBase= TSPool<BirdTryLandingBehaviour>.Spawn(); break;
                case EBirdBehaviour.Landing: behaviourBase= TSPool<BirdLandingBehaviour>.Spawn(); break;
                case EBirdBehaviour.Perching: behaviourBase= TSPool<BirdPerchingBehaviour>.Spawn(); break;
            }

            behaviourBase.Init( _behaviour,_controller);
            return behaviourBase;
        }
        
        static void Recycle(BirdBehaviourBase behaviourBase)
        {
            switch (behaviourBase.m_Behaviour)
            {
                default: throw new InvalidEnumArgumentException();
                case EBirdBehaviour.Startling: TSPool<BirdStartleTransform>.Recycle(behaviourBase as BirdStartleTransform); break;
                case EBirdBehaviour.Flying: TSPool<BirdFlyingBehaviour>.Recycle(behaviourBase as BirdFlyingBehaviour); break;
                case EBirdBehaviour.TryLanding: TSPool<BirdTryLandingBehaviour>.Recycle(behaviourBase as BirdTryLandingBehaviour); break;
                case EBirdBehaviour.Landing: TSPool<BirdLandingBehaviour>.Recycle(behaviourBase as BirdLandingBehaviour); break;
                case EBirdBehaviour.Perching: TSPool<BirdPerchingBehaviour>.Recycle(behaviourBase as BirdPerchingBehaviour); break;
            }
        }
    }
    
    public abstract class BirdBehaviourBase:IBoidsBehaviour
    {
        public EBirdBehaviour m_Behaviour { get;private set; }
        public BirdBehaviourController m_Controller { get; private set; }
        public void Init(EBirdBehaviour _behaviour, BirdBehaviourController _controller)
        {
            m_Behaviour = _behaviour;
            m_Controller = _controller;
        }

        public virtual void Begin(BoidsActor _actor){}
        public virtual void Tick(BoidsActor _actor,float _deltaTime){}
        public virtual void End(){}

        public virtual void DrawGizmosSelected()
        {
            Gizmos_Extend.DrawString(Vector3.up*.2f,m_Behaviour.ToString());   
        }

    }
    
    public class BirdStartleTransform:BirdBehaviourBase,IBoidsTransformVelocity
    {
        public override void Begin(BoidsActor _actor)
        {
            _actor.m_Animation.SetAnimation("Bird_Flapping");
        }

        public void TickVelocity(BoidsActor _actor, IEnumerable<BoidsActor> _flock, float _deltaTime, ref Vector3 _velocity)
        {
            _velocity += this.TickFlocking(_actor,_flock, _deltaTime);
            _velocity += this.TickFlapping(_actor,_deltaTime);
            _velocity += this.TickRandom(_actor,_deltaTime);
        }

        public override void Tick(BoidsActor _actor, float _deltaTime)
        {
            base.Tick(_actor,_deltaTime);
            Vector3 offset = _actor.m_Target.m_Destination - _actor.m_Behaviour.Position;
            if (offset.sqrMagnitude > _actor.m_Config.speed * _actor.m_Config.speed)
                m_Controller.SetBehaviour(EBirdBehaviour.Flying);
        }
    }

    public class BirdFlyingBehaviour : BirdBehaviourBase,IBoidsTransformVelocity
    {
        private readonly Timer m_TiringTimer = new Timer(10f);
        private readonly Timer m_AnimationTimer = new Timer(1f);
        private readonly ValueChecker<bool> m_Gliding=new ValueChecker<bool>();
        public override void Begin(BoidsActor _actor)
        {
            m_Gliding.Check(true);
            _actor.m_Animation.SetAnimation("Bird_Flying");
            m_TiringTimer.Replay();
            m_AnimationTimer.Replay();
        }

        public override void Tick(BoidsActor _actor, float _deltaTime)
        {
            base.Tick(_actor, _deltaTime);
            m_TiringTimer.Tick(_deltaTime);
            if (m_TiringTimer.m_Timing)
                return;
            m_Controller.SetBehaviour(EBirdBehaviour.TryLanding);
        }

        public void TickVelocity(BoidsActor _actor, IEnumerable<BoidsActor> _flock, float _deltaTime, ref Vector3 _velocity)
        {
            _velocity += this.TickFlocking(_actor,_flock,_deltaTime);
            _velocity += this.TickHovering(_actor,_deltaTime);
            _velocity += this.TickAvoid(_actor,_deltaTime);
            
            m_AnimationTimer.Tick(_deltaTime);
            if (m_AnimationTimer.m_Timing)
                return;
            
            if (m_Gliding.Check(_velocity.y > 0))
            {
                _actor.m_Animation.SetAnimation(m_Gliding?"Bird_Flying":"Bird_Gliding");
                m_AnimationTimer.Replay();
            }
        }
    }

    public class BirdTryLandingBehaviour : BirdBehaviourBase,IBoidsTransformVelocity
    {
        public override void Begin(BoidsActor _actor)
        {
            _actor.m_Animation.SetAnimation("Bird_Flapping");
        }

        public override void Tick(BoidsActor _actor, float _deltaTime)
        {
            base.Tick(_actor, _deltaTime);
            Vector3 offset = _actor.m_Target.m_Destination - _actor.m_Behaviour.Position;
            if (offset.sqrMagnitude < _actor.m_Config.speed*_actor.m_Config.speed)
                m_Controller.SetBehaviour(EBirdBehaviour.Landing);
        }

        public void TickVelocity(BoidsActor _actor, IEnumerable<BoidsActor> _flock, float _deltaTime, ref Vector3 _velocity)
        {
            _velocity += this.TickFlocking(_actor,_flock,_deltaTime);
            _velocity += this.TickAvoid(_actor,_deltaTime);
            _velocity += this.TickHoverTowards(_actor,_deltaTime);
        }
    }

    public class BirdLandingBehaviour : BirdBehaviourBase,IBoidsTransformSetter
    {
        private readonly Timer m_LandTimer = new Timer(3f);
        private Vector3 m_startPos;
        public override void Begin(BoidsActor _actor)
        {
            _actor.m_Animation.SetAnimation("Bird_Landing");
            m_startPos = _actor.m_Behaviour.Position;
            m_LandTimer.Replay();
        }

        public void TickTransform(BoidsActor _actor, float _deltaTime, ref Vector3 _position, ref Quaternion _rotation)
        {
            m_LandTimer.Tick(_deltaTime);
            if (!m_LandTimer.m_Timing)
            {
                m_Controller.SetBehaviour(EBirdBehaviour.Perching);
                return;
            }
            float scale = m_LandTimer.m_TimeElapsedScale;
            _position = Vector3.Lerp(m_startPos,_actor.m_Target.m_Destination,scale);
            Vector3 direction =  _actor.m_Target.m_Destination - _actor.m_Behaviour.Position;
            _rotation = Quaternion.Lerp(Quaternion.LookRotation(direction,Vector3.up),  Quaternion.LookRotation(Vector3.up,-direction),scale*.75f);
        }
    }


    public class BirdPerchingBehaviour : BirdBehaviourBase,IBoidsTransformSetter
    {
        private readonly Timer m_Stand = new Timer(5f);
        private readonly Timer m_HalfStand = new Timer(5f);
        public override void Begin(BoidsActor _actor)
        {
            m_Stand.Replay();
            m_HalfStand.Replay();
            _actor.m_Animation.SetAnimation("Bird_Stop_Standing");
            // _actor.m_Animation.SetAnimation("Bird_Flapping");
        }
        
        public void TickTransform(BoidsActor _actor, float _deltaTime, ref Vector3 _position, ref Quaternion _rotation)
        {
            _rotation = Quaternion.LookRotation(Vector3.forward,Vector3.up);

            if (TickStanding(_actor,_deltaTime))
                return;

            if (TickHalfStanding(_actor, _deltaTime))
                return;
        }
        
        bool TickStanding(BoidsActor _actor, float _deltaTime)
        {
            if (!m_Stand.m_Timing)
                return false;

            m_Stand.Tick(_deltaTime);
            if(!m_Stand.m_Timing)
                _actor.m_Animation.SetAnimation("Bird_Stop_HalfStanding");
            return true;
        }

        bool TickHalfStanding(BoidsActor _actor,float _deltaTime)
        {
            if (!m_HalfStand.m_Timing)
                return false;
            m_HalfStand.Tick(_deltaTime);
            if (!m_HalfStand.m_Timing)
                _actor.m_Animation.SetAnimation("Bird_Stop_Sitting");
            return true;
        }

    }
}