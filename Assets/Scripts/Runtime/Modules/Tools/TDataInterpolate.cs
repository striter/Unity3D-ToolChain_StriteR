using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public interface IInterpolate<T>
{
    T Interpolate(T _dst, float _interpolate);
}

public static class TDataInterpolate
{
    private static readonly Dictionary<Type, Func<object, object, float, object>> kBasicInterpolations =
        new Dictionary<Type, Func<object, object, float, object>>
        {
            {typeof(int), (_value1,_value2,_interpolate)=>UMath.IntLerp((int)_value1,(int)_value2,_interpolate)},
            {typeof(bool), (_value1,_value2,_interpolate)=>UMath.BoolLerp((bool)_value1,(bool)_value2,_interpolate)},
            {typeof(float), (_value1,_value2,_interpolate)=>Mathf.Lerp((float)_value1,(float)_value2,_interpolate)},
            {typeof(Vector2), (_value1,_value2,_interpolate)=>Vector2.Lerp((Vector2)_value1,(Vector2)_value2,_interpolate)},
            {typeof(Vector3), (_value1,_value2,_interpolate)=>Vector3.Lerp((Vector3)_value1,(Vector3)_value2,_interpolate)},
            {typeof(Vector4), (_value1,_value2,_interpolate)=>Vector4.Lerp((Vector4)_value1,(Vector4)_value2,_interpolate)},
            {typeof(Color), (_value1,_value2,_interpolate)=>Color.Lerp((Color)_value1,(Color)_value2,_interpolate)},
        };

    private static readonly Dictionary<Type, FieldInfo[]> kFieldInfos = new Dictionary<Type, FieldInfo[]>();
    
    private static readonly Type kInterpolate = typeof(IInterpolate<>);
    private static readonly Dictionary<Type, MethodInfo> kInterpolateMethod = new Dictionary<Type, MethodInfo>();

    public static T Interpolate<T>(T _src, T _dst, float _interpolate) where T:struct=> (T)Interpolate(typeof(T),_src,_dst,_interpolate);
    static object Interpolate(Type _type, object _src, object _dst, float _interpolate)
    {
        if (kBasicInterpolations.ContainsKey(_type))
            return kBasicInterpolations[_type](_src, _dst, _interpolate);

        if(kInterpolateMethod.ContainsKey(_type))
            return kInterpolateMethod[_type].Invoke(_src,new[]{_dst,_interpolate});
        
        if (_type.IsGenericType&&_type.GetGenericTypeDefinition() == kInterpolate)
        {
            var interpolateMethod = _type.GetMethod(nameof(IInterpolate<object>.Interpolate),BindingFlags.Public | BindingFlags.DeclaredOnly);
            kInterpolateMethod.Add(_type,interpolateMethod);
            return interpolateMethod.Invoke(_src, new[] { _dst, _interpolate });
        }

        if (_type.IsValueType)
        {
            if(!kFieldInfos.ContainsKey(_type))
                kFieldInfos.Add(_type,_type.GetAllFields( BindingFlags.Public| BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Instance).ToArray());

            object value = Activator.CreateInstance(_type);
            foreach (var field in kFieldInfos[_type])
                field.SetValue(value,Interpolate( field.FieldType,field.GetValue(_src),field.GetValue(_dst),_interpolate));
            return value;
        }

        throw new Exception("Invalid Type To Interpolate:"+_type.Name);
    }
}
