using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
[AttributeUsage(AttributeTargets.Method)]
public class PartialMethodAttribute : Attribute
{
    public object m_Trigger;
    public object m_Sorting;
    public PartialMethodAttribute(object _trigger, object _sorting)
    {
        if (!_trigger.GetType().IsEnum)
            throw new Exception("Trigger For Enum Field Only!");

        Type _sortingType = _sorting.GetType();
        if (!_sortingType.IsValueType)
            throw new Exception("Sorting For Value Type Only!");

        m_Trigger = _trigger;
        m_Sorting = _sorting;
    }
}
public interface IPartialMethods<T, Y> where T : Enum where Y : struct { }
public static class IParticalMethods_Helper 
{
    static readonly Type s_ParticalAttribute = typeof(PartialMethodAttribute);
    static Dictionary<Type, Dictionary<Enum, MethodInfo[]>> s_CollectedMethods=new Dictionary<Type, Dictionary<Enum, MethodInfo[]>>();
    public static void InitMethods<T,Y>(this IPartialMethods<T,Y> _target) where T : Enum where Y : struct
    {
        Type targetType = _target.GetType();
        if (s_CollectedMethods.ContainsKey(targetType))
            return;
        Type triggerType = typeof(T);
        Type sortingType = typeof(Y);
        var triggerMethods = targetType.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic).TakeAll(p => {
            var attribute = (PartialMethodAttribute)p.GetCustomAttribute(s_ParticalAttribute);
            if (attribute == null)
                return null;
            if (attribute.m_Trigger.GetType() != triggerType)
                throw new Exception("Trigger Type Mismatch:"+attribute.m_Trigger+" "+ triggerType);
            if (attribute.m_Sorting.GetType() != sortingType)
                throw new Exception("Sorting Type Mismatch:" + attribute.m_Sorting + " " + sortingType);

            return new Ref<KeyValuePair<MethodInfo, PartialMethodAttribute>>(new KeyValuePair<MethodInfo, PartialMethodAttribute>(p, attribute));
        }).Select(p => p.m_RefValue).GroupBy(p=>(Enum)p.Value.m_Trigger,p=>new KeyValuePair<Y,MethodInfo>((Y)p.Value.m_Sorting , p.Key));

        Dictionary<Enum, MethodInfo[]> collectedMethods = new Dictionary<Enum, MethodInfo[]>();
        foreach (var methodGroup in triggerMethods)
            collectedMethods.Add(methodGroup.Key, methodGroup.OrderBy(p=>p.Key).Select(p=>p.Value).ToArray());
        s_CollectedMethods.Add(targetType, collectedMethods);
    }

    public static void InvokeMethods<T,Y>(this IPartialMethods<T,Y> _target,T _trigger,params object[] _objects) where T : Enum where Y:struct
    {
        Type type = _target.GetType();
        if (!s_CollectedMethods.ContainsKey(type))
            throw new Exception("Should Init Before Trigger:" + type);

        foreach (var method in s_CollectedMethods[type][_trigger])
            method.Invoke(_target,_objects);
    }
}