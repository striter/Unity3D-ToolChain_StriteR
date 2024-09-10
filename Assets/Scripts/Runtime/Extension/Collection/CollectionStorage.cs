using System.Collections.Generic;

namespace System.Linq.Extensions
{
    public static class UList
    {
        static class Storage<T> { public static List<T> m_List = new List<T>(); }
        
        public static List<T> Empty<T>()
        {
            var list = Storage<T>.m_List;
            list.Clear();
            return list;
        }
        public static List<T> Traversal<T>(IEnumerable<T> _src)
        {
            var list = Storage<T>.m_List;
            list.Clear();
            list.AddRange(_src);
            return list;
        }
    }

}