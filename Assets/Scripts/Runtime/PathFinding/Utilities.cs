using System.Collections.Generic;
using Runtime.DataStructure;

namespace Runtime.PathFinding
{
    public static class AStarStorage<T>
    {
        private static PriorityQueue<T, float> frontier = new PriorityQueue<T, float>();
        private static Dictionary<T, T> previousLink = new Dictionary<T, T>();
        private static Dictionary<T, float> pathCosts = new Dictionary<T, float>();
        public static void Request(out PriorityQueue<T, float> _frontier, out Dictionary<T, T> _link, out Dictionary<T, float> _distance)
        {
            frontier.Clear();
            previousLink.Clear();
            pathCosts.Clear();
            _frontier = frontier;
            _link = previousLink;
            _distance = pathCosts;
        }

        public static void Output(Dictionary<T, T> _link, Stack<T> _outputPaths,T _src,T _tar)
        {
            var goal = _tar;
            _outputPaths.Clear();
            while ( previousLink.ContainsKey(goal))
            {
                _outputPaths.Push(goal);
                goal = previousLink[goal];
            }
            _outputPaths.Push(_src);
        }
    }

}