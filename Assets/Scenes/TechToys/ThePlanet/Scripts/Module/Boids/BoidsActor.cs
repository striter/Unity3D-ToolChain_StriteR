using System.Collections.Generic;
using TPool;
using UnityEngine;

namespace TechToys.ThePlanet.Module.BOIDS
{
    public class BoidsActor : APoolElement , IBoidsVertex
    {
        public IBoidsAnimation m_Animation { get; private set; }
        public ABoidsTarget m_Target { get; private set; }
        public ABoidsBehaviour m_Behaviour { get; private set; }
        public Vector3 Position => transform.position;
        public Quaternion Rotation => transform.rotation;
        public Vector3 Up => transform.up;
        public Vector3 Forward => m_Behaviour.m_Direction;
        public BoidsActor(Transform _transform) : base(_transform)
        {
        }
        public BoidsActor Initialize(ABoidsBehaviour _behaviour,ABoidsTarget _target,IBoidsAnimation _animation)
        {
            m_Behaviour = _behaviour;
            m_Target = _target;
            m_Animation = _animation;
            m_Animation.Init(transform);
            m_Animation.Initialize();
            return this;
        }
        public BoidsActor SetVertex(FBoidsVertex _start)
        {
            m_Target.Initialize(this,_start);
            m_Behaviour.Initialize(this,_start);
            return this;
        }
        public override void OnPoolRecycle()
        {
            base.OnPoolRecycle();
            m_Behaviour.Recycle();
        }
        public void Tick(float _deltaTime, Dictionary<int,BoidsActor> _actors)
        {
            m_Animation.Tick(_deltaTime);
            m_Behaviour.Tick(_deltaTime,m_Target.FilterMembers(_actors),out var position,out var rotation);
            
            transform.position = position;
            transform.rotation = rotation;
        }
#if UNITY_EDITOR
        public void DrawGizmosSelected()
        {
            Gizmos.DrawSphere(Position,.2f);
            Gizmos.DrawLine(Position,Position+Rotation*Vector3.forward*.4f);

            Gizmos.DrawLine(Position,m_Target.m_Destination);
            Gizmos.matrix = Matrix4x4.TRS(Position,Rotation,Vector3.one);
            
            Gizmos.DrawLine(Position,Position+Forward*.2f);

            m_Target?.OnDrawGizmosSelected();
            m_Behaviour?.DrawGizmosSelected();
        }
#endif
    }
}