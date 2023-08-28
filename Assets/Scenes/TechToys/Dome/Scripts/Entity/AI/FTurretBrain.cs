using UnityEngine;

namespace Dome.Entity.AI
{
    public class FTurretBrain : ADomeBrain<FDomeTurret>
    {
        public FTurretBrain(FDomeTurret _entity) : base(_entity)
        {
        }
        
        public override void Tick(bool _working, float _deltaTime)
        {
            if (!_working) return;
            
            m_Entity.TickTargetChasing(FDomeEntityFilters.GetDistanceToOrigin(m_Entity.position));

            m_Entity.input = new FDomeEntityInput() {
                primary = m_Entity.desiredTarget!=null ? EInputState.Press : EInputState.Empty,
            };
        }

    }
}