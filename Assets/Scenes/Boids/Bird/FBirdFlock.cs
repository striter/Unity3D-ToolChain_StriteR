using System;
using System.ComponentModel;
using System.Linq;
using TPoolStatic;
using UnityEngine;

namespace Boids.Bird
{
    public class FBirdFlock : BoidsFlock<FBirdBehaviour>
    {
        public FBirdConfig fBirdConfig;
        [NonSerialized]public Transform[] m_Landings;
        public Material m_Material;
        public Mesh[] m_Meshes;
        protected override ABoidsBehaviour GetController() => new FBirdBehaviour(fBirdConfig);
        protected override ABoidsTarget GetTarget() => new FBirdTarget(this);
        protected override IBoidsAnimation GetAnimation() => new BoidsMeshAnimation(m_Material,m_Meshes);

        public override void Init()
        {
            base.Init();
            m_Landings = transform.Find("Landings").GetSubChildren().ToArray();
        }

        public void Spawn()
        {
            var random = m_Landings.RandomItem();
            SpawnActor(random.localToWorldMatrix);
        }
        public void Startle()
        {
            foreach (var actor in Actors)
                actor.Startle();
        }
    }

    public class FBirdTarget : ABoidsTarget
    {
        private FBirdFlock m_Flock;
        public FBirdTarget(FBirdFlock _flock)
        {
            m_Flock = _flock;
        }

        public override bool TryLanding()
        {
            float sqrDistance = float.MaxValue;
            Transform nearestBirdLanding = null;
            foreach (var birdLanding in m_Flock.m_Landings)
            {
                float curSqrDistance = (m_Actor.Transform.position - birdLanding.position).sqrMagnitude;
                if ( curSqrDistance > sqrDistance)
                    continue;
                sqrDistance = curSqrDistance;
                nearestBirdLanding = birdLanding;
            }
            SetTarget(nearestBirdLanding.localToWorldMatrix);
            return base.TryLanding();
        }
    }
    
    public class FBirdBehaviour : BoidsBehaviour<EBirdBehaviour>
    {
        private readonly FBirdConfig m_Config;
        public FBirdBehaviour(FBirdConfig _config)
        {
            m_Config = _config;
        }

        public override void Spawn(BoidsActor _actor, Matrix4x4 _landing)
        {
            base.Spawn(_actor, _landing);
            SetBehaviour(EBirdBehaviour.Perching);
        }
        public void Startle() => SetBehaviour(EBirdBehaviour.Startling);
        protected override IBoidsState SpawnBehaviour(EBirdBehaviour _behaviourType)
        {
            switch (_behaviourType)
            {
                default: throw new InvalidEnumArgumentException();
                case EBirdBehaviour.Startling: return TSPool<Behaviours.Startle<EBirdBehaviour>>.Spawn().Init(m_Config.startleConfig,m_Config.flockingConfig,EBirdBehaviour.Flying);
                case EBirdBehaviour.Flying: return TSPool<Behaviours.Flying<EBirdBehaviour>>.Spawn().Init(m_Config.flyingConfig,m_Config.flockingConfig,m_Config.evadeConfig,EBirdBehaviour.Hovering); 
                case EBirdBehaviour.Hovering:return TSPool<Behaviours.Hovering<EBirdBehaviour>>.Spawn().Init(m_Config.hoveringConfig,m_Config.flockingConfig,m_Config.evadeConfig,EBirdBehaviour.Landing);
                case EBirdBehaviour.Landing:return TSPool<Behaviours.HoverLanding<EBirdBehaviour>>.Spawn().Spawn(m_Config.landConfig,EBirdBehaviour.Perching); 
                case EBirdBehaviour.Perching:return TSPool<Behaviours.Perching>.Spawn().Init(m_Config.perchConfig,m_Config.perchFlocking); 
            }
        }

        protected override void RecycleBehaviour(EBirdBehaviour _behaviourType, IBoidsState _state)
        {
            switch (_behaviourType)
            {
                default: throw new InvalidEnumArgumentException();
                case EBirdBehaviour.Startling:  TSPool<Behaviours.Startle<EBirdBehaviour>>.Recycle(_state as Behaviours.Startle<EBirdBehaviour>); break;
                case EBirdBehaviour.Flying: TSPool<Behaviours.Flying<EBirdBehaviour>>.Recycle(_state as Behaviours.Flying<EBirdBehaviour>); break;
                case EBirdBehaviour.Hovering:  TSPool<Behaviours.Hovering<EBirdBehaviour>>.Recycle(_state as Behaviours.Hovering<EBirdBehaviour>); break;
                case EBirdBehaviour.Landing: TSPool<Behaviours.Landing<EBirdBehaviour>>.Recycle(_state as Behaviours.Landing<EBirdBehaviour>); break;
                case EBirdBehaviour.Perching: TSPool<Behaviours.Perching>.Recycle(_state as Behaviours.Perching); break;
            }
        }
    }
}