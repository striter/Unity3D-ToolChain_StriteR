
using System.Collections.Generic;
using System.Linq.Extensions;

public static class USearching
{
    public static IEnumerable<T> BFS<T>(this IGraph<T> _graph, T _start)
    {
        if(_graph.Count == 0)
            yield break;
        
        var queue = UQueue.Empty<T>();
        var visited = UHashSet.Empty<T>();
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

    public static IEnumerable<T> DFS<T>(this IGraph<T> _graph, T _start)
    {
        if(_graph.Count == 0)
            yield break;

        var stack = UStack.Empty<T>();
        var visited = UHashSet.Empty<T>();
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
