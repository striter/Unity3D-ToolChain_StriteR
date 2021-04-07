using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class CullingMaskAttribute : PropertyAttribute { }

[AttributeUsage(AttributeTargets.Field)]
public class RangeVectorAttribute : PropertyAttribute 
{
    public float m_Min { get; private set; }
    public float m_Max { get; private set; }
    public RangeVectorAttribute(float _min,float _max)
    {
        m_Min = _min;
        m_Max = _max;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class RangeIntAttribute:PropertyAttribute
{
    public int m_Min { get; private set; }
    public int m_Max { get; private set; }
    public RangeIntAttribute(int _min,int _max)
    {
        m_Min = _min;
        m_Max = _max;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class ClampAttribute:PropertyAttribute
{
    public float m_Min { get; private set; }
    public float m_Max { get; private set; }
    public ClampAttribute(float _min=float.MinValue, float _max=float.MaxValue)
    {
        m_Min = _min;
        m_Max = _max;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class MFoldoutAttribute:PropertyAttribute
{
    public string m_FieldName;
    public object[] m_Value;
    public MFoldoutAttribute(string _foldoutFieldName,params object[] _refValues)
    {
        m_FieldName = _foldoutFieldName;
        m_Value = _refValues;
    }
}
[AttributeUsage(AttributeTargets.Field)]
public class MFoldAttribute : MFoldoutAttribute
{
    public MFoldAttribute(string _foldoutFieldName) : base(_foldoutFieldName, null) { }
    public MFoldAttribute(string _foldoutFieldName, params object[] _refValues) : base(_foldoutFieldName, _refValues) { }
}
[AttributeUsage(AttributeTargets.Field)]
public class MTitleAttribute:PropertyAttribute
{

}