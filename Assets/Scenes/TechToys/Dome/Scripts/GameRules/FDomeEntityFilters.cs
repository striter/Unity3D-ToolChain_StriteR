using System;
using System.Collections.Generic;
using Dome.Entity;
using Unity.Mathematics;

namespace Dome
{
    public static class FDomeEntityFilters
    {
        public static Dictionary<ETeam, Predicate<ADomeEntity>> FilterTeams { get; private set; } = new();

        public static Func<ADomeEntity, float> GetDistanceToOrigin(float3 _origin)
        {
            return p => (p.position - _origin).sqrmagnitude();
        }

        public static Func<ADomeEntity, float> GetAngleToViewDirection(float3 _origin,float3 _viewDirection)
        {
            return p => math.abs( umath.closestAngle(_viewDirection,(p.position - _origin).normalize(), kfloat3.up));
        }

        static FDomeEntityFilters()
        {
            foreach (var team in UEnum.GetEnums<ETeam>()) {
                FilterTeams.Add(team, p => p is ITeam teamEntity && teamEntity.team == team);
            }
        }
        
        public static ETeam GetEnemyTeam(this ETeam _team)
        {
            switch (_team)
            {
                default: return ETeam.None;
                case ETeam.Blue: return ETeam.Red;
                case ETeam.Red: return ETeam.Blue;
            }
        }

    }
}