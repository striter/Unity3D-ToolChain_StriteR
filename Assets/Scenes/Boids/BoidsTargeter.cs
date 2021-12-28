using System;
using System.Security.Cryptography;
using UnityEngine;

namespace Boids
{
    public abstract class ABoidsTarget
    {
        protected BoidsActor m_Actor { get; private set; }
        public Vector3 m_Destination { get; private set; }
        public Vector3 m_Forward { get; private set; }
        public Vector3 m_Up { get; private set; }
        public Vector3 m_Right { get; private set; }
        public Quaternion m_Rotation { get; private set; }
        public virtual void Spawn(BoidsActor _actor,Matrix4x4 _landing)
        {
            m_Actor = _actor;
            SetTarget(_landing);
        }

        protected void SetTarget(Matrix4x4 _landing)
        {
            m_Destination = _landing.MultiplyPoint(Vector3.zero);
            m_Forward = _landing.MultiplyVector(Vector3.forward);
            m_Up = _landing.MultiplyVector(Vector3.up);
            m_Right = _landing.MultiplyVector(Vector3.right);
            m_Rotation = _landing.rotation;
        }
        public virtual bool TryLanding() => true;
    }
}