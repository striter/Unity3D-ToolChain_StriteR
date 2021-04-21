using System;
using System.Collections.Generic;
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
    public KeyValuePair<string, object[]>[] m_FieldsMatches;
    public MFoldoutAttribute(params KeyValuePair<string, object[]>[] _pairs) { m_FieldsMatches = _pairs; }
    public MFoldoutAttribute(string _foldoutFieldName, params object[] _refValues) : this(new KeyValuePair<string, object[]>(_foldoutFieldName, _refValues)) { }
    public MFoldoutAttribute(string _foldoutFieldName1, object _refValue1,string _foldoutFieldName2,object _refValue2) : this(new KeyValuePair<string, object[]>(_foldoutFieldName1,new object[] { _refValue1}),new KeyValuePair<string,object[]>(_foldoutFieldName2,new object[] { _refValue2})) { }
}
[AttributeUsage(AttributeTargets.Field)]
public class MFoldAttribute : MFoldoutAttribute
{
    public MFoldAttribute(string _foldoutFieldName) : base(_foldoutFieldName, null) { }
    public MFoldAttribute(string _foldoutFieldName, params object[] _refValues) : base(_foldoutFieldName, _refValues) { }
    public MFoldAttribute(params KeyValuePair<string, object[]>[] _pairs) : base(_pairs) { }
}
[AttributeUsage(AttributeTargets.Field)]
public class MTitleAttribute:PropertyAttribute
{

}