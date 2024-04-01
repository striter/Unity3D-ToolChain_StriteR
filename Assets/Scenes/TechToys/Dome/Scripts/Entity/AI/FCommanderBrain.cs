using Dome.Entity;
namespace Dome.Entity.AI
{
    public class FCommanderBrain : ADomeBrain<FDomeCommander>
    {
        public FCommanderBrain(FDomeCommander _entity) : base(_entity)
        {
        }
        
        public override void Tick(bool _working, float _deltaTime)
        {
            //Yes do nothing yes do nothing
        }
    }
}