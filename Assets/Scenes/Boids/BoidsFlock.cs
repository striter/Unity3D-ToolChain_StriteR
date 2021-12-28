using System;
using System.Collections.Generic;
using TPool;
using UnityEngine;

namespace Boids
{
    public abstract class ABoidsFlock:MonoBehaviour
    {
        protected TObjectPoolClass<int, BoidsActor> m_Actors { get; private set; }
        protected abstract ABoidsBehaviour GetController();
        protected abstract ABoidsTarget GetTarget();
        protected abstract IBoidsAnimation GetAnimation();
        public virtual void Init()
        {
            m_Actors = new TObjectPoolClass<int, BoidsActor>(transform.Find("Actor"),GetParameters);
        }
        object[] GetParameters()=>
            new object[] { GetController(), GetTarget(),GetAnimation() };
        protected void SpawnActor(Matrix4x4 _landing)
        {
            m_Actors.Spawn().Spawn(_landing);
        }

        public void Clear()
        {
            m_Actors.Clear();
        }

        public void Tick(float _deltaTime)
        {
            m_Actors.Traversal(p=>p.Tick(_deltaTime,m_Actors));
        }
        

#if UNITY_EDITOR
        protected virtual void OnDrawGizmos()
        {  
            m_Actors?.Collect(p=>UnityEditor.Selection.activeObject==p.Transform.gameObject).Traversal(p=>p.DrawGizmosSelected());
        }
#endif
    }
    public abstract class BoidsFlock<T>:ABoidsFlock where T:ABoidsBehaviour
    {
        protected IEnumerable<T> Actors
        {
            get
            {
                foreach (var actor in m_Actors)
                    yield return actor.m_Behaviour as T;
            }
        }
    }
}