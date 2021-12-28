using System.ComponentModel;
using TPoolStatic;
using UnityEngine;

namespace Boids.Butterfly
{
    public class FButterflyFlock : BoidsFlock<FButterflyBehaviour>
    {        
        public FButterflyConfig m_Config;
        protected override ABoidsBehaviour GetController() => new FButterflyBehaviour(m_Config);
        protected override ABoidsTarget GetTarget() => new FButterflyTarget();

        public int m_Count=5;
        public void Spawn()
        {
            for(int i=0;i<m_Count;i++)
                SpawnActor(transform.localToWorldMatrix);
        }
        public void Startle()
        {
            foreach (var actor in Actors)
                actor.Startle();
        }
        
        public void Recall()
        {
            foreach (var actor in Actors)
                actor.Recall();
        }
    }
    

    public class FButterflyTarget : ABoidsTarget
    {
        
    }
    public sealed class FButterflyBehaviour:BoidsBehaviour<EButterFlyBehaviour>
    {
        private readonly FButterflyConfig m_Config;
        public FButterflyBehaviour(FButterflyConfig _config)
        {
            m_Config = _config;
        }

        public override void Spawn(BoidsActor _actor, Matrix4x4 _landing)
        {
            base.Spawn(_actor, _landing);
            SetBehaviour(EButterFlyBehaviour.Stopped);
        }

        public void Startle()=> SetBehaviour(EButterFlyBehaviour.Startle);
        public void Recall() => SetBehaviour(EButterFlyBehaviour.Landing);
        protected override IBoidsState SpawnBehaviour(EButterFlyBehaviour _behaviourType)
        {
            switch (_behaviourType)
            {
                default: throw new InvalidEnumArgumentException();
                case EButterFlyBehaviour.Startle:  return TSPool<Behaviours.Startle<EButterFlyBehaviour>>.Spawn().Init(m_Config.startleConfig,m_Config.flockingConfig,EButterFlyBehaviour.Floating);
                case EButterFlyBehaviour.Floating: return TSPool<Behaviours.Floating>.Spawn().Init(m_Config.floatingConfig,m_Config.flockingConfig);
                case EButterFlyBehaviour.Landing:return TSPool<Behaviours.Landing<EButterFlyBehaviour>>.Spawn().Init(m_Config.landConfig,EButterFlyBehaviour.Stopped);
                case EButterFlyBehaviour.Stopped:return TSPool<Behaviours.Idle>.Spawn().Init(m_Config.idleConfig);
            }
        }

        protected override void RecycleBehaviour(EButterFlyBehaviour _behaviourType, IBoidsState state)
        {
            switch (_behaviourType)
            {
                default: throw new InvalidEnumArgumentException();
                case EButterFlyBehaviour.Startle: TSPool<Behaviours.Startle<EButterFlyBehaviour>>.Recycle(state as Behaviours.Startle<EButterFlyBehaviour>); break;
                case EButterFlyBehaviour.Floating: TSPool<Behaviours.Floating>.Recycle(state as Behaviours.Floating); break;
                case EButterFlyBehaviour.Landing: TSPool<Behaviours.Landing<EButterFlyBehaviour>>.Recycle(state as Behaviours.Landing<EButterFlyBehaviour>); break;
                case EButterFlyBehaviour.Stopped:TSPool<Behaviours.Idle>.Recycle(state as Behaviours.Idle); break;
            }
        }
    }
}