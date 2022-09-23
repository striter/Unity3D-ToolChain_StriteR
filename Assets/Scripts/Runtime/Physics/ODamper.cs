using System;
using UnityEngine;

public enum EDamperMode
{
    Lerp,
    Spring,
}

[Serializable]
public class Damper
{
    public EDamperMode mode = EDamperMode.Lerp;
    [MFoldout(nameof(mode),EDamperMode.Lerp)][Clamp(0)] public float halfLife = 2f;
    [MFoldout(nameof(mode),EDamperMode.Spring)][Range(0,50)] public float stiffness = 11f;
    [MFoldout(nameof(mode),EDamperMode.Spring)][Range(0,30)] public float damping = 6f;
    
    public Vector3 position { get; private set; }
    private Vector3 velocity;

    public void Begin(Vector3 _begin)
    {
        position = _begin;
        velocity = Vector3.zero;
    }
    
    public Vector3 Tick(float _deltaTime,Vector3 _position)
    {
        switch (mode)
        {
            case EDamperMode.Lerp: {
                position = Vector3.Lerp(position,_position, 1.0f - FastNegExp( _deltaTime*0.69314718056f /(halfLife+float.Epsilon)));
            } break;
            case EDamperMode.Spring:
            {
                velocity += _deltaTime * stiffness * (_position - position) + _deltaTime * damping * -velocity;
                position += _deltaTime * velocity;
            } break;
        }
        return position;
    }

    public static float FastNegExp(float x)
    {
        return 1.0f / (1.0f + x + 0.48f * x * x + 0.235f * x * x * x);
    }
}
