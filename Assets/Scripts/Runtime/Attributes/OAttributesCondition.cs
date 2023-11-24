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
        AllNonEquals,
        AnyEquals,
        AnyNonEquals,
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
    public override EConditionAction Condition => EConditionAction.AnyEquals;
    public MFoldoutAttribute(string _foldoutFieldName, params object[] _refValues) : base(new KeyValuePair<string, object[]>(_foldoutFieldName, _refValues)) { }
    public MFoldoutAttribute(string _foldoutFieldName1, object _refValue1, string _foldoutFieldName2, object _refValue2) : base(new KeyValuePair<string, object[]>(_foldoutFieldName1, new object[] { _refValue1 }), new KeyValuePair<string, object[]>(_foldoutFieldName2, new object[] { _refValue2 })) { }
    public MFoldoutAttribute(string _foldoutFieldName1, object _refValue1, string _foldoutFieldName2, object[] _refValue2) : base(new KeyValuePair<string, object[]>(_foldoutFieldName1, new object[] { _refValue1 }), new KeyValuePair<string, object[]>(_foldoutFieldName2,  _refValue2 )) { }
}

[AttributeUsage(AttributeTargets.Field)]
public class MFoldAttribute : ConditionAttribute
{
    public override EConditionAction Condition => EConditionAction.AnyNonEquals;
    public MFoldAttribute(string _foldoutFieldName) : base(new KeyValuePair<string, object[]>(_foldoutFieldName, null)) { }
    public MFoldAttribute(string _foldoutFieldName, params object[] _refValues) : base(new KeyValuePair<string, object[]>(_foldoutFieldName, _refValues)) { }
    public MFoldAttribute(params KeyValuePair<string, object[]>[] _pairs) : base(_pairs) { }
}


[AttributeUsage(AttributeTargets.Method)]
public class ButtonAttribute : ConditionAttribute
{
    protected EConditionAction m_Condition = EConditionAction.AlwaysVisible;
    public override EConditionAction Condition => m_Condition;
    public ButtonAttribute()
    {
        
    }

    public ButtonAttribute(EConditionAction _conditionAction, string _conditions,params object[] _refValues):base(new ConditionFieldParameters(_conditions,_refValues))
    {
        m_Condition = _conditionAction;
    }
    
}


[AttributeUsage(AttributeTargets.Field)]        //Should be deprecated tbh
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

