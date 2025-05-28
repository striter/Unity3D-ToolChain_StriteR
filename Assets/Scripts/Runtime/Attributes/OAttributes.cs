using System;
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


[AttributeUsage(AttributeTargets.GenericParameter | AttributeTargets.Field,AllowMultiple = true)]
public class Readonly:PropertyAttribute{ }

[AttributeUsage(AttributeTargets.Field)]
public class Rename : PropertyAttribute
{
    public string name;
    public Rename(string _name)
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
    public readonly float m_Min;
    public readonly float m_Max;
    public ClampAttribute(float _min=float.MinValue, float _max=int.MaxValue)
    {
        m_Min = _min;
        m_Max = _max;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class CullingMaskAttribute : PropertyAttribute {
    public static bool Enabled(int _mask,int _layer) =>  _mask == -1 || ((_mask >> _layer) & 1) == 1;
}

[AttributeUsage(AttributeTargets.Field)]
public class AssetAttribute : PropertyAttribute
{
    public Func<UnityEngine.Object> m_Getter;
    public AssetAttribute(Func<UnityEngine.Object> _getter)
    {
        m_Getter = _getter;
    }
}