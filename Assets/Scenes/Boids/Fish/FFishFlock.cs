using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Boids.Behaviours;
using TPoolStatic;
using UnityEngine;

namespace Boids.Fish
{
    public class FFishFlock : ABoidsFlock
    {
        public FFishConfig m_Config;
        protected override ABoidsBehaviour GetController() => new FFishBehaviour(m_Config);

        protected override ABoidsTarget GetTarget() => new FBoidsTargetEmpty();
        protected override IBoidsAnimation GetAnimation() => new BoidsEmptyAnimation();

        public void Spawn()
        {
            SpawnActor(transform.localToWorldMatrix);
        }
    }

    public enum EFishBehaviour
    {
        Floating,
    }
    
    public class FFishBehaviour:BoidsBehaviour<EFishBehaviour>
    {
        private readonly FFishConfig m_Config;
        
        public FFishBehaviour(FFishConfig _config)
        {
            m_Config = _config;
        }

        public override void Spawn(BoidsActor _actor, Matrix4x4 _landing)
        {
            base.Spawn(_actor, _landing);
            SetBehaviour(EFishBehaviour.Floating);
        }

        protected override IBoidsState SpawnBehaviour(EFishBehaviour _behaviourType)
        {
            switch (_behaviourType)
            {
                default: throw new InvalidEnumArgumentException();
                case EFishBehaviour.Floating: return TSPool<Hovering>.Spawn().Init(m_Config.hovering,m_Config.flocking);
            }
        }

        protected override void RecycleBehaviour(EFishBehaviour _behaviourType, IBoidsState _state)
        {
            switch (_behaviourType)
            {
                default: throw new InvalidEnumArgumentException();
                case EFishBehaviour.Floating: TSPool<Hovering>.Recycle(_state as Hovering); break;
            }
        }
    }
}