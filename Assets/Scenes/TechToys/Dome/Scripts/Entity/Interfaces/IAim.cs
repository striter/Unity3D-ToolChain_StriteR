using System;
using System.Linq.Extensions;
using Unity.Mathematics;
using UnityEngine;

namespace Dome.Entity
{
    public interface IAim : IEntity , ITeam
    {
        public IEntity desiredTarget { get; set; }
        public float2 desiredRotationLS { get; set; }
    }

    public static class ITarget_Extension
    {
        public static void OnInitialize(this IAim _this,EntityInitializeParameters _parameters)
        {
            _this.desiredRotationLS = 0;
            _this.desiredTarget = null;
        }

        public static void TickTargetChasing(this IAim _this,Func<ADomeEntity,float> _minElementSelection)
        {
            _this.UpdateTarget(IEntity.kEntities.GetEntities<ILive>(FDomeEntityFilters.FilterTeams[_this.team.GetEnemyTeam()]).       //Targeting nearest enermy
                MinElement(_minElementSelection)); 
        }

        public static void UpdateTarget(this IAim _this,IEntity _target)
        {
            _this.desiredTarget = _target;
            Debug.Assert(_target!=null);
            var rotationLS = float2.zero;
            if (_this.desiredTarget != null)
            {
                var aimDirection = (_this.desiredTarget.targetPosition() - _this.targetPosition()).normalize();
                rotationLS = umath.closestPitchYaw(aimDirection);
                rotationLS.y -= umath.closestPitchYaw(_this.rotation).y;  //Convert to LS
            }
            _this.desiredRotationLS = rotationLS;
        }


        public static void OnRecycle(this IAim _this)
        {
            _this.desiredTarget = null;
            _this.desiredRotationLS = default;
        }
        
    }
}