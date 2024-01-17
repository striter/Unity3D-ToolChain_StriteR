using System.Collections.Generic;
using Runtime.DataStructure;

public static class UAStar<T>
{
    public static PriorityQueue<T, float> frontier = new PriorityQueue<T, float>();
    public static Dictionary<T, T> previousLink = new Dictionary<T, T>();
    public static Dictionary<T, float> pathCosts = new Dictionary<T, float>();
}

public static class AStar_Extension
{
    public static void PathFind<T,Graph>(this Graph _graph, T _src, T _tar,Stack<T> _outputPaths) where Graph:IGraph<T>,IGraphPathFinding<T>
    {
        UAStar<T>.frontier.Clear();
        UAStar<T>.pathCosts.Clear();
        UAStar<T>.previousLink.Clear();

        var start = _src;
        UAStar<T>.frontier.Enqueue(start,0);
        UAStar<T>.pathCosts.Add(start,0);
        
        while ( UAStar<T>.frontier.Count > 0)
        {
            var current =  UAStar<T>.frontier.Dequeue();
            if (current.Equals(_tar))
                break;

            var curCost =  UAStar<T>.pathCosts[current];
            foreach (var next in _graph.GetAdjacentNodes(current))
            {
                var cost = _graph.Cost(current,next);
                var newCost = curCost + cost;
                bool contains =  UAStar<T>.pathCosts.ContainsKey(next);
                if (!contains)
                {
                    UAStar<T>.pathCosts.Add(next,newCost);
                    UAStar<T>.previousLink.Add(next,current);
                }
                
                if (!contains || newCost <  UAStar<T>.pathCosts[next])
                {
                    UAStar<T>.pathCosts[next] = newCost;
                    UAStar<T>.frontier.Enqueue(next,curCost+cost + _graph.Heuristic(next,_tar));
                    UAStar<T>.previousLink[next] = current;
                }
            }
        }

        var goal = _tar;
        _outputPaths.Clear();
        while ( UAStar<T>.previousLink.ContainsKey(goal))
        {
            _outputPaths.Push(goal);
            goal =  UAStar<T>.previousLink[goal];
        }
        _outputPaths.Push(start);
    }
}