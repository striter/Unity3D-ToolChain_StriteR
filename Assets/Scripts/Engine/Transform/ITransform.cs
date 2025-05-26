using UnityEngine;
public interface ITransform
{
    Transform transform { get; }
    public Vector3 position => transform.position;
}