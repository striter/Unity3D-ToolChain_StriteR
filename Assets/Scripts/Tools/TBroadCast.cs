using System;
using System.Collections.Generic;

interface BroadCastMessageInterface<T>
{
    List<T> m_MessageList { get; }
}

static class BroadCastMessageInteraface_Extend
{
    public static int Count(this BroadCastMessageInterface<Action> messages) => messages.m_MessageList.Count;
    public static void Add<T>(this BroadCastMessageInterface<T> messages, T message)
    {
        if (messages.m_MessageList.Contains(message))
            throw new Exception("Message Already Registed!" + message);
        messages.m_MessageList.Add(message);
    }

    public static void Remove<T>(this BroadCastMessageInterface<T> messages, T message)
    {
        if (!messages.m_MessageList.Contains(message))
            throw new Exception("Message Not Registed!" + message);
        messages.m_MessageList.Remove(message);
    }
}

public static class TBroadCaster<TEnum>
{
    public class MessageBase { }
    public static Dictionary<TEnum, MessageBase> m_Messages = new Dictionary<TEnum, MessageBase>();
    #region Messages
    public class BroadCastMessage : MessageBase, BroadCastMessageInterface<Action>
    {
        public List<Action> m_MessageList { get; } = new List<Action>();
        public void Trigger()
        {
            foreach (Action message in m_MessageList)
                message();
        }
    }
    public class BroadCastMessage<T> : MessageBase, BroadCastMessageInterface<Action<T>>
    {
        public List<Action<T>> m_MessageList { get; } = new List<Action<T>>();
        public void Trigger(T template)
        {
            foreach (Action<T> message in m_MessageList)
                message(template);
        }
    }
    public class BroadCastMessage<T, Y> : MessageBase, BroadCastMessageInterface<Action<T, Y>>
    {
        public List<Action<T, Y>> m_MessageList { get; } = new List<Action<T, Y>>();
        public void Trigger(T template1,Y template2)
        {
            foreach (Action<T,Y> message in m_MessageList)
                message(template1,template2);
        }
    }
    public class BroadCastMessage<T, Y, U> : MessageBase, BroadCastMessageInterface<Action<T, Y, U>>
    {
        public List<Action<T, Y, U>> m_MessageList { get; } = new List<Action<T, Y, U>>();
        public void Trigger(T template1, Y template2,U template3)
        {
            foreach (Action<T, Y,U> message in m_MessageList)
                message(template1, template2,template3);
        }
    }
    public class BroadCastMessage<T, Y, U, I> : MessageBase, BroadCastMessageInterface<Action<T, Y, U, I>>
    {
        public List<Action<T, Y, U, I>> m_MessageList { get; } = new List<Action<T, Y, U, I>>();
        public void Trigger(T template1, Y template2, U template3,I template4)
        {
            foreach (Action<T, Y, U,I> message in m_MessageList)
                message(template1, template2, template3,template4);
        }
    }
    #endregion
    #region Messages Get
    public static BroadCastMessage Get(TEnum identity)
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
                throw new Exception("Null Void Message Found Of"+identity);
        }
        return message;
    }

    public static BroadCastMessage<T> Get<T>(TEnum identity)
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
                throw new Exception("Null Single Message Found Of" + identity + "," + typeof(T));
        }
        return message;
    }
    public static BroadCastMessage<T, Y> Get<T, Y>(TEnum identity)
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
                throw new Exception("Null Single Message Found Of" + identity + "," + typeof(T) + "," + typeof(Y));
        }
        return message;
    }
    public static BroadCastMessage<T, Y,U> Get<T, Y, U>(TEnum identity)
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
                throw new Exception("Null Single Message Found Of" + identity + "," + typeof(T) + "," + typeof(Y)+","+typeof(U));
        }
        return message;
    }
    public static BroadCastMessage<T, Y, U, I> Get<T, Y, U, I>(TEnum identity)
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
                throw new Exception("Null Single Message Found Of" + identity + "," + typeof(T) + "," + typeof(Y)+","+typeof(U)+","+typeof(I));
        }
        return message;
    }
    #endregion
}
