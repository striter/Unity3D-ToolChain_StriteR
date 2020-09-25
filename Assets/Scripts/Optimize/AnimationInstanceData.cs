using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationInstanceData : ScriptableObject
{
    public Mesh m_InstanceMesh;
    public Texture2D m_AnimationAtlas;
    public AnimationInstanceParam[] m_AnimationParams;
}

[System.Serializable]
public class AnimationInstanceParam
{
    public string m_Name;
    public int m_FrameBegin;
    public float m_FrameRate;
    public int m_FrameCount;
    public float m_Length;
    public bool m_Loop;
    public AnimationInstanceParam(string _name, int _startFrame,float _frameRate,float _length,bool _loop)
    {
        m_Name = _name;
        m_FrameBegin = _startFrame;
        m_FrameRate = _frameRate;
        m_FrameCount =(int)( _frameRate * _length);
        m_Length = _length;
        m_Loop= _loop;
    }
}