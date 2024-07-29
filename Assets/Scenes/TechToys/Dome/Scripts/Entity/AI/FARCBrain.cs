using System.Collections.Generic;
using Dome.Entity;
using Unity.Mathematics;
using UnityEngine;

namespace Dome.Entity.AI
{
    public class FARCBrain : ADomeBrain<ADomeARC>
    {
        private readonly Stack<FDomeCell> destinations = new();

        private float2 moveInput;
        public FARCBrain(ADomeARC _entity) :base(_entity)
        {
            var randomCell = kGrid.Random();
            kGrid.PathFind(kGrid.Validate(m_Entity.position),randomCell, destinations);
            moveInput = 0;
        }

        private static readonly float kMinAngularToForward = 30f ;
        private static readonly float kMinAngularToRotate = 5f;
        public override void Tick(bool _working,float _deltaTime)
        {
            if (!_working) return;
            
            m_Entity.TickTargetChasing(FDomeEntityFilters.GetDistanceToOrigin(m_Entity.position));
            TickPathFind();
            m_Entity.input = new FDomeEntityInput() {
                primary = m_Entity.desiredTarget!=null? EInputState.Press: EInputState.Empty,
                move =  moveInput,
            };
        }

        void TickPathFind()
        {
            if (destinations.Count <= 0)
            {
                kGrid.PathFind(kGrid.Validate(m_Entity.position),kGrid.Random(), destinations);
                return;
            }
            
            var current = destinations.Peek().positions.Origin;
            var direction = current - m_Entity.position;
            if (direction.sqrmagnitude()<3f)
            {
                destinations.Pop();
                return;
            }

            var desireYaw = umath.getRadClockwise(kfloat2.up,direction.xz) * kmath.kRad2Deg;
            var deltaAngle = umath.deltaAngle( m_Entity.yaw,desireYaw);
            var deltaAngleValue = math.abs(deltaAngle);
            var rotate = deltaAngleValue > kMinAngularToRotate;
            var forward = deltaAngleValue < kMinAngularToForward;

            moveInput = new float2(rotate ? math.sign(deltaAngle) : 0, forward ? 1f : 0f);
        }

        public override void DrawGizmos()
        {
            base.DrawGizmos();
            Gizmos.matrix = Matrix4x4.Translate(Vector3.up * .5f);
            foreach (var destination in destinations)
                Gizmos.DrawWireSphere(destination.positions.Origin,.1f);
            if(destinations.Count>0)
                Gizmos.DrawLine(m_Entity.position,destinations.Peek().positions.Origin);
            UGizmos.DrawLines(destinations,p=>p.positions.Origin);
        }
    }
}