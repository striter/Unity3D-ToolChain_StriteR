﻿using System;
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
            inActiveElements = null;
        }
        
        public T Spawn() => inActiveElements.Count > 0 ? inActiveElements.Pop() : new();

        public void Despawn(T _element) => inActiveElements.Push(_element);
    }
}