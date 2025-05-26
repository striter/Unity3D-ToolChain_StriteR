using System.Collections.Generic;
using Runtime.DataStructure;

namespace Runtime.PathFinding
{
    public static partial class UPathFinding
    {
        public static void AStar<T>(this IGraphPathFinding<T> _graph, T _src, T _tar,Stack<T> _outputPaths)
        {
            AStarStorage<T>.Request(out var frontier, out var previousLink, out var pathCosts);
            var start = _src;
            frontier.Enqueue(start,0);
            pathCosts.Add(start,0);
            
            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();
                if (current.Equals(_tar))
                    break;

                var curCost = pathCosts[current];
                foreach (var next in _graph.GetAdjacentNodes(current))
                {
                    var cost = _graph.Cost(current,next);
                    var newCost = curCost + cost;
                    var contains = pathCosts.ContainsKey(next);
                    if (!contains)
                    {
                        pathCosts.Add(next,newCost);
                        previousLink.Add(next,current);
                    }
                    
                    if (!contains || newCost < pathCosts[next])
                    {
                        pathCosts[next] = newCost;
                        frontier.Enqueue(next,curCost+cost + _graph.Heuristic(next,_tar));
                        previousLink[next] = current;
                    }
                }
            }
            AStarStorage<T>.Output(previousLink,_outputPaths,_src,_tar);
        }
    }
}