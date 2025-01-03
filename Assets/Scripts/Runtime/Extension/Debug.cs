using System;
using System.Reflection;
using UnityEngine;

public static class UDebug
{
    static object CheckValue(object _value,Type _type)
    {
        if (_value is not double doubleVal) 
            return _value;
        
        if(_type == typeof(float))
            return Convert.ToSingle(doubleVal);
        if (_type == typeof(int))
            return Convert.ToInt32(doubleVal);

        return _value;
    }

    private static readonly BindingFlags kBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
    public static object GetFieldValue(object _parent, string _paths)
    {
        if (string.IsNullOrEmpty(_paths))
            return _parent;
        
        var target = _parent;
        var paths = _paths.Split('.');
        for (var i = 0; i < paths.Length; i++)
        {
            var fieldInfo = target.GetType().GetField(paths[i], kBindingFlags);
            if (fieldInfo == null)
            {
                Debug.LogError($"Field {paths[i]} not found for {target}({target.GetType().Name})");
                return null;
            }
            target = fieldInfo.GetValue(target);
            if (target == null)
                break;
        }
        return target;
    }
    
    public static object GetPropertyValue(object _parent, string _paths)
    {
        if (string.IsNullOrEmpty(_paths))
            return _parent;
        
        var target = _parent;
        var paths = _paths.Split('.');
        for (var i = 0; i < paths.Length; i++)
        {
            var propertyInfo = target.GetType().GetProperty(paths[i], kBindingFlags);
            if (propertyInfo == null)
            {
                Debug.LogError($"Property {paths[i]} not found for {target}({target.GetType().Name})");
                return null;
            }
            target = propertyInfo.GetValue(target);
        }
        return target;
    }

    public static object GetIndex(object _parent, int _index)
    {
        var type = _parent.GetType();
        var propertyInfo = type.GetProperty("Item",kBindingFlags);
        if (propertyInfo == null)
        {
            Debug.LogError($"Property Item not found for {type.Name}");
            return null;
        }
        
        return propertyInfo.GetValue(_parent, new object[] { _index });
    }

    public static void SetIndex(object _parent, int _index, object _value)
    {
        var type = _parent.GetType();
        var propertyInfo = type.GetProperty("Item",kBindingFlags);
        if (propertyInfo == null)
        {
            Debug.LogError($"Property Item not found for {type.Name}");
            return;
        }
        propertyInfo.SetValue(_parent, _value, new object[] { _index });
    }
    
    public static object CallMethod(object _object, string _methodName, params object[] _args)
    {
        var type = _object.GetType();
        var method = _object.GetType().GetMethod(_methodName, kBindingFlags);
        if (method == null)
        {
            Debug.LogError($"Method {_methodName} not found for {_object}({type.Name})");
            return null;
        }
        
        var argsInfo = method.GetParameters();
        if (_args.Length != argsInfo.Length)
        {
            Debug.LogError($"Method {_methodName} has {argsInfo.Length} args, but {_args.Length} given for {_object}({type.Name})");
            return null;
        }
        
        if(_args.Length != 0)
            for (var i = 0; i < argsInfo.Length; i++)
                _args[i] = CheckValue(_args[i], argsInfo[i].ParameterType);
        
        return method.Invoke(_object, _args);
    }
    
    public static void SetFieldValue(object _object,  string _fieldName, object _value)
    {
        var type = _object.GetType();
        var fieldInfo = type.GetField(_fieldName,kBindingFlags);
        if (fieldInfo == null)
        {
            Debug.LogError($"Field:{_fieldName} not found in {type.Name}");
            return;
        }

        _value = CheckValue(_value,fieldInfo.FieldType);
        fieldInfo.SetValue(_object,_value);
    }

    public static void SetPropertyValue(object _object, string _propertyName, object _value)
    {
        var type = _object.GetType();
        var propertyInfo = type.GetProperty(_propertyName,kBindingFlags);
        if (propertyInfo == null)
        {
            Debug.LogError($"Property:{_propertyName} not found in {type.Name}");
            return;
        }
        
        propertyInfo.SetValue(_object,CheckValue(_value,propertyInfo.PropertyType));
    }
}
