using System;
using System.Collections.Generic;
using TPool;
using UnityEngine;

namespace Boids
{
    public abstract class ABoidsFlock:MonoBehaviour
    {
        public Mesh[] m_Meshes;
        public Material m_Material;
        protected TObjectPoolClass<int, BoidsActor> m_Actors { get; private set; }
        protected abstract ABoidsBehaviour GetController();
        public void Init()
        {
            m_Actors = new TObjectPoolClass<int, BoidsActor>(transform.Find("Actor"));
        }

        protected void SpawnActor(Vector3 position,Vector3 normal)
        {
            m_Actors.Spawn().Spawn( GetController(),position,m_Meshes,m_Material);
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
        private void OnDrawGizmos()
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