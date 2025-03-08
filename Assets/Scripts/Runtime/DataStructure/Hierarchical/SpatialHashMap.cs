using System;
using System.Collections;
using System.Collections.Generic;

namespace Runtime.DataStructure
{
    public class SpatialHashMap<Key,Element> : IEnumerable<Key> where Key:struct 
    {
        private MultiHashMap<Key,  int> m_Map = new();
        private Func<Element, Key> m_ConvertFunc;

        private List<Element> kQueryResults = new();
        private SpatialHashMap() { }
        public SpatialHashMap(Func<Element, Key> _convertFunc) {
            m_ConvertFunc = _convertFunc;
        }

        public void Clear()
        {
            m_Map.Clear();
            kQueryResults.Clear();
        }
        
        public void Dispose()
        {
            Clear();
            m_Map.Dispose();
            m_Map = null;
        }

        public void Construct(IList<Element> _values)
        {
            Clear();
            for(var i = _values.Count - 1; i >= 0; i--)
                m_Map.Add(m_ConvertFunc(_values[i]),i);
        }

        public IList<Element> Query(Predicate<Key> _predicateKey,IList<Element> _values)
        {
            kQueryResults.Clear();
            for(var i = m_Map.Keys.Count - 1 ; i >= 0 ;i--)
            {
                var key = m_Map.Keys[i];
                if(!_predicateKey(key))
                    continue;
                foreach (var valueIndex in m_Map[key])
                    kQueryResults.Add(_values[valueIndex]);
            }

            return kQueryResults;
        }

        public IList<Element> Query(Predicate<Key> _predicateKey,Predicate<Element> _predicateValue,IList<Element> _values)
        {
            var result = Query(_predicateKey, _values);
            for (var i = result.Count - 1; i >= 0; i--)
                if(!_predicateValue(result[i]))
                    result.RemoveAt(i);
            return result;
        }

        public IEnumerator<Key> GetEnumerator() => m_Map.Keys.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IList<Key> Keys => m_Map.Keys;
    }
}