using System.Collections.Generic;

namespace Runtime.PathFinding
{
    public static partial class UPathFinding
    {
        public static void Dijkstra<T>(this IGraphPathFinding<T> _graph, T _src, T _tar,Stack<T> _outputPaths)
        {
            AStarStorage<T>.Request(out var frontier, out var previous, out var distance);
            distance.Add(_src,0);
            frontier.Enqueue(_src,0);
            _outputPaths.Clear();
            foreach (var vertex in _graph)
            {
                if(vertex.Equals(_src))
                    continue;
                
                distance.Add(vertex,float.MaxValue);
                frontier.Enqueue(vertex,float.MaxValue);
            }

            while (frontier.Count > 0)
            {
                var u = frontier.Dequeue();
                foreach (var v in _graph.GetAdjacentNodes(u))
                {
                    var alt = distance[u] + _graph.Cost(u,v);
                    if (!(alt < distance[v])) 
                        continue;
                    previous[v] = u;
                    distance[v] = alt;
                    frontier.Enqueue(v,alt);
                }
            }
            AStarStorage<T>.Output(previous,_outputPaths,_src,_tar);
        }
    }
}