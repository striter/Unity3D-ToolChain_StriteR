using System.Collections.Generic;

public class MultiHashMap<Key,Value>
{
    private List<Value> m_Elements = new List<Value>();
    private Dictionary<Key,  List<Value>> m_Grids = new Dictionary<Key, List<Value>>();
    
    public List<Value> Values => m_Elements;
    public void Add(Key _key, Value _value)
    {
        m_Elements.Add(_value);
        if(!m_Grids.ContainsKey(_key))
            m_Grids.Add(_key,new List<Value>());
        m_Grids[_key].Add(_value);
    }

    public void Remove(Key _key, Value _value)
    {
        if (!m_Grids.ContainsKey(_key))
            return;
        
        m_Elements.Remove(_value);

        var elements = m_Grids[_key];
        elements.Remove(_value);
        if (elements.Count == 0)
            m_Grids.Remove(_key);
    }
    
    public List<Value> this[Key _key] => m_Grids[_key];
    public bool TryGetValues(Key _key, out List<Value> _values)
    {
        if (!m_Grids.ContainsKey(_key))
        {
            _values = null;
            return false;
        }
        
        _values = m_Grids[_key];
        return true;
    }

    public IEnumerable<Value> GetValues(params Key[] _values) => GetValues((IEnumerable<Key>)_values);
    public IEnumerable<Value> GetValues(IEnumerable<Key> _values)
    {
        foreach (var key in _values)
        {
            if (!m_Grids.ContainsKey(key))
                continue;
            
            var elements = m_Grids[key];
            foreach (var element in elements)
                yield return element;
        }
    }
}
