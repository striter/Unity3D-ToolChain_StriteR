using System.Collections.Generic;
using Runtime.Scripting;

namespace Runtime.DataStructure
{
    public class MultiHashMap<Key, Value>
    {
        private List<Key> m_Keys = new();
        private List<Value> m_Values = new List<Value>();
        private Dictionary<Key, List<Value>> m_Map = new Dictionary<Key, List<Value>>();
        private ListPool<Value> kGridElementPool = new ();
        public void Dispose()
        {
            m_Map = null;
            m_Keys = null;
            m_Values = null;
            kGridElementPool.Dispose();
            kGridElementPool = null;
        }
        
        public IList<Value> Values => m_Values;
        public IList<Key> Keys => m_Keys;
        
        public void Add(Key _key, Value _value)
        {
            m_Values.Add(_value);
            if (!m_Map.TryGetValue(_key,out var valueList))
            {
                m_Keys.Add(_key);
                valueList = kGridElementPool.Spawn();
                m_Map.Add(_key,valueList);
            }
            valueList.Add(_value);
        }

        public void Remove(Key _key, Value _value)
        {
            if (!m_Map.TryGetValue(_key,out var elements))
                return;

            m_Values.Remove(_value);
            elements.Remove(_value);
            
            if (elements.Count != 0) 
                return;
            elements.Clear();
            kGridElementPool.Despawn(elements);
            
            m_Keys.Remove(_key);
            m_Map.Remove(_key);
        }

        public virtual void Clear()
        {
            foreach (var elementList in m_Map.Values)
            {
                elementList.Clear();
                kGridElementPool.Despawn(elementList);
            }
            
            m_Values.Clear();
            m_Keys.Clear();
            m_Map.Clear();
        }
        
        public List<Value> this[Key _key] => m_Map[_key];

        public bool TryGetValues(Key _key, out List<Value> _values)
        {
            if (!m_Map.ContainsKey(_key))
            {
                _values = null;
                return false;
            }

            _values = m_Map[_key];
            return true;
        }

        public IEnumerable<Value> GetValues(params Key[] _values) => GetValues((IEnumerable<Key>)_values);
        public IEnumerable<Value> GetValues(IEnumerable<Key> _values)
        {
            foreach (var key in _values)
            {
                if (!m_Map.ContainsKey(key))
                    continue;

                var elements = m_Map[key];
                foreach (var element in elements)
                    yield return element;
            }
        }
    }
}