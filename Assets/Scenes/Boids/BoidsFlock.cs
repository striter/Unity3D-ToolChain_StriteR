using TPool;
using UnityEngine;

namespace Boids
{
    public class BoidsFlock : MonoBehaviour
    {
        public bool butterfly = false;
        public Mesh[] m_Meshes;
        public Material m_Material;
        public BoidsConfig m_Data=BoidsConfig.Default;
        public int m_Count=10;
        public TObjectPoolClass<int,BoidsActor> m_Actors;

        public void Init()
        {
            m_Actors = new TObjectPoolClass<int, BoidsActor>(transform.Find("Actor"));
            Spawn(m_Count);
        }

        public void Spawn(int _count)
        {
            for (int i = 0; i < _count; i++)
            {
                ABoidsBehaviourController controller = butterfly ? new ButterflyBehaviourController() : (ABoidsBehaviourController)new BirdBehaviourController();
                m_Actors.Spawn().Spawn(m_Data,controller,transform.position,m_Meshes,m_Material);
            }
        }
        
        public void Clear()
        {
            m_Actors.Clear();
        }

        public void Tick(float _deltaTime)
        {
            m_Actors.Traversal(p=>p.Tick(_deltaTime,m_Actors));
        }
        
        public void Startle()
        {
            foreach (var actor in m_Actors)
                actor.m_Behaviour.Startle();
        }

        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {  
            m_Actors?.Collect(p=>UnityEditor.Selection.activeObject==p.Transform.gameObject).Traversal(p=>p.DrawGizmosSelected());
        }
        #endif
    }
}