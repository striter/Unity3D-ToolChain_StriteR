using Dome.Entity;
using Unity.Mathematics;
using System.Linq.Extensions;

namespace Dome
{
    public class FDomeTeam
    {
        public ETeam m_TeamNumber { get; private set; }
        public int m_CommanderID { get; private set; }

        public float m_TeamResources { get; private set; }
        
        private FDomeGameRules m_GameRules;
        private FDomeEntities m_EntityFactory;
        
        public FDomeTeam(ETeam _teamNumber,FDomeGameRules _gameRules,FDomeEntities _factory)
        {
            m_TeamNumber = _teamNumber;
            m_GameRules = _gameRules;
            m_EntityFactory = _factory;
            m_TeamResources = 0;
        }

        public void RoundStart(float4 _initialTechPoint)
        {
            m_CommanderID = 0;
            SpawnInitialStructures(_initialTechPoint);
        }

        public void RoundFinish()
        {
            m_EntityFactory.GetEntities(FDomeEntityFilters.FilterTeams[m_TeamNumber]).Traversal(p => m_EntityFactory.Recycle(p.id));
        }

        public void SpawnInitialStructures(float4 _initialTechPoint)
        {
            var pos = _initialTechPoint.xyz;
            var yaw = _initialTechPoint.w;
            var rotation = quaternion.Euler(0, yaw * kmath.kDeg2Rad, 0);
            var initialPosition = new TR(pos, rotation);
            m_CommanderID = m_EntityFactory.Spawn("Commander",initialPosition, m_TeamNumber).id;
            m_EntityFactory.Spawn("BCommandStation", initialPosition, m_TeamNumber);
            m_EntityFactory.Spawn("AAntiAir",initialPosition,m_TeamNumber);
            m_EntityFactory.Spawn("AMBT",initialPosition,m_TeamNumber);
            m_EntityFactory.Spawn("AScout",initialPosition,m_TeamNumber);
        }
        
        public void Tick(float _deltaTime)
        {
            
        }

    }

}