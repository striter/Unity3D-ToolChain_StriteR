using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Mathematics;
using UnityEngine;

namespace UnityEditor.Extensions
{
    public enum EButtonParameters
    {
        NotSupported,
        String,
        Float,
        Integer,
        Vector3,
        Object,
    }

    public class ParameterData
    {
        public Type type;
        public string name;
        public object value;
    }
    public struct ButtonAttributeData
    {
        public InspectorButtonAttribute attribute;
        public MethodInfo method;
        public ParameterData[] parameters;
    }

    public static class UInspectorExtension
    {
        private static Dictionary<Type, EButtonParameters> kTypeLookupTable = new Dictionary<Type, EButtonParameters>()
        {
            {typeof(string),EButtonParameters.String},
            {typeof(float),EButtonParameters.Float},
            {typeof(int),EButtonParameters.Integer},
            {typeof(float3),EButtonParameters.Vector3},
            {typeof(Vector3),EButtonParameters.Vector3},
        };

        public static EButtonParameters LookUp(Type _type)
        {
            if(_type.IsSubclassOf(typeof(UnityEngine.Object)))
                return EButtonParameters.Object;
            
            return kTypeLookupTable.GetValueOrDefault(_type, EButtonParameters.NotSupported);
        }

        public static List<ButtonAttributeData> GetInspectorMethods(object _target)
        {
            List<ButtonAttributeData> kClickMethods = new();
            foreach (var (method,attribute) in _target.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Select(p=>(p,p.GetCustomAttribute<InspectorButtonAttribute>(true))))
            {
                if (attribute == null)
                    continue;

                var parameters = method.GetParameters();
                var buttonData = new ButtonAttributeData()
                {
                    attribute = attribute,
                    parameters = new ParameterData[parameters.Length],
                    method = method,
                };

                for (var i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];
                    buttonData.parameters[i] = new ParameterData()
                    {
                        type = parameter.ParameterType,
                        name = parameter.Name,
                        value = parameter.HasDefaultValue ? parameter.DefaultValue : (parameter.ParameterType.IsClass ? null : Activator.CreateInstance(parameter.ParameterType)),
                    };
                }
                kClickMethods.Add(buttonData); 
            }

            return kClickMethods;
        }
    }
}