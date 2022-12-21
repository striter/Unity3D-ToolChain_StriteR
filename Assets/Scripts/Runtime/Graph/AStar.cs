using System.Collections.Generic;

public static class UAStar<T> where T:struct
{
    private static PriorityQueue<T, float> frontier = new PriorityQueue<T, float>();
    private static Dictionary<T, T> previousLink = new Dictionary<T, T>();
    private static Dictionary<T, float> pathCosts = new Dictionary<T, float>();
    public static void PathFind<Graph>(Graph _graph, INode<T> _src, INode<T> _tar,ref Stack<T> _outputPaths) where Graph:IGraph<T>,IGraphPathFinding<T>
    {
        frontier.Clear();
        pathCosts.Clear();
        previousLink.Clear();

        var start = _src.identity;
        frontier.Enqueue(start,0);
        pathCosts.Add(start,0);
        
        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            if (current.Equals(_tar.identity))
                break;

            var curCost = pathCosts[current];
            foreach (var next in _graph.GetAdjacentNodes(current))
            {
                var cost = _graph.Cost(current,next);
                var newCost = curCost + cost;
                bool contains = pathCosts.ContainsKey(next);
                if (!contains)
                {
                    pathCosts.Add(next,newCost);
                    previousLink.Add(next,current);
                }
                
                if (!contains || newCost < pathCosts[next])
                {
                    pathCosts[next] = newCost;
                    frontier.Enqueue(next,curCost+cost + _graph.Heuristic(next,_tar.identity));
                    previousLink[next] = current;
                }
            }
        }

        var goal = _tar.identity;
        _outputPaths.Clear();
        while (previousLink.ContainsKey(goal))
        {
            _outputPaths.Push(goal);
            goal = previousLink[goal];
        }
        _outputPaths.Push(start);
    }
}