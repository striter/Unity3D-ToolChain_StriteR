using System;
using System.Collections.Generic;
public class TBroadCaster<TEnum>
{      //Message Center  Add / Remove / Trigger

    private static Dictionary<TEnum, List<LocalMessage>> dic_delegates = new Dictionary<TEnum, List<LocalMessage>>();
    public static void Init()
    {
        dic_delegates.Clear();
        TCommon.TraversalEnum((TEnum temp) => { dic_delegates.Add(temp, new List<LocalMessage>()); });
    }

    public static void Add(TEnum type, Action Listener)
    {
        if (dic_delegates.Count == 0)
            UnityEngine.Debug.LogError("Please Init Broadcaster Before Add");

        dic_delegates[type].Add(new LocalMessage(Listener));
    }
    public static void Remove(TEnum type, Action Listener)
    {
        LocalMessage target = null;
        foreach (LocalMessage mb in dic_delegates[type])
        {
            if (mb.e_type == LocalMessage.enum_MessageType.Void && mb.ac_listener == Listener)
            {
                target = mb;
                break;
            }
        }
        if (target != null)
            dic_delegates[type].Remove(target);
    }
    public static void Trigger(TEnum type)
    {
        bool triggered = false;
        foreach (LocalMessage del in dic_delegates[type])
        {
            if (del.e_type == LocalMessage.enum_MessageType.Void)
            {
                triggered = true;
                del.Trigger();
            }
        }
        if (!triggered)
            UnityEngine.Debug.LogWarning("No Message Triggered By:" + type.ToString());
    }

    public static void Add<T>(TEnum type, Action<T> Listener)
    {
        if (dic_delegates.Count == 0)
            UnityEngine.Debug.LogError("Please Init Broadcaster Before Add");

        dic_delegates[type].Add(new LocalMessage<T>(Listener));
    }
    public static void Remove<T>(TEnum type, Action<T> Listener)
    {
        LocalMessage<T> removeTarget = null;
        foreach (LocalMessage mb in dic_delegates[type])
        {
            if (mb.e_type == LocalMessage.enum_MessageType.OneTemplate)
                removeTarget = mb as LocalMessage<T>;
            if (removeTarget != null && removeTarget.ac_listener == Listener)
                break;
            removeTarget = null;
        }

        if (removeTarget != null)
            dic_delegates[type].Remove(removeTarget);
    }
    public static void Trigger<T>(TEnum type, T template)
    {
        bool triggered = false;
        foreach (LocalMessage del in dic_delegates[type])
        {
            if (del.e_type != LocalMessage.enum_MessageType.OneTemplate)
                return;

            triggered = true;
            (del as LocalMessage<T>).Trigger(template);
        }

        if (!triggered)
            UnityEngine.Debug.LogWarning("No Message Triggered By:"+type.ToString()+"|"  + typeof(T).ToString());
    }

    public static void Add<T, Y>(TEnum type, Action<T, Y> Listener)
    {
        if (dic_delegates.Count == 0)
            UnityEngine.Debug.LogError("Please Init Broadcaster Before Add");

        dic_delegates[type].Add(new LocalMessage<T, Y>(Listener));
    }

    public static void Remove<T, Y>(TEnum type, Action<T, Y> Listener)
    {
        LocalMessage<T, Y> removeTarget = null;
        foreach (LocalMessage mb in dic_delegates[type])
        {
            if (mb.e_type == LocalMessage.enum_MessageType.TwoTemplate)
                removeTarget = mb as LocalMessage<T, Y>;
            if (removeTarget != null && removeTarget.ac_listener == Listener)
                break;
            removeTarget = null;
        }

        if (removeTarget != null)
            dic_delegates[type].Remove(removeTarget);
    }
    public static void Trigger<T, Y>(TEnum type, T template1, Y template2)
    {
        bool triggered = false;
        foreach (LocalMessage del in dic_delegates[type])
        {
            if (del.e_type != LocalMessage.enum_MessageType.TwoTemplate)
                return;

            triggered = true;
            (del as LocalMessage<T,Y>).Trigger(template1,template2);
        }

        if (!triggered)
            UnityEngine.Debug.LogWarning("No Message Triggered By:" + type.ToString() + "|" + typeof(T) .ToString() + "|" + typeof(Y).ToString());
    }

