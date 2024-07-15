using System;
using System.Collections.Generic;
using System.Linq.Extensions;
using TechToys.ThePlanet;
using TPool;
using UnityEngine;

namespace TechToys.ThePlanet.Module.BOIDS
{
    public abstract class BoidsFlock<TBoidsBehaviour,TBoidsTarget>:ITransform where TBoidsBehaviour:ABoidsBehaviour where TBoidsTarget:ABoidsTarget
    {
        public Transform transform { get; }
        public BoidsActor this[int _index] => m_Actors[_index];
        private readonly ObjectPoolClass<int, BoidsActor> m_Actors;
        public BoidsFlock(Transform _transform)
        {
            transform = _transform;
            m_Actors = new ObjectPoolClass<int, BoidsActor>(_transform.Find("Actor"),GetParameters);
        }

        protected abstract TBoidsTarget GetTarget();
        protected abstract TBoidsBehaviour GetController();
        protected abstract IBoidsAnimation GetAnimation();
        object[] GetParameters()
        {
            var target = GetTarget();
            var controller = GetController();
            var animation = GetAnimation();
            return new object[]{ controller,target,animation};
        }

        protected BoidsActor SpawnActor()
        {
            var actor = m_Actors.Spawn();
            return actor;
        }

        protected bool RecycleActor(int identity) => m_Actors.Recycle(identity)!=null;
        public virtual void Dispose()
        {
            m_Actors.Clear();
        }
        public virtual void Tick(float _deltaTime)
        {
            foreach (var actor in m_Actors)
                actor.Tick(_deltaTime,m_Actors.m_Dic);
        }
        

#if UNITY_EDITOR
        public virtual void DrawGizmos(bool _drawRelativePoints)
        {
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = Color.white;
            m_Actors?.Collect(p=>UnityEditor.Selection.activeObject==p.transform.gameObject).Traversal(p=>p.DrawGizmosSelected());
        }
#endif
    }
}