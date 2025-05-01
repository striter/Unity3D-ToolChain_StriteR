using System;
using System.Collections.Generic;

namespace Runtime.Scripting
{
    public class Pool<T> : IDisposable  where T : new()
    {
        private Stack<T> inActiveElements = new ();
        public Pool(int _initialSize = 32)
        {
            for(var i = 0; i < _initialSize;i++)
                inActiveElements.Push(new T());
        }

        public void Dispose()
        {
            inActiveElements.Clear();
        }
        
        public virtual T Spawn() => inActiveElements.Count > 0 ? inActiveElements.Pop() : new();
        public void Spawn(out T element) => element = Spawn();
        public virtual void Despawn(T _element)
        {
            if (_element == null)
                return;
            inActiveElements.Push(_element);
        }
    }

    public class ListPool<T> : Pool<List<T>>
    {
        public override void Despawn(List<T> _element)
        {
            _element?.Clear();
            base.Despawn(_element);
        }
    }
}