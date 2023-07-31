using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

public static class UReflection
{
    public static void CopyFields<T>(T _src, T _dst) where T : class
    {
        foreach (var fieldInfo in _src.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
            fieldInfo.SetValue(_dst,fieldInfo.GetValue(_src));
    }
    
    public static T CreateInstance<T>(Type t,params object[] constructorArgs) => (T)Activator.CreateInstance(t, constructorArgs);
    public static void TraversalAllInheritedClasses<T>(Action<Type> OnEachClass)
    {
        Type[] allTypes = Assembly.GetExecutingAssembly().GetTypes();
        Type parentType = typeof(T);
        for (int i = 0; i < allTypes.Length; i++)
        {
            if (allTypes[i].IsClass && !allTypes[i].IsAbstract && allTypes[i].IsSubclassOf(parentType))
                OnEachClass(allTypes[i]);
        }
    }
    public static void TraversalAllInheritedClasses<T>(Action<Type, T> OnInstanceCreated, params object[] constructorArgs) => TraversalAllInheritedClasses<T>(type=> OnInstanceCreated(type, CreateInstance<T>(type, constructorArgs)));
    public static void InvokeAllMethod<T>(List<Type> classes,string methodName,T template) where T:class
    {
        foreach (Type t in classes)
        {
            MethodInfo method = t.GetMethod(methodName);
            if (method == null)
                throw new Exception("Null Method Found From:"+t.ToString()+"."+methodName);
            method.Invoke(null,new object[] {template });
        }
    }
    public static Stack<Type> GetInheritTypes(this Type _type)
    {
        if (_type == null)
            throw new NullReferenceException();

        Stack<Type> inheritStack = new Stack<Type>();
        while (_type.BaseType != null)
        {
            _type = _type.BaseType;
            inheritStack.Push(_type);
        }
        return inheritStack;
    }

    public static IEnumerable<Type> GetChildTypes(this Type _baseType)=>
        Assembly.GetCallingAssembly().GetTypes().Where(_type => _type.IsSubclassOf(_baseType) );
    
    public static object GetValue(this Stack<FieldInfo> _fieldStacks, object _targetObject)
    {
        object dstObject = _targetObject;
        foreach(var field in _fieldStacks)
            dstObject = field.GetValue(dstObject);
        return dstObject;
    }
    public static void SetValue(this Stack<FieldInfo> _fieldStacks, object _targetObject, object _value)
    {
        Stack<object> dstObjects = new Stack<object>();
        object dstObject = _targetObject;
        int totalCount = _fieldStacks.Count;
        int fieldCount = totalCount;
        foreach (var field in _fieldStacks)
        {
            if (--fieldCount == 0)
                break;
            dstObject = field.GetValue(dstObject);
            dstObjects.Push(dstObject);
        }
        dstObject = _value;
        for (int i = totalCount-1; i >=0; i--)
        {
            FieldInfo field = _fieldStacks.ElementAt(i);
            object tarObject = dstObjects.Count==0?_targetObject:dstObjects.Pop();
            field.SetValue(tarObject, dstObject);
            dstObject = tarObject;
        }
    }
    public static IEnumerable<FieldInfo> GetAllFields(this Type _type,BindingFlags _flags)
    {
        if (_type == null)
            throw new NullReferenceException();

        foreach (var fieldInfo in _type.GetFields(_flags | BindingFlags.Public | BindingFlags.NonPublic))
            yield return fieldInfo;
        var inheritStack = _type.GetInheritTypes();
        while(inheritStack.Count>0)
        {
            var type = inheritStack.Pop();
            foreach (var fieldInfo in type.GetFields(_flags | BindingFlags.NonPublic))
                if(fieldInfo.IsPrivate)
                    yield return fieldInfo;
        }
    }
    public static IEnumerable<MethodInfo> GetAllMethods(this Type _type, BindingFlags _flags)
    {
        if (_type == null)
            throw new NullReferenceException();

        foreach (var methodInfo in _type.GetMethods(_flags | BindingFlags.Public | BindingFlags.NonPublic))
            yield return methodInfo;
        var inheritStack = _type.GetInheritTypes();
        while (inheritStack.Count > 0)
        {
            var type = inheritStack.Pop();
            foreach (var methodInfo in type.GetMethods(_flags | BindingFlags.NonPublic))
                if (methodInfo.IsPrivate)
                    yield return methodInfo;
        }
    }
}
