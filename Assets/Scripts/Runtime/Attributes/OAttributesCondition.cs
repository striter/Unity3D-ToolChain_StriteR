using System;
using System.Collections.Generic;
using System.Linq;
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

    public struct Prediction
    {
        public string fieldName;
        public Func<object,bool> prediction;
        public Prediction(string _fieldName,Func<object,bool> _prediction)
        {
            fieldName = _fieldName;
            prediction = _prediction;
        }
    }
    
    public abstract EConditionAction Condition { get; }
    public readonly Prediction[] m_Conditions;

    public ConditionAttribute()
    {
        m_Conditions = null;
    }
    public ConditionAttribute(Prediction[] _conditions) => m_Conditions = _conditions;

    public ConditionAttribute(params KeyValuePair<string, object[]>[] _pairs) :this( _pairs.Select(p=>new Prediction(p.Key,fieldValue=>FieldEquals(fieldValue,p.Value))).ToArray()) { }

    static bool FieldEquals(object _comparer, object[] refValues)
    {
        if (_comparer is Enum enumComparer && _comparer.GetType().GetCustomAttributes(typeof(FlagsAttribute),false).Length>0)
            return refValues?.All(p => enumComparer.HasFlag((Enum)p)) ?? false;
        return refValues?.Contains(_comparer) ??  _comparer == null;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class FoldoutAttribute : ConditionAttribute
{
    public override EConditionAction Condition => EConditionAction.AllEquals;
    public FoldoutAttribute(string _fieldName, params object[] _refValues) : base(new KeyValuePair<string, object[]>(_fieldName, _refValues)) { }
    public FoldoutAttribute(string _fieldName1, object _refValue1, string _fieldName2, object _refValue2) : base(new KeyValuePair<string, object[]>(_fieldName1, new [] { _refValue1 }), new KeyValuePair<string, object[]>(_fieldName2, new [] { _refValue2 })) { }
    public FoldoutAttribute(string _fieldName1, object _refValue, string _fieldName2, params object[] _refValues) : base(new KeyValuePair<string, object[]>(_fieldName1, new [] { _refValue }), new KeyValuePair<string, object[]>(_fieldName2,  _refValues )) { }
}

[AttributeUsage(AttributeTargets.Field)]
public class FoldAttribute : ConditionAttribute
{
    public override EConditionAction Condition => EConditionAction.NonAnyEquals;
    public FoldAttribute(string _fieldName) : base(new KeyValuePair<string, object[]>(_fieldName, null)) { }
    public FoldAttribute(string _fieldName, params object[] _refValues) : base(new KeyValuePair<string, object[]>(_fieldName, _refValues)) { }
}

[AttributeUsage(AttributeTargets.Method)]
public class InspectorButtonAttribute : ConditionAttribute
{
    public override EConditionAction Condition => EConditionAction.AlwaysVisible;
    public bool undo;
    public InspectorButtonAttribute(bool _undo = false) { undo = _undo;}
    protected InspectorButtonAttribute(bool _undo,params Prediction[] _param) : base(_param) { undo = _undo; }
    protected InspectorButtonAttribute(params KeyValuePair<string, object[]>[] _pairs) : base(_pairs) { }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class InspectorButtonFoldout : InspectorButtonAttribute
{
    public override EConditionAction Condition => EConditionAction.AllEquals;
    public InspectorButtonFoldout(string _fieldName, params object[] _refValues) : base(new KeyValuePair<string, object[]>(_fieldName, _refValues)) { }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class InspectorButtonFold : InspectorButtonAttribute
{
    public override EConditionAction Condition => EConditionAction.NonAnyEquals;
    public InspectorButtonFold(string _fieldName, params object[] _refValues) : base(new KeyValuePair<string, object[]>(_fieldName, _refValues)) { }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class InspectorButtonEditor : InspectorButtonAttribute
{
    public override EConditionAction Condition => EConditionAction.AllEquals;
    public InspectorButtonEditor(bool _undo = false):base(_undo, new Prediction(null,_=>!Application.isPlaying)) { }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class InspectorButtonRuntime : InspectorButtonAttribute
{
    public override EConditionAction Condition => EConditionAction.AllEquals;
    public InspectorButtonRuntime(bool _undo = false) : base(_undo, new Prediction(null, _ => Application.isPlaying)) { }
}
