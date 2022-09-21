using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[AttributeUsage(AttributeTargets.Field)]
public class MFoldoutAttribute : PropertyAttribute
{
    public readonly KeyValuePair<string, object[]>[] m_FieldsMatches;
    public MFoldoutAttribute(params KeyValuePair<string, object[]>[] _pairs) { m_FieldsMatches = _pairs; }
    public MFoldoutAttribute(string _foldoutFieldName, params object[] _refValues) : this(new KeyValuePair<string, object[]>(_foldoutFieldName, _refValues)) { }
    public MFoldoutAttribute(string _foldoutFieldName1, object _refValue1, string _foldoutFieldName2, object _refValue2) : this(new KeyValuePair<string, object[]>(_foldoutFieldName1, new object[] { _refValue1 }), new KeyValuePair<string, object[]>(_foldoutFieldName2, new object[] { _refValue2 })) { }
    public MFoldoutAttribute(string _foldoutFieldName1, object _refValue1, string _foldoutFieldName2, object _refValue2,string _foldoutFieldName3, object _refValue3) : this(new KeyValuePair<string, object[]>(_foldoutFieldName1, new object[] { _refValue1 }), new KeyValuePair<string, object[]>(_foldoutFieldName2, new object[] { _refValue2 }),new KeyValuePair<string, object[]>(_foldoutFieldName3,new object[]{_refValue3})) { }
}

[AttributeUsage(AttributeTargets.Field)]
public class MFoldAttribute : MFoldoutAttribute
{
    public MFoldAttribute(string _foldoutFieldName) : base(_foldoutFieldName, null) { }
    public MFoldAttribute(string _foldoutFieldName, params object[] _refValues) : base(_foldoutFieldName, _refValues) { }
    public MFoldAttribute(params KeyValuePair<string, object[]>[] _pairs) : base(_pairs) { }
}

[AttributeUsage(AttributeTargets.Field)]
public class ExtendButtonAttribute : PropertyAttribute
{
    public readonly (string title, string method, object[] parameters)[] m_Buttons;
    public ExtendButtonAttribute(string _title, string _method, params object[] _parameters)
    {
        m_Buttons = new (string title, string method, object[] parameters)[]{(_title,_method,_parameters)};
    }
    public ExtendButtonAttribute(string _title1, string _method1, object[] _parameters1,string _title2, string _method2, object[] _parameters2)
    {
        m_Buttons = new (string title, string method, object[] parameters)[]{(_title1,_method1,_parameters1),(_title2,_method2,_parameters2)};
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class ScriptableObjectEditAttribute : PropertyAttribute { }

[AttributeUsage(AttributeTargets.Field)]
public class MTitleAttribute : PropertyAttribute { }

[AttributeUsage(AttributeTargets.Field)]
public class Readonly:PropertyAttribute{ }

[AttributeUsage(AttributeTargets.Field)]
public class IntEnumAttribute : PropertyAttribute
{
    public readonly int[] m_Values;
    public IntEnumAttribute(params int[] _values)
    {
        m_Values = _values;
        #if UNITY_EDITOR
        Type intType =typeof(int);
        if (m_Values.Any(p => p.GetType() != intType))
            throw new Exception("Type Must All Equals");
        #endif 
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class PositionAttribute : PropertyAttribute { }

[AttributeUsage(AttributeTargets.Field)]
public class CullingMaskAttribute : PropertyAttribute { }


[AttributeUsage(AttributeTargets.Field)]
public class MinMaxRangeAttribute : PropertyAttribute
{
    public readonly float m_Min;
    public readonly float m_Max;
    public readonly string m_MaxTarget;

    public MinMaxRangeAttribute(string _maxTarget,float _min,float _max)
    {
        m_Min = _min;
        m_Max = _max;
        m_MaxTarget = _maxTarget;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class RangeVectorAttribute : PropertyAttribute
{
    public readonly float m_Min;
    public readonly float m_Max;
    public RangeVectorAttribute(float _min,float _max)
    {
        m_Min = _min;
        m_Max = _max;
    }
}
[AttributeUsage(AttributeTargets.Field)]
public class ClampAttribute:PropertyAttribute
{
    public readonly float m_Min;
    public readonly float m_Max;
    public ClampAttribute(float _min=float.MinValue, float _max=float.MaxValue)
    {
        m_Min = _min;
        m_Max = _max;
    }
}


[AttributeUsage(AttributeTargets.Field)]
public class PreloadAssetsAttribute : PropertyAttribute
{
    public readonly string m_Path;
    public PreloadAssetsAttribute(string _path)
    {
        m_Path = _path;
    }
}