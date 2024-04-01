using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using System.Reflection;
using Dome.Entity;
using UnityEngine;

namespace Dome
{
    public interface IFireEvent
    {
        public object[] kDefaultParameters { get; }
    }

    public static class IEventFire_Extension
    {
        static Dictionary<Type,Dictionary<string,List<MethodInfo>>> kInterfaceLookup = new();
        static IEventFire_Extension()
        {
            List<Type> extensions = new List<Type>();
            List<Type> staticTypes = Assembly.GetExecutingAssembly().GetTypes().Collect(p => p.IsStatic()).ToList();
            foreach (var childType in typeof(ADomeEntity).GetChildTypes())
            {
                var lookUp = new Dictionary<string, List<MethodInfo>>();
                kInterfaceLookup.Add(childType,lookUp);
                
                extensions.Clear();
                foreach (var extension in childType.GetInterfaces())
                    extensions.TryAdd(extension);   
                
                foreach (var type in staticTypes)
                {
                    foreach (var method in type.GetMethods())
                    {
                        var parameters = method.GetParameters();
                        if(parameters.Length<=0) continue;
                        
                        var interfaceType = parameters[0].ParameterType;
                        if(!extensions.Any(p=>p==interfaceType)) continue;

                        var methodName = method.Name;
                        if (!lookUp.TryGetValue(methodName, out var methods))
                        {
                            methods = new List<MethodInfo>();
                            lookUp.Add(methodName,methods);
                        }
                    
                        methods.Add(method);
                    }
                }
            }
        }

        public static void FireEvents(this IFireEvent _this,string _eventName, params object[] _parameters)
        {
            _parameters = _parameters == null ? _this.kDefaultParameters : _this.kDefaultParameters.Add(_parameters);
            if (!kInterfaceLookup[_this.GetType()].TryGetValue(_eventName,out var methodsToInvoke)) return;
            foreach (var method in methodsToInvoke)
            {
                try
                {
                    method.Invoke(null, _parameters);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"{method.DeclaringType}:{method.Name}");
                    throw e;
                }
            }
        }
    }
}