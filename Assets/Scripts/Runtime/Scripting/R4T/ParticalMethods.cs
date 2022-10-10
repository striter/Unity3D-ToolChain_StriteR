using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

[AttributeUsage(AttributeTargets.Method)]
public class PartialMethodAttribute : Attribute
{
    public readonly object m_Trigger;
    public readonly object m_Sorting;
    public PartialMethodAttribute(object _trigger, object _sorting)
    {
        if (!_trigger.GetType().IsValueType || !_sorting.GetType().IsValueType)
            throw new Exception("Trigger For Value Type Only!");

        m_Trigger = _trigger;
        m_Sorting = _sorting;
    }
}
public interface IPartialMethods<T, Y> where T : Enum where Y : struct { }
public static class PartialMethodsHelper 
{
    static readonly Type m_PartialAttribute = typeof(PartialMethodAttribute);
    static readonly Dictionary<Type, Dictionary<Enum, MethodInfo[]>> m_MethodsCollected=new Dictionary<Type, Dictionary<Enum, MethodInfo[]>>();
    public static void InitMethods<T,Y>(this IPartialMethods<T,Y> _target) where T : Enum where Y : struct
    {
        Type targetType = _target.GetType();
        if (m_MethodsCollected.ContainsKey(targetType))
            return;
        Type triggerType = typeof(T);
        Type sortingType = typeof(Y);
        var triggerMethods = targetType.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic)
            .Select(p => (p,(PartialMethodAttribute)p.GetCustomAttribute(m_PartialAttribute))).Collect(p=>
            {
                var attribute = p.Item2;
            if (attribute == null)
                return false;
            if (attribute.m_Trigger.GetType() != triggerType)
                throw new Exception("Trigger Type Mismatch:"+attribute.m_Trigger+" "+ triggerType);
            if (attribute.m_Sorting.GetType() != sortingType)
                throw new Exception("Sorting Type Mismatch:" + attribute.m_Sorting + " " + sortingType);

            return true;
        }).GroupBy(p=>(Enum)p.Item2.m_Trigger,pair=>new KeyValuePair<Y,MethodInfo>((Y)pair.Item2.m_Sorting ,pair.p));

        Dictionary<Enum, MethodInfo[]> collectedMethods = new Dictionary<Enum, MethodInfo[]>();
        foreach (var methodGroup in triggerMethods)
            collectedMethods.Add(methodGroup.Key, methodGroup.OrderBy(p=>p.Key).Select(p=>p.Value).ToArray());
        m_MethodsCollected.Add(targetType, collectedMethods);
    }

    public static void InvokeMethods<T,Y>(this IPartialMethods<T,Y> _target,T _trigger,params object[] _objects) where T : Enum where Y:struct
    {
        Type type = _target.GetType();
        if (!m_MethodsCollected.ContainsKey(type))
            return;

        if (!m_MethodsCollected[type].ContainsKey(_trigger))
            return;
        
        foreach (var method in m_MethodsCollected[type][_trigger])
            method.Invoke(_target,_objects);
    }
}