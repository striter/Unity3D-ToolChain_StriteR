using Unity.Mathematics;

namespace Dome.Entity
{
    public class FDomeCommander : ADomeEntity,ITeam , IPlayerControl , IVelocityMove
    {
        public ETeam team { get; set; }

        public float3 lastPosition { get; set; }
        public float3 velocity { get; set; }
        public float kStartSpeed => 50f;
    }
}