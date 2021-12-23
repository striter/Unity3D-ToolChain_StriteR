using System.Collections.Generic;
using System.ComponentModel;
using TPoolStatic;
using UnityEngine;

namespace Boids
{
    public enum EButterFlyBehaviour
    {
        Floating,
        Landing,
        Perching,
    }
    
    public class ButterflyBehaviourController:BoidsBehaviourController<ButterflyBehaviour>
    {
        public override void Init(BoidsActor _actor, Vector3 _position)
        {
            base.Init(_actor, _position);
            SetBehaviour(EButterFlyBehaviour.Floating);
        }

        public override void Startle()
        {
            
        }

        public void SetBehaviour(EButterFlyBehaviour _behaviour)
        {
            if(m_Behaviour!=null)
                Recycle(m_Behaviour);
            
            SetBehaviour(Spawn(this,_behaviour));
        }

        static ButterflyBehaviour Spawn(ButterflyBehaviourController _controller, EButterFlyBehaviour _behaviour)
        {
            ButterflyBehaviour behaviour;
            switch (_behaviour)
            {
                default: throw new InvalidEnumArgumentException();
                case EButterFlyBehaviour.Floating: behaviour = TSPool<ButterflyFloatingBehaviour>.Spawn(); break;
                case EButterFlyBehaviour.Landing: behaviour = TSPool<ButterFlyLandingBehaviour>.Spawn();break;
                case EButterFlyBehaviour.Perching: behaviour = TSPool<ButterFlyPerchingBehaviour>.Spawn();break;
            }

            behaviour.Init(_behaviour,_controller);
            return behaviour;
        }

        static void Recycle(ButterflyBehaviour _behaviour)
        {
            switch (_behaviour.m_Behaviour)
            {
                default: throw new InvalidEnumArgumentException();
                case EButterFlyBehaviour.Floating: TSPool<ButterflyFloatingBehaviour>.Recycle(_behaviour as ButterflyFloatingBehaviour); break;
                case EButterFlyBehaviour.Landing: TSPool<ButterFlyLandingBehaviour>.Recycle(_behaviour as ButterFlyLandingBehaviour);break;
                case EButterFlyBehaviour.Perching: TSPool<ButterFlyPerchingBehaviour>.Recycle(_behaviour as ButterFlyPerchingBehaviour);break;
            }
        }
    }

    public abstract class ButterflyBehaviour : IBoidsBehaviour
    {
        public EButterFlyBehaviour m_Behaviour { get; private set; }
        private ButterflyBehaviourController m_Controller { get; set; }
        public void Init(EButterFlyBehaviour _behaviour,ButterflyBehaviourController _controller)
        {
            m_Behaviour = _behaviour;
            m_Controller = _controller;
        }
        
        public virtual void Begin(BoidsActor _actor)
        {
        }

        public void Tick(BoidsActor _actor, float _deltaTime)
        {
            
        }

        public virtual void End()
        {
        }
        public virtual void DrawGizmosSelected() { 
            Gizmos_Extend.DrawString(Vector3.up*.2f,m_Behaviour.ToString());   
        }
    }

    public class ButterflyFloatingBehaviour : ButterflyBehaviour,IBoidsTransformVelocity
    {
        public override void Begin(BoidsActor _actor)
        {
            base.Begin(_actor);
            _actor.m_Animation.SetAnimation("Butterfly_Flap");
        }
        public void TickVelocity(BoidsActor _actor, IEnumerable<BoidsActor> _flock, float _deltaTime, ref Vector3 _velocity)
        {
            _velocity += this.TickFlocking(_actor,_flock,_deltaTime);
            _velocity += this.TickHovering(_actor,_deltaTime);
            _velocity += this.TickRandom(_actor, _deltaTime);
        }
    }

    public class ButterFlyLandingBehaviour : ButterflyBehaviour
    {
        public override void Begin(BoidsActor _actor)
        {
            base.Begin(_actor);
        }
    }

    public class ButterFlyPerchingBehaviour : ButterflyBehaviour
    {
        public override void Begin(BoidsActor _actor)
        {
            base.Begin(_actor);
            _actor.m_Animation.SetAnimation("Butterfly_Sit");
            // _actor.m_Animation.SetAnimation("Butterfly_SitFlap");
        }
    }
    
}