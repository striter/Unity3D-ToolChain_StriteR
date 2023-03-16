using System;
using System.ComponentModel;
using TechToys.ThePlanet.Module.BOIDS.States.Bird;
using TPoolStatic;
namespace TechToys.ThePlanet.Module.BOIDS.Bird
{
    public class FBirdBehaviour : BoidsBehaviour<EBirdBehaviour>
    {
        private readonly FBirdConfig m_Config;
        private Action<int> DoPoopAction;
        public FBirdBehaviour(FBirdConfig _config, Action<int> _poopAction)
        {
            m_Config = _config;
            DoPoopAction = _poopAction;
        }

        public void Initialize(EBirdBehaviour _behaviour)
        {
            SetBehaviour(_behaviour);
        }


        public void Startle() => SetBehaviour(EBirdBehaviour.Startling);
        protected override IBoidsState CreateBehaviour(EBirdBehaviour _behaviourType)
        {
            switch (_behaviourType)
            {
                default: throw new InvalidEnumArgumentException();
                case EBirdBehaviour.Startling: return new Startle<EBirdBehaviour>(m_Config.startleConfig, m_Config.flockingConfig, m_Config.m_FollowingConfig, EBirdBehaviour.Flying);
                case EBirdBehaviour.Flying: return new Flying<EBirdBehaviour>(m_Config.flyingConfig, m_Config.flockingConfig, m_Config.m_FollowingConfig, EBirdBehaviour.PreLanding);
                case EBirdBehaviour.PreLanding: return new PreLanding<EBirdBehaviour>(m_Config.hoveringConfig, m_Config.flockingConfig, EBirdBehaviour.Landing);
                case EBirdBehaviour.Landing: return new HoverLanding<EBirdBehaviour>(m_Config.landConfig, EBirdBehaviour.Perching);
                case EBirdBehaviour.Perching: return new Perching(m_Config.perchConfig, DoPoopAction);
                case EBirdBehaviour.Traveling: return new Flying<EBirdBehaviour>(m_Config.flyingConfig, m_Config.flockingConfig, m_Config.m_FollowingConfig,EBirdBehaviour.Invalid);
            }
        }
    }
}