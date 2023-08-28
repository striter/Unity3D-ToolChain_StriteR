using System.Collections.Generic;
using System.Linq;

namespace TechToys.ThePlanet.Module.BOIDS.Bird
{
    public class FBirdTarget : ABoidsTarget
    {
        private readonly FBOIDS_Bird m_Flock;
        private FBirdFlock m_Flocking;
        public FBirdPerchingRoot m_Perching { get; private set; }
        public BoidsActor m_Leader => m_Flock[m_Flocking.leader];
        public bool m_IsLeader => m_Actor.identity == m_Flocking.leader;
        
        public FBirdTarget(FBOIDS_Bird _flock)
        {
            m_Flock = _flock;
            m_Perching = null;
        }

        public void Initialize(FBirdFlock _flocking,FBirdPerchingRoot _startPerching=null)
        {
            m_Flocking = _flocking;
            m_Perching = _startPerching;
        }

        private readonly List<BoidsActor> kActorsHelper = new List<BoidsActor>();
        public override IList<BoidsActor> FilterMembers(Dictionary<int, BoidsActor> _totalMembers)
        {
            m_Flocking.members.Select(p=>_totalMembers[p]).FillList(kActorsHelper);
            return kActorsHelper;
        }

        public override bool PickAvailablePerching()
        {
            float minSqrDistance = float.MaxValue;
            m_Perching = null;
            
            foreach (var perching in m_Flock.m_PerchingRoots.Values)
            {
                if(!perching.LandingAvailable)
                    continue;

                float sqrDistance = (perching.m_Root.CenterWS - m_Leader.Position).sqrMagnitude;
                if(sqrDistance>minSqrDistance)
                    continue;

                minSqrDistance = sqrDistance;
                m_Perching = perching;
            }

            if (m_Perching != null)
            {
                SetTarget( m_Perching.SwitchRandomSpot(m_Actor.identity));
                return true;
            }
            
            return false;
        }

        public override bool PickAnotherSpot()
        {
            if (m_Perching == null)
                return false;
            SetTarget(m_Perching.SwitchRandomSpot(m_Actor.identity));
            return true;
        }
    }
}