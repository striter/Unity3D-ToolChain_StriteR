using System.Collections.Generic;
using TPool;
using UnityEngine;

namespace PCG.Module.BOIDS
{
    public class BoidsActor : APoolItem<int>,IBoidsVertex
    {
        public readonly IBoidsAnimation m_Animation;
        public readonly ABoidsTarget m_Target;
        public readonly ABoidsBehaviour m_Behaviour;
        public Vector3 Position => Transform.position;
        public Quaternion Rotation => Transform.rotation;
        public Vector3 Up => Transform.up;
        public Vector3 Forward => m_Behaviour.m_Direction;
        public BoidsActor(Transform _transform,ABoidsBehaviour _behaviour,ABoidsTarget _target,IBoidsAnimation _animation) : base(_transform)
        {
            m_Behaviour = _behaviour;
            m_Target = _target;
            m_Animation = _animation;
            m_Animation.Init(_transform);
        }
        public BoidsActor Initialize(FBoidsVertex _start)
        {
            m_Animation.Initialize();
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
            
            Transform.position = position;
            Transform.rotation = rotation;
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