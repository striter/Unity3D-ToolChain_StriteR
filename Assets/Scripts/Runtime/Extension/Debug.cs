using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using System.Reflection;
using UnityEngine;

public static class UDebug
{
    public static object GetType(string _className) => Type.GetType(_className);
    public static object GetInstanceType(object _instance) => _instance.GetType();
    
    private static readonly BindingFlags kInstanceBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy;
    private static readonly BindingFlags kStaticBindingFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
    static object CheckArgsValue(object _value,Type _type)
    {
        if (_value is not double doubleVal) 
            return _value;
        
        if(_type == typeof(float))
            return Convert.ToSingle(doubleVal);
        if (_type == typeof(int))
            return Convert.ToInt32(doubleVal);

        return _value;
    }

    static Type CheckArgsType(Type _inputType,Type _argsType)
    {
        if (_argsType == typeof(int) && _inputType == typeof(double))
            return typeof(int);
        
        if (_argsType == typeof(float) && _inputType == typeof(double))
            return typeof(float);
        return _inputType;
    }


    //Methods    
    private static Dictionary<Type,MethodInfo[]> kMethodsHelper = new Dictionary<Type, MethodInfo[]>();
    public static object CallMethod(object _instance, string _methodName, params object[] _args)
    {
        if (_instance == null)
        {
            Debug.LogError($"Parameter _object can't be null");
            return null;
        }
        
        return CallTypeMethod(_instance.GetType(),_instance,_methodName,kInstanceBindingFlags,_args);
    }
    public static object CallMethodStatic(string _className, string _methodName, params object[] _args)
    {
        var type = Type.GetType(_className);
        if (type == null)
        {
            Debug.LogError($"Type {_className} not found");
            return null;
        }
        return CallTypeMethod(type, null, _methodName, kStaticBindingFlags, _args);
    }

    public static object CallTypeMethod(Type _type,object _instance, string _methodName,BindingFlags _bindingFlags, params object[] _args)
    {
        if (!kMethodsHelper.TryGetValue(_type, out var methods))
        {
            methods = _type.GetMethods(_bindingFlags);
            kMethodsHelper.Add(_type, methods);
        }

        MethodInfo methodToInvoke = null;
        var argsLength = _args.Length;
        foreach(var method in methods.Where(p => p.Name == _methodName))
        {
            var methodParams = method.GetParameters();
            if(method.GetParameters().Length != argsLength)
                continue;

            var validMethod = true;
            for (var i = 0; i < methodParams.Length; i++)
            {
                var param = methodParams[i];
                if (param.ParameterType != CheckArgsType(_args[i].GetType(),param.ParameterType))
                    validMethod = false;
            }

            if (validMethod)
                methodToInvoke = method;
        }
        
        if (methodToInvoke == null)
        {
            Debug.LogError($"Method {_methodName} not found for ({_type.Name})");
            return null;
        }
        
        var argsInfo = methodToInvoke.GetParameters();
        if (_args.Length != argsInfo.Length)
        {
            Debug.LogError($"Method {_methodName} has {argsInfo.Length} args, but {_args.Length} given for ({_type.Name})");
            return null;
        }
        
        if(_args.Length != 0)
            for (var i = 0; i < argsInfo.Length; i++)
                _args[i] = CheckArgsValue(_args[i], argsInfo[i].ParameterType);
        
        return methodToInvoke.Invoke(_instance, _args);
    }
    
    //Setter
    public static void SetFieldValue(object _object,  string _fieldName, object _value)
    {
        var type = _object.GetType();
        var fieldInfo = type.GetField(_fieldName,kInstanceBindingFlags);
        if (fieldInfo == null)
        {
            Debug.LogError($"Field:{_fieldName} not found in {type.Name}");
            return;
        }

        _value = CheckArgsValue(_value,fieldInfo.FieldType);
        fieldInfo.SetValue(_object,_value);
    }
    
