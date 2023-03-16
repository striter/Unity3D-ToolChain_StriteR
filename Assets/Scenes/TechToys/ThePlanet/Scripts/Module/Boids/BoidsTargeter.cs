using System.Collections.Generic;
using UnityEngine;

namespace TechToys.ThePlanet.Module.BOIDS
{
    public interface IBoidsVertex
    {
        Vector3 Position { get; }
        Quaternion Rotation { get; }
        Vector3 Forward { get; }
        Vector3 Up { get; }
    }

    public struct FBoidsVertex : IBoidsVertex
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 forward;
        public Vector3 up;
        public static readonly FBoidsVertex kZero = new FBoidsVertex(){position = Vector3.zero,rotation = Quaternion.identity,forward = Vector3.forward };
        public FBoidsVertex(Vector3 _position, Quaternion _rotation)
        {
            position = _position;
            rotation = _rotation;
            forward = _rotation * Vector3.forward;
            up = _rotation * Vector3.up;
        }
        public Vector3 Position => position;
        public Quaternion Rotation => rotation;
        public Vector3 Forward => forward;
        public Vector3 Up => up;
    }

    public struct FBoidsTransform : IBoidsVertex
    {
        public readonly Transform transform;
        public Vector3 Position => transform.position;
        public Quaternion Rotation => transform.rotation;
        public Vector3 Forward => transform.forward;
        public Vector3 Up => transform.up;
        public FBoidsTransform(Transform _transform)
        {
            transform = _transform;
        }
    }
    
    public abstract class ABoidsTarget:IBoidsVertex
    {
        protected BoidsActor m_Actor { get; private set; }
        public IBoidsVertex m_Target { get; private set; }
        public Vector3 m_Destination => m_Target.Position;
        public Vector3 m_Forward => m_Rotation * Vector3.forward;
        public Vector3 m_Up => m_Rotation * Vector3.up;
        public Vector3 m_Right => m_Rotation*Vector3.right;
        public Quaternion m_Rotation => m_Target.Rotation;
        public Vector3 Position => m_Actor.Position;
        public Quaternion Rotation => m_Actor.Rotation;
        public Vector3 Forward => m_Actor.Forward;
        public Vector3 Up => m_Actor.Up;
        public virtual void Initialize(BoidsActor _actor,IBoidsVertex _vertex)
        {
            m_Actor = _actor;
            SetTarget(_vertex);
        }

        public void SetTarget(IBoidsVertex _vertex)
        {
            m_Target = _vertex;
        }
        
        private static readonly List<BoidsActor> kActors = new List<BoidsActor>();

        public virtual IList<BoidsActor> FilterMembers(Dictionary<int, BoidsActor> _totalMembers)
        {
            _totalMembers.Values.FillList(kActors);
            return kActors;
        }
        public virtual bool PickAvailablePerching() => true;
        public virtual bool PickAnotherSpot() => true;
        
        public virtual void OnDrawGizmosSelected()
        {
            Gizmos.matrix=Matrix4x4.identity;
            Gizmos.DrawLine(m_Actor.Position,m_Actor.m_Target.m_Destination);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(m_Actor.m_Target.m_Destination,m_Actor.m_Target.m_Right);
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(m_Actor.m_Target.m_Destination,m_Actor.m_Target.m_Forward);
            Gizmos.color = Color.green;
            Gizmos.DrawRay(m_Actor.m_Target.m_Destination,m_Actor.m_Target.m_Up);
            Gizmos.DrawWireSphere(m_Actor.m_Target.m_Destination,.2f);
        }

    }

    public class FBoidsTargetEmpty:ABoidsTarget
    {
        
    }
}