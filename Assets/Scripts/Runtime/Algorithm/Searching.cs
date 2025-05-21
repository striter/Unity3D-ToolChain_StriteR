
using System.Collections.Generic;
using System.Linq.Extensions;
using Runtime.Pool;

public static class USearching
{
    static readonly int kSearchingPoolKey = nameof(USearching).GetHashCode();
    public static IEnumerable<T> BFS<T>(this IGraphFinite<T> _graph, T _start)
    {
        if(_graph.Count == 0)
            yield break;
        
        var queue = PoolQueue<T>.Empty(kSearchingPoolKey);
        var visited = PoolHashSet<T>.Empty(kSearchingPoolKey);
        queue.Enqueue(_start);
        visited.Add(_start);

        var iteration =  _graph.Count;
        while (queue.Count > 0 && iteration-- > 0)
        {
            var current = queue.Dequeue();
            yield return current;
            
            foreach (var next in _graph.GetAdjacentNodes(current))
            {
                if (visited.Contains(next))
                    continue;
                queue.Enqueue(next);
                visited.Add(next);
            }
        }
    }

    public static IEnumerable<T> DFS<T>(this IGraphFinite<T> _graph, T _start)
    {
        if(_graph.Count == 0)
            yield break;

        var stack = PoolStack<T>.Empty(kSearchingPoolKey);
        var visited = PoolHashSet<T>.Empty(kSearchingPoolKey);
        stack.Push(_start);
        visited.Add(_start);
        
        var iteration =  _graph.Count;
        while (stack.Count > 0 && iteration-- > 0)
        {
            var current = stack.Pop();
            yield return current;
            
            foreach (var next in _graph.GetAdjacentNodes(current))
            {
                if (visited.Contains(next))
                    continue;
                stack.Push(next);
                visited.Add(next);
            }
        }  
    }

}
