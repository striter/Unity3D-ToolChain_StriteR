using UnityEngine;

namespace TechToys.CharacterControl.InverseKinematics
{
    public abstract class AInverseKinematic : MonoBehaviour
    {
        public abstract bool Valid { get; }
        public virtual void Initialize(){}
        public virtual void UnInitialize(){}
        public abstract void Tick(float _deltaTime);
        public abstract void Reset();
    }

    public enum ETransformAxis
    {
        Forward,
        Back,
        Up,
        Down,
        Right,
        Left,
    }

    public static class ETransformAxis_Extension
    {
        public static Vector3 GetAxis(this Transform _transform,ETransformAxis _axis) => _axis switch {
                ETransformAxis.Forward => _transform.forward,
                ETransformAxis.Back => -_transform.forward,
                ETransformAxis.Right => _transform.right,
                ETransformAxis.Left => -_transform.right,
                ETransformAxis.Up => _transform.up,
                ETransformAxis.Down => -_transform.up,
                _ => Vector3.zero
            };

        public static ETransformAxis GetCrossAxis(this ETransformAxis _axis) => _axis.Next().Next();
    }
}