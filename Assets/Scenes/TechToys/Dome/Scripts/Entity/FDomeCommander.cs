using Unity.Mathematics;

namespace Dome.Entity
{
    public class FDomeCommander : ADomeEntity,ITeam , IPlayerControl
    {
        public ETeam team { get; set; }
        
    }
}