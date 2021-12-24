using System.Collections.Generic;
using TPool;
using UnityEngine;

namespace Boids
{
    public class BoidsActor : APoolItem<int>
    {
        public ABoidsBehaviour m_Behaviour { get; private set; }
        public readonly BoidsAnimation m_Animation;
        public readonly BoidsTarget m_Target;
        
        public BoidsActor(Transform _transform) : base(_transform)
        {
            m_Animation = new BoidsAnimation(_transform);
            m_Target = new BoidsTarget();
        }
        public BoidsActor Spawn(ABoidsBehaviour _behaviour,Vector3 _position,Mesh[] _blendMeshes,Material _material)
        { 
            m_Animation.Init(_material,_blendMeshes);
            m_Target.m_Destination = _position;
            m_Behaviour=_behaviour.Init(this);
            m_Behaviour.Spawn(m_Target.m_Destination,Quaternion.identity);
            return this;
        }
        
        public override void OnPoolRecycle()
        {
            base.OnPoolRecycle();
            m_Behaviour.Recycle();
            m_Behaviour = null;
        }
        
        public void Tick(float _deltaTime, IEnumerable<BoidsActor> _flock)
        {
            m_Animation.Tick(_deltaTime);
            m_Behaviour.Tick(_deltaTime,_flock,out var position,out var rotation);
            
            Transform.position = position;
            Transform.rotation = Quaternion.Lerp(Transform.rotation, rotation, _deltaTime * 5f);
        }
        public void DrawGizmosSelected()
        {
            m_Behaviour?.DrawGizmosSelected();
            // Gizmos.DrawRay(Vector3.zero,m_HoverDirection);
        }
    }
}