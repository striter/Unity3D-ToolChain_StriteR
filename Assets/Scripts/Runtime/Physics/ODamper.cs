using System;
using UnityEngine;

[Serializable]
public class Damper
{
    [Clamp(0)] public float damping;
    private Vector3 current;

    public void Begin(Vector3 _begin)
    {
        current = _begin;
    }
    
    public Vector3 Tick(float _deltaTime,Vector3 _position)
    {
        current = Vector3.Lerp(current,_position,_deltaTime*damping);
        return current;
    }
}