    public static void Add<T, Y, U>(TEnum type, Action<T, Y, U> Listener)
    {
        if (dic_delegates.Count == 0)
            UnityEngine.Debug.LogError("Please Init Broadcaster Before Add");

        dic_delegates[type].Add(new LocalMessage<T, Y, U>(Listener));
    }
    public static void Remove<T, Y, U>(TEnum type, Action<T, Y, U> Listener)
    {
        LocalMessage<T, Y, U> removeTarget = null;
        foreach (LocalMessage mb in dic_delegates[type])
        {
            if (mb.e_type == LocalMessage.enum_MessageType.ThreeTemplate)
                removeTarget = mb as LocalMessage<T, Y, U>;
            if (removeTarget != null && removeTarget.ac_listener == Listener)
                break;
            removeTarget = null;
        }

        if (removeTarget != null)
            dic_delegates[type].Remove(removeTarget);
    }
    public static void Trigger<T, Y, U>(TEnum type, T template1, Y template2, U template3)
    {
        bool triggered = false;
        foreach (LocalMessage del in dic_delegates[type])
        {
            if (del.e_type != LocalMessage.enum_MessageType.ThreeTemplate)
                return;
            triggered = true;
            (del as LocalMessage<T, Y, U>).Trigger(template1, template2, template3);
        }

        if (!triggered)
            UnityEngine.Debug.LogWarning("No Message Triggered By:" + type.ToString() + "|" + typeof(T).ToString() + "|" + typeof(Y).ToString() + "|" + typeof(U).ToString());
    }
}

#region LocalMessages
public class LocalMessage
{
    public enum enum_MessageType
    {
        Void,
        OneTemplate,
        TwoTemplate,
        ThreeTemplate,
    }
    public virtual enum_MessageType e_type => enum_MessageType.Void;
    public Action ac_listener { get; private set; }
    public LocalMessage(Action listener)
    {
        ac_listener = listener;
    }
    public void Trigger()
    {
        ac_listener.Invoke();
    }

    public LocalMessage()
    {
    }
}
public class LocalMessage<T> : LocalMessage
{
    public override enum_MessageType e_type => enum_MessageType.OneTemplate;
    public new Action<T> ac_listener { get; private set; }
    public LocalMessage(Action<T> _listener)
    {
        ac_listener = _listener;
    }
    public void Trigger(T _object)
    {
        ac_listener(_object);

    }
}
public class LocalMessage<T, Y> : LocalMessage
{
    public override enum_MessageType e_type => enum_MessageType.TwoTemplate;
    public new Action<T, Y> ac_listener { get; private set; }
    public LocalMessage(Action<T, Y> _listener)
    {
        ac_listener = _listener;
    }
    public void Trigger(T _object1, Y _object2)
    {
        ac_listener(_object1, _object2);
    }
}
public class LocalMessage<T, Y,U> : LocalMessage
{
    public override enum_MessageType e_type => enum_MessageType.ThreeTemplate;
    public new Action<T, Y,U> ac_listener { get; private set; }
    public LocalMessage(Action<T, Y,U> _listener)
    {
        ac_listener = _listener;
    }
    public void Trigger(T _object1, Y _object2,U _object3)
    {
        ac_listener(_object1, _object2,_object3);
    }
}
public class LocalMessage<T, Y, U,I> : LocalMessage
{
    public override enum_MessageType e_type => enum_MessageType.ThreeTemplate;
    public new Action<T, Y, U> ac_listener { get; private set; }
    public LocalMessage(Action<T, Y, U> _listener)
    {
        ac_listener = _listener;
    }
    public void Trigger(T _object1, Y _object2, U _object3)
    {
        ac_listener(_object1, _object2, _object3);
    }
}
#endregion

