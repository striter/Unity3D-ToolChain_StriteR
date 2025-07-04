﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class TitleAttribute : PropertyAttribute { }

[AttributeUsage(AttributeTargets.Field)]
public class ScriptableObjectEditAttribute : PropertyAttribute
{
    public bool on;
    public ScriptableObjectEditAttribute()
    {
        on = false;
    }
    
    public ScriptableObjectEditAttribute(bool _on)
    {
        on = _on;
    }
}


[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field,AllowMultiple = true)]
public class ReadonlyAttribute : PropertyAttribute{ }

[AttributeUsage(AttributeTargets.Field)]
public class RenameAttribute : PropertyAttribute
{
    public string name;
    public RenameAttribute(string _name)
    {
        name = _name;
    }
}

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
public class InspectorExtensionAttribute : PropertyAttribute { }

[AttributeUsage(AttributeTargets.Field)]
public class PositionAttribute : PropertyAttribute { }

[AttributeUsage(AttributeTargets.Field)]
public class MinMaxRangeAttribute : PropertyAttribute
{
    public readonly float m_Min;
    public readonly float m_Max;
    public readonly string m_MaxTarget;

    public MinMaxRangeAttribute(float _min, float _max)
    {
        m_Min = _min;
        m_Max = _max;
    }
    
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
    public string m_ClampMinField;
    public readonly float m_Min;
    public string m_ClampMaxField;
    public readonly float m_Max;
    public ClampAttribute(float _min=float.MinValue, float _max=int.MaxValue)
    {
        m_Min = _min;
        m_Max = _max;
    }

    public ClampAttribute(float _min, string _clampMaxField)
    {
        m_Min = _min;
        m_ClampMaxField = _clampMaxField;
    }

    public ClampAttribute(string _clampMinField, float _max)
    {
        m_ClampMinField = _clampMinField;
        m_Max = _max;
    }
    
    public ClampAttribute(string _clampMinField, string _clampMaxField)
    {
        m_ClampMinField = _clampMinField;
        m_ClampMaxField = _clampMaxField;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class CullingMaskAttribute : PropertyAttribute {
    public static bool Enabled(int _mask,int _layer) =>  _mask == -1 || ((_mask >> _layer) & 1) == 1;
}

[AttributeUsage(AttributeTargets.Field)]
public class DefaultAssetAttribute : PropertyAttribute
{
    public string m_RelativePath;
    public DefaultAssetAttribute(string relativePath)
    {
        m_RelativePath = relativePath;
    }
}