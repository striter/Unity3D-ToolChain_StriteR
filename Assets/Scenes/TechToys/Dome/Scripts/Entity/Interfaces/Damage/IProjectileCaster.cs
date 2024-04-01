using System.Linq;
using Dome.Model;
using Unity.Mathematics;
using UnityEngine;

namespace Dome.Entity
{
    public interface IProjectileCaster : ICaster, IOwner ,IEntity, IModel , ITeam , IAim
    {
        public FDomeEntityInput input { get; set; }
        public string kProjectileName { get; }
        public float kCastCooldown { get; }
        public Counter projectileCastCooldown { get; set; }
    }

    public static class IProjectileCaster_Extension
    {
        public static IProjectile CastProjectile(this IProjectileCaster _owner, string _projectileName,float3 _targetPosition)
        {
            var castTransform = _owner.GetCasterTransform();
            var startPosition = castTransform.position;
            var direction = (_targetPosition - (float3)startPosition).normalize();
            var entity = IEntity.kEntities.Spawn(_projectileName,new TR(startPosition,quaternion.LookRotation(direction,kfloat3.up)), _owner.team, _owner);
            if (entity is IProjectile projectile)
            {
                projectile.TriggerEffectsAt(projectile.kMuzzleParticleName,castTransform,.5f);
                return projectile;
            }
            Debug.LogWarning($"{_projectileName} isn't a projectile?");
            return null;
        }

        public static void OnInitialize(this IProjectileCaster _caster, EntityInitializeParameters _initialize)
        {
            _caster.projectileCastCooldown = new Counter(_caster.kCastCooldown,true);
        }
        

        public static void Tick(this IProjectileCaster _caster,float _deltaTime)
        {
            _caster.projectileCastCooldown.Tick(_deltaTime);
            if (_caster.desiredTarget == null) return;
            
            if (!_caster.input.primary.Press()) return;
            {
                if (_caster.projectileCastCooldown.m_Playing) return;
                _caster.projectileCastCooldown.Replay();
                _caster.CastProjectile(_caster.kProjectileName, _caster.desiredTarget.targetPosition());
            }
        }
        
    }
}