using System.ComponentModel;
using TechToys.ThePlanet.Module.BOIDS.States;
using UnityEngine;

namespace TechToys.ThePlanet.Module.BOIDS.Fish
{
    public class FFishFlock : BoidsFlock<FFishBehaviour, FBoidsTargetEmpty>
    {
        private FFishConfig m_Config;

        public FFishFlock(FFishConfig _config, Transform _transform) : base(_transform)
        {
            m_Config = _config;
        }

        public override void Tick(float _deltaTime)
        {
            base.Tick(_deltaTime);
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        protected override FFishBehaviour GetController() => new FFishBehaviour(m_Config);
        protected override FBoidsTargetEmpty GetTarget() => new FBoidsTargetEmpty();
        protected override IBoidsAnimation GetAnimation() => new FBoidsAnimationEmpty();
    }

    public enum EFishBehaviour
    {
        Floating,
    }

    public class FFishBehaviour : BoidsBehaviour<EFishBehaviour>
    {
        private readonly FFishConfig m_Config;

        public FFishBehaviour(FFishConfig _config)
        {
            m_Config = _config;
        }

        public override void Initialize(BoidsActor _actor, FBoidsVertex _vertex)
        {
            base.Initialize(_actor, _vertex);
            SetBehaviour(EFishBehaviour.Floating);
        }


        protected override IBoidsState CreateBehaviour(EFishBehaviour _behaviourType)
        {
            switch (_behaviourType)
            {
                default: throw new InvalidEnumArgumentException();
                case EFishBehaviour.Floating: return new FloatFlocking(m_Config.floating, m_Config.flocking);
            }
        }
    }
}