using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Geometry.Voxel;
using TPoolStatic;
using UnityEngine;

namespace Boids.Bird
{
    public class FBirdFlock : BoidsFlock<FBirdBehaviour>
    {
        public FBirdConfig fBirdConfig;
        protected override ABoidsBehaviour GetController() => new FBirdBehaviour(fBirdConfig);

        public void Spawn(Vector3 _screenPos)
        {
            var ray = Camera.main.ScreenPointToRay(_screenPos);
            float distance = UGeometryIntersect.RayPlaneDistance(KGeometry.kZeroPlane, ray);
            if (distance <= 0)
                return;
            Vector3 point = ray.GetPoint(distance);
            SpawnActor(point,Vector3.up);
        }
        public void Startle()
        {
            foreach (var actor in Actors)
                actor.Startle();
        }
    }
    
    public class FBirdBehaviour : BoidsBehaviour<EBirdBehaviour>
    {
        private FBirdConfig m_Config;
        public FBirdBehaviour(FBirdConfig _config)
        {
            m_Config = _config;
        }
        public override void Spawn(Vector3 _position, Quaternion rotation)
        {
            base.Spawn(_position, rotation);
            SetBehaviour(EBirdBehaviour.Perching);
        }
        public void Startle() => SetBehaviour(EBirdBehaviour.Startling);
        protected override IBoidsState SpawnBehaviour(EBirdBehaviour _behaviourType)
        {
            switch (_behaviourType)
            {
                default: throw new InvalidEnumArgumentException();
                case EBirdBehaviour.Startling:return  TSPool<Behaviours.Startle<EBirdBehaviour>>.Spawn().Init(m_Config.startleConfig,m_Config.flockingConfig,EBirdBehaviour.Flying);
                case EBirdBehaviour.Flying:return  TSPool<Behaviours.Flying<EBirdBehaviour>>.Spawn().Init(m_Config.flyingConfig,m_Config.flockingConfig,m_Config.evadeConfig,EBirdBehaviour.Hovering); 
                case EBirdBehaviour.Hovering:return TSPool<Behaviours.Hovering<EBirdBehaviour>>.Spawn().Init(m_Config.hoveringConfig,m_Config.flockingConfig,m_Config.evadeConfig,EBirdBehaviour.Landing);
                case EBirdBehaviour.Landing:return TSPool<Behaviours.Landing<EBirdBehaviour>>.Spawn().Init(m_Config.landConfig,EBirdBehaviour.Perching); 
                case EBirdBehaviour.Perching:return TSPool<Behaviours.Perching>.Spawn().Init(m_Config.perchConfig,m_Config.perchFlocking); 
            }
        }

        protected override void RecycleBehaviour(EBirdBehaviour _behaviourType, IBoidsState state)
        {
            switch (_behaviourType)
            {
                default: throw new InvalidEnumArgumentException();
                case EBirdBehaviour.Startling:  TSPool<Behaviours.Startle<EBirdBehaviour>>.Recycle(state as Behaviours.Startle<EBirdBehaviour>); break;
                case EBirdBehaviour.Flying: TSPool<Behaviours.Flying<EBirdBehaviour>>.Recycle(state as Behaviours.Flying<EBirdBehaviour>); break;
                case EBirdBehaviour.Hovering:  TSPool<Behaviours.Hovering<EBirdBehaviour>>.Recycle(state as Behaviours.Hovering<EBirdBehaviour>); break;
                case EBirdBehaviour.Landing: TSPool<Behaviours.Landing<EBirdBehaviour>>.Recycle(state as Behaviours.Landing<EBirdBehaviour>); break;
                case EBirdBehaviour.Perching: TSPool<Behaviours.Perching>.Recycle(state as Behaviours.Perching); break;
            }
        }
    }
}