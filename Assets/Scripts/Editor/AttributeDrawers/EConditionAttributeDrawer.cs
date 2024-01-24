using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;
using System.Linq;
using Object = System.Object;

namespace UnityEditor.Extensions
{
    [CustomPropertyDrawer(typeof(ConditionAttribute),true)]
    public class ConditionAttributeDrawer:MainAttributePropertyDrawer<ConditionAttribute> 
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!(attribute as ConditionAttribute).IsPropertyVisible( property))
                return -2;

            return base.GetPropertyHeight(property, label);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!(attribute as ConditionAttribute).IsPropertyVisible( property))
                return;
            
            base.OnGUI(position, property, label);
        }
    }

    public static class UConditionAttribute
    {

        static bool Equals(this ConditionAttribute.ConditionFieldParameters parameter, object _comparer)
        {
            return parameter.refValue?.Contains(_comparer) ??  _comparer == null;
        }
        

        static bool IsVisible(this ConditionAttribute _attribute, Func<IEnumerable<(FieldInfo, object)>> _getFields)
        {
            if (_attribute.m_Conditions==null || _attribute.m_Conditions.Length==0) return true;
            var fields = _getFields();
            switch (_attribute.Condition)
            {
                default: throw new InvalidEnumArgumentException();
                case ConditionAttribute.EConditionAction.AlwaysVisible: return true;
                case ConditionAttribute.EConditionAction.AnyEquals: return _attribute.m_Conditions.All(condition => fields.Collect(p => p.Item1.Name == condition.fieldName).Any(p => Equals(condition,p.Item2)));
                case ConditionAttribute.EConditionAction.NonAnyEquals: return !_attribute.m_Conditions.All(condition => fields.Collect(p => p.Item1.Name == condition.fieldName).Any(p => Equals(condition,p.Item2)));
                case ConditionAttribute.EConditionAction.AllEquals: return _attribute.m_Conditions.All(condition => fields.Collect(p => p.Item1.Name == condition.fieldName).All(p => Equals(condition,p.Item2)));
                case ConditionAttribute.EConditionAction.NonAllEquals: return _attribute.m_Conditions.All(condition => fields.Collect(p => p.Item1.Name == condition.fieldName).All(p => !Equals(condition,p.Item2)));
            }
        }
        public static bool IsPropertyVisible(this ConditionAttribute _attribute,SerializedProperty _property)=>IsVisible(_attribute,()=>_property.AllRelativeFields());
        public static bool IsElementVisible(this ConditionAttribute _attribute,Object _target)=>IsVisible(_attribute,()=>_target.GetType().GetFields().Select(p=>(p,p.GetValue(_target))));
    }
}