using Dome.Collision;
using Unity.Mathematics;
using UnityEngine;

namespace Dome.Entity
{
    public interface IProjectile : IVelocityMove, IEffect , ITeam , IEntity ,ICollisionCaster
    {
        public string kMuzzleParticleName {get;}
        public string kProjectileTrail { get; }
        public string kProjectileImpact { get; }
        
        public float kMaxLastingTime { get; }
        public float timeToRecycle { get; set; }
        public ECollisionCasterData kProjectileSize { get; }
    }

    public static class IProjectile_Extension
    {
        public static void OnInitialize(this IProjectile _entity, EntityInitializeParameters _parameters)
        {
            _entity.AttachEffect(_entity.kProjectileTrail,_entity.transform);
            _entity.timeToRecycle = UTime.time + _entity.kMaxLastingTime;
        }

        public static void Tick(this IProjectile _entity,float _deltaTime)
        {
            var startPosition = _entity.lastPosition;
            var endPosition = _entity.position;
            if ((startPosition - endPosition).sqrmagnitude() <= 0.01f) return;

            if (UTime.time > _entity.timeToRecycle)
            {
                IEntity.kEntities.Recycle(_entity.id);
                return;
            }
            
            if (!_entity.CastHit(_entity.kProjectileSize, startPosition, endPosition, out var hitInfo,out var hitID) 
                || hitID== _entity.id)
                return;

            var hitEntity = IEntity.kEntities.Get(hitID);
            if (!FDomeEntityFilters.FilterTeams[_entity.team.GetEnemyTeam()](hitEntity))
                return;
            
            _entity.TriggerEffectsAt(_entity.kProjectileImpact,hitInfo.point,quaternion.LookRotationSafe(hitInfo.normal,kfloat3.up),.5f);
            IEntity.kEntities.Recycle(_entity.id);
        }
    }
    
}