    public static void SetFieldValueStatic(string _className, string _fieldName, object _value)
    {
        var type = Type.GetType(_className);
        if (type == null)
        {
            Debug.LogError($"Type {_className} not found");
            return;
        }
        
        var fieldInfo = type.GetField(_fieldName,kStaticBindingFlags);
        if (fieldInfo == null)
        {
            Debug.LogError($"Field:{_fieldName} not found in {type.Name}");
            return;
        }
        
        _value = CheckArgsValue(_value,fieldInfo.FieldType);
        fieldInfo.SetValue(null,_value);
    }

    public static void SetPropertyValue(object _object, string _propertyName, object _value)
    {
        var type = _object.GetType();
        var propertyInfo = type.GetProperty(_propertyName,kInstanceBindingFlags);
        if (propertyInfo == null)
        {
            Debug.LogError($"Property:{_propertyName} not found in {type.Name}");
            return;
        }
        
        propertyInfo.SetValue(_object,CheckArgsValue(_value,propertyInfo.PropertyType));
    }
    
    public static void SetPropertyValueStatic(string _className, string _propertyName, object _value)
    {
        var type = Type.GetType(_className);
        if (type == null)
        {
            Debug.LogError($"Type {_className} not found");
            return;
        }
        
        var propertyInfo = type.GetProperty(_propertyName,kStaticBindingFlags);
        if (propertyInfo == null)
        {
            Debug.LogError($"Property:{_propertyName} not found in {type.Name}");
            return;
        }
        
        propertyInfo.SetValue(null,CheckArgsValue(_value,propertyInfo.PropertyType));
    }
    
    //Getter
    
    public static object GetFieldValue(object _parent, string _paths)
    {
        if (string.IsNullOrEmpty(_paths))
            return _parent;
        
        var target = _parent;
        var paths = _paths.Split('.');
        for (var i = 0; i < paths.Length; i++)
        {
            var fieldInfo = target.GetType().GetField(paths[i], kInstanceBindingFlags);
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
    
    public static object GetFieldValueStatic(string _className, string _path)
    {
        if (string.IsNullOrEmpty(_path))
            return null;
        
        var type = Type.GetType(_className);
        if (type == null)
        {
            Debug.LogError($"Type {_className} not found");
            return null;
        }
        
        var fieldInfo = type.GetField(_path, kStaticBindingFlags);
        if (fieldInfo == null)
        {
            Debug.LogError($"Field {_path} not found for {type.Name}");
            return null;
        }
        return fieldInfo.GetValue(null);
    }
    
    public static object GetPropertyValue(object _parent, string _paths)
    {
        if (string.IsNullOrEmpty(_paths))
            return _parent;
        
        var target = _parent;
        var paths = _paths.Split('.');
        for (var i = 0; i < paths.Length; i++)
        {
            var propertyInfo = target.GetType().GetProperty(paths[i], kInstanceBindingFlags);
            if (propertyInfo == null)
            {
                Debug.LogError($"Property {paths[i]} not found for {target}({target.GetType().Name})");
                return null;
            }
            target = propertyInfo.GetValue(target);
        }
        return target;
    }

    public static void GetPropertyValueStatic(string _className, string _property, out object _value)
    {
        var type = Type.GetType(_className);
        if (type == null)
        {
            Debug.LogError($"Type {_className} not found");
            _value = null;
            return;
        }
        
        var propertyInfo = type.GetProperty(_property,kStaticBindingFlags);
        if (propertyInfo == null)
        {
            Debug.LogError($"Property:{_property} not found in {type.Name}");
            _value = null;
            return;
        }
        
        _value = propertyInfo.GetValue(null);
    }
    
    public static object GetIndex(object _parent, int _index)
    {
        var type = _parent.GetType();
        var propertyInfo = type.GetProperty("Item",kInstanceBindingFlags);
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
        var propertyInfo = type.GetProperty("Item",kInstanceBindingFlags);
        if (propertyInfo == null)
        {
            Debug.LogError($"Property Item not found for {type.Name}");
            return;
        }
        propertyInfo.SetValue(_parent, _value, new object[] { _index });
    }
}
