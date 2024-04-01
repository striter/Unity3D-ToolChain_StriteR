namespace Dome.Entity
{
    public interface IRayCaster : ICaster
    {
        public string kMuzzleParticleName {get;}
        public string kProjectileTrail { get; }
        public string kProjectileImpact { get; }
    }
}