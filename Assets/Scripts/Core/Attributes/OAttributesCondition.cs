using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;


public abstract class ConditionAttribute : PropertyAttribute
{
    public enum EConditionAction
    {
        AlwaysVisible,
        AllEquals,
        NonAllEquals,
        AnyEquals,
        NonAnyEquals,
    }

    public struct ConditionFieldParameters
    {
        public string fieldName;
        public object[] refValue;
        public ConditionFieldParameters(string _fieldName,params object[] _refValue)
        {
            fieldName = _fieldName;
            refValue = _refValue;
        }
    }
    
    public abstract EConditionAction Condition { get; }
    public readonly ConditionFieldParameters[] m_Conditions;

    public ConditionAttribute()
    {
        m_Conditions = null;
    }
    public ConditionAttribute(params ConditionFieldParameters[] _conditions)
    {
        m_Conditions = _conditions; 
    }
    public ConditionAttribute(params KeyValuePair<string, object[]>[] _pairs) { m_Conditions = _pairs.Select(p=>new ConditionFieldParameters(p.Key,p.Value)).ToArray(); }
}

[AttributeUsage(AttributeTargets.Field)]
public class MFoldoutAttribute : ConditionAttribute
{
    public override EConditionAction Condition => EConditionAction.AllEquals;
    public MFoldoutAttribute(string _foldoutFieldName, params object[] _refValues) : base(new KeyValuePair<string, object[]>(_foldoutFieldName, _refValues)) { }
    public MFoldoutAttribute(string _foldoutFieldName1, object _refValue1, string _foldoutFieldName2, object _refValue2) : base(new KeyValuePair<string, object[]>(_foldoutFieldName1, new object[] { _refValue1 }), new KeyValuePair<string, object[]>(_foldoutFieldName2, new object[] { _refValue2 })) { }
    public MFoldoutAttribute(string _foldoutFieldName1, object _refValue1, string _foldoutFieldName2, object[] _refValue2) : base(new KeyValuePair<string, object[]>(_foldoutFieldName1, new object[] { _refValue1 }), new KeyValuePair<string, object[]>(_foldoutFieldName2,  _refValue2 )) { }
}

[AttributeUsage(AttributeTargets.Field)]
public class MFoldAttribute : ConditionAttribute
{
    public override EConditionAction Condition => EConditionAction.NonAnyEquals;
    public MFoldAttribute(string _foldoutFieldName) : base(new KeyValuePair<string, object[]>(_foldoutFieldName, null)) { }
    public MFoldAttribute(string _foldoutFieldName, params object[] _refValues) : base(new KeyValuePair<string, object[]>(_foldoutFieldName, _refValues)) { }
    public MFoldAttribute(params KeyValuePair<string, object[]>[] _pairs) : base(_pairs) { }
}

[AttributeUsage(AttributeTargets.Method)]
public class ButtonAttribute : ConditionAttribute
{
    public override EConditionAction Condition => EConditionAction.AlwaysVisible;
    public ButtonAttribute() {}
    protected ButtonAttribute(params KeyValuePair<string, object[]>[] _pairs) : base(_pairs) { }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class FoldoutButtonAttribute : ButtonAttribute
{
    public override EConditionAction Condition => EConditionAction.AnyEquals;
    public FoldoutButtonAttribute(string _foldoutFieldName, params object[] _refValues) : base(new KeyValuePair<string, object[]>(_foldoutFieldName, _refValues)) { }
}