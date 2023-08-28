using Dome.Model;
using Unity.Mathematics;
using UnityEngine;

namespace Dome.Entity
{
    public interface IARCModel : ITurretModel
    {
        public float2 trackMoves { get; set; }
        public MeshRenderer trackRenderer { get; set; }
    }

    public static class IARCModel_Extension
    {
        private static readonly int kTrackMove = Shader.PropertyToID("_TrackMovement");

        public static void OnModelSet(this IARCModel _model, IModel _)
        {
            _model.trackMoves = 0;
            _model.trackRenderer = _model.modelRoot.transform.FindInAllChild(p => p.name.Contains("Track")).GetComponent<MeshRenderer>();
        }

        public static void OnModelClear(this IARCModel _model)
        {
            _model.trackMoves = 0;
            _model.trackRenderer = null;
        }

        public static void Tick(this IARCModel _model,float _deltaTime)
        {
            if (!_model.isModelAvailable()) return;
            if (!(_model is IARCMove move)) return;
        
            var forwardScalar = (move.speed / move.kSpeedDamper.max);
            var rotateScalar = (move.angularSpeed / move.kAngularSpeedDamper.max);
        
            var speedPitch = - forwardScalar * 3;
            var speedRoll = - rotateScalar * 1;
            _model.SetModelPositionRotation(
                new float3(0,forwardScalar>0?forwardScalar*0.1f:0,0),
                quaternion.Euler(new float3(speedPitch,0,speedRoll)*kmath.kDeg2Rad));

            _model.trackMoves += new float2(
                speedPitch + speedRoll,
                speedPitch - speedRoll) * _deltaTime;
            _model.trackRenderer.material.SetVector(kTrackMove,umath.repeat(_model.trackMoves,1f).to4());
        }
    }
}