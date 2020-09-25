using System;
using System.Collections.Generic;

public static class TBroadCaster<TEnum>
{
    static Dictionary<TEnum, MessageBase> m_Messages = new Dictionary<TEnum, MessageBase>();
    public static void Clear() => m_Messages.Clear();
    #region Messages Get
    static BroadCastMessage GetMessage(TEnum identity)
    {
        BroadCastMessage message;
        if (!m_Messages.ContainsKey(identity))
        {
            message = new BroadCastMessage();
            m_Messages.Add(identity, message);
        }
        else
        {
            message = (m_Messages[identity] as BroadCastMessage);
            if (message == null)
                throw new Exception("Wrong Message Type Of"+identity);
        }
        return message;
    }

    static BroadCastMessage<T> GetMessage<T>(TEnum identity)
    {
        BroadCastMessage<T> message;
        if (!m_Messages.ContainsKey(identity))
        {
            message = new BroadCastMessage<T>();
            m_Messages.Add(identity, message);
        }
        else
        {
            message = (m_Messages[identity] as BroadCastMessage<T>);
            if (message == null)
                throw new Exception("Wrong Message Type Of" + identity + "," + typeof(T));
        }
        return message;
    }
    static BroadCastMessage<T, Y> GetMessage<T, Y>(TEnum identity)
    {
        BroadCastMessage<T, Y> message;
        if (!m_Messages.ContainsKey(identity))
        {
            message = new BroadCastMessage<T, Y>();
            m_Messages.Add(identity, message);
        }
        else
        {
            message = (m_Messages[identity] as BroadCastMessage<T, Y>);
            if (message == null)
                throw new Exception("Wrong Message Type Of" + identity + "," + typeof(T) + "," + typeof(Y));
        }
        return message;
    }
    static BroadCastMessage<T, Y,U> GetMessage<T, Y, U>(TEnum identity)
    {
        BroadCastMessage<T, Y, U> message;
        if (!m_Messages.ContainsKey(identity))
        {
            message = new BroadCastMessage<T, Y, U>();
            m_Messages.Add(identity, message);
        }
        else
        {
            message = (m_Messages[identity] as BroadCastMessage<T, Y, U>);
            if (message == null)
                throw new Exception("Wrong Message Type Of" + identity + "," + typeof(T) + "," + typeof(Y)+","+typeof(U));
        }
        return message;
    }
    static BroadCastMessage<T, Y, U, I> GetMessage<T, Y, U, I>(TEnum identity)
    {
        BroadCastMessage<T, Y, U, I> message;
        if (!m_Messages.ContainsKey(identity))
        {
            message = new BroadCastMessage<T, Y, U, I>();
            m_Messages.Add(identity, message);
        }
        else
        {
            message = (m_Messages[identity] as BroadCastMessage<T, Y, U, I>);
            if (message == null)
                throw new Exception("Wrong Message Type Of" + identity + "," + typeof(T) + "," + typeof(Y)+","+typeof(U)+","+typeof(I));
        }
        return message;
    }
    #endregion
    #region Message Helper
    public static void Add(TEnum identity, Action message) => GetMessage(identity).Add(message);
    public static void Add<T>(TEnum identity, Action<T> message) => GetMessage<T>(identity).Add(message);
    public static void Add<T, Y>(TEnum identity, Action<T,Y> message) => GetMessage< T,Y>(identity).Add(message);
    public static void Add<T, Y, U>(TEnum identity, Action<T,Y,U> message) => GetMessage<T, Y, U>(identity).Add(message);
    public static void Add<T, Y, U, I>(TEnum identity, Action<T,Y,U,I> message) => GetMessage<T, Y, U, I>(identity).Add(message);
    public static void Remove(TEnum identity, Action message) => GetMessage(identity).Remove(message);
    public static void Remove<T>(TEnum identity, Action<T> message) => GetMessage<T>(identity).Remove(message);
    public static void Remove<T, Y>(TEnum identity, Action<T, Y> message) => GetMessage<T, Y>(identity).Remove(message);
    public static void Remove<T, Y, U>(TEnum identity, Action<T, Y, U> message) => GetMessage<T, Y, U>(identity).Remove(message);
    public static void Remove<T, Y, U, I>(TEnum identity, Action<T, Y, U, I> message) => GetMessage<T, Y, U, I>(identity).Remove(message);
    public static void Trigger(TEnum identity) => GetMessage(identity).Trigger();
    public static void Trigger<T>(TEnum identity, T template1) => GetMessage<T>(identity).Trigger(template1);
    public static void Trigger<T,Y>(TEnum identity, T template1,Y template2) => GetMessage<T,Y>(identity).Trigger(template1,template2);
    public static void Trigger<T,Y,U>(TEnum identity, T template1, Y template2,U template3) => GetMessage<T,Y,U>(identity).Trigger(template1,template2,template3);
    public static void Trigger<T,Y,U,I>(TEnum identity, T template1, Y template2,U template3,I template4) => GetMessage<T,Y,U,I>(identity).Trigger(template1,template2,template3,template4);
    #endregion
}

#region Messages
internal interface IBroadCastMessage<T>
{
    List<T> m_MessageList { get; }
}

internal static class IBroadCastMesasge_Extend
{
    public static int Count(this IBroadCastMessage<Action> messages) => messages.m_MessageList.Count;
    public static void Add<T>(this IBroadCastMessage<T> messages, T message)
    {
        if (messages.m_MessageList.Contains(message))
            throw new Exception("Message Already Registed!" + message);
        messages.m_MessageList.Add(message);
    }

    public static void Remove<T>(this IBroadCastMessage<T> messages, T message)
    {
        if (!messages.m_MessageList.Contains(message))
            throw new Exception("Message Not Registed!" + message);
        messages.m_MessageList.Remove(message);
    }
}
public class MessageBase { }
public class BroadCastMessage : MessageBase, IBroadCastMessage<Action>
{
    public List<Action> m_MessageList { get; } = new List<Action>();
    public void Trigger()
    {
        foreach (Action message in m_MessageList)
            message();
    }
}
public class BroadCastMessage<T> : MessageBase, IBroadCastMessage<Action<T>>
{
    public List<Action<T>> m_MessageList { get; } = new List<Action<T>>();
    public void Trigger(T template)
    {
        foreach (Action<T> message in m_MessageList)
            message(template);
    }
}
public class BroadCastMessage<T, Y> : MessageBase, IBroadCastMessage<Action<T, Y>>
{
    public List<Action<T, Y>> m_MessageList { get; } = new List<Action<T, Y>>();
    public void Trigger(T template1, Y template2)
    {
        foreach (Action<T, Y> message in m_MessageList)
            message(template1, template2);
    }
}
public class BroadCastMessage<T, Y, U> : MessageBase, IBroadCastMessage<Action<T, Y, U>>
{
    public List<Action<T, Y, U>> m_MessageList { get; } = new List<Action<T, Y, U>>();
    public void Trigger(T template1, Y template2, U template3)
    {
        foreach (Action<T, Y, U> message in m_MessageList)
            message(template1, template2, template3);
    }
}
public class BroadCastMessage<T, Y, U, I> : MessageBase, IBroadCastMessage<Action<T, Y, U, I>>
{
    public List<Action<T, Y, U, I>> m_MessageList { get; } = new List<Action<T, Y, U, I>>();
    public void Trigger(T template1, Y template2, U template3, I template4)
    {
        foreach (Action<T, Y, U, I> message in m_MessageList)
            message(template1, template2, template3, template4);
    }
}
#endregion
