using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PCG.Module.BOIDS.Bird
{

    public enum EBirdBehaviour
    {
        Invalid = -1,

        Startling = 0,
        Flying = 1,

        PreLanding = 2,
        Landing = 3,
        Perching = 4,

        Traveling = 11,
        Traveling2 = 12,
    }

    public interface IBirdPerchingRoot : IBoidsPerching, ITransform
    {
        Vector3 CenterWS { get; }
        List<FBoidsVertex> m_BirdLandings { get; }
    }

    public class FBirdPerchingRoot
    {
        public IBirdPerchingRoot m_Root { get; }
        private readonly int[] m_AssignedBirds;
        private int m_AvailableCount;
        public bool LandingAvailable => m_AvailableCount > 2;//&& m_Birds.Count<KBirds.kMaxPerchingCount;
        public FBirdPerchingRoot(IBirdPerchingRoot _root)
        {
            m_Root = _root;
            m_AssignedBirds = new int[_root.m_BirdLandings.Count];
            m_AvailableCount = m_AssignedBirds.Length;
            Clear();
        }
        public FBoidsVertex SwitchRandomSpot(int _boids)
        {
            var srcIndex = m_AssignedBirds.IndexOf(_boids);
            if (srcIndex != -1)
            {
                m_AvailableCount += 1;
                m_AssignedBirds[srcIndex] = -1;
            }

            var dispatchIndex = m_AssignedBirds.IndexOf(-1, Random.Range(0, m_AssignedBirds.Length));
            m_AssignedBirds[dispatchIndex] = _boids;
            m_AvailableCount -= 1;
            return m_Root.m_BirdLandings[dispatchIndex];
        }

        public void Clear()
        {
            for (int i = 0; i < m_AssignedBirds.Length; i++)
                m_AssignedBirds[i] = -1;
            m_AvailableCount = m_AssignedBirds.Length;
        }

        public IEnumerable<int> ActiveActors()
        {
            foreach (var actor in m_AssignedBirds)
            {
                if (actor == -1)
                    continue;
                yield return actor;
            }
        }
#if UNITY_EDITOR
        public void DrawGizmos()
        {
            foreach (var (index, bird) in m_AssignedBirds.LoopIndex())
            {
                Gizmos.color = bird == -1 ? Color.red : Color.green;
                Gizmos.DrawWireSphere(m_Root.m_BirdLandings[index].position, .1f);
            }
        }
#endif
    }

    public class FBirdFlock
    {
        public int leader;
        public int[] members; //Include Leader
        public Counter duration;

        public FBirdFlock(int[] _members, float _duration)
        {
            leader = _members[0];
            members = _members;
            duration = new Counter(_duration);
        }

        public bool Tick(float _deltaTime) => duration.Tick(_deltaTime);
    }

}
