using Dome.Model;
using Unity.Mathematics;
using UnityEngine;
namespace Dome.Entity
{
    public interface ITurretModel : IModel
    {
        public Damper viewDamper { get; set; }
        public Transform pitchTransform { get; set; }
        public Transform yawTransform { get; set; }
        
        public Damper kViewDamperData { get; }
    }

    public static class ITurretModel_Extension
    {
        public static void OnModelSet(this ITurretModel _model,IModel _)
        {
           _model.yawTransform = _model.modelRoot.transform.FindInAllChild(p=>p.name.Contains("Head") || p.name.Contains("Turret"));
           _model.pitchTransform = _model.modelRoot.transform.FindInAllChild(p=>p.name.Contains("Gun"));
           if (!_model.pitchTransform)
               _model.pitchTransform = _model.yawTransform;
           
           _model.viewDamper = _model.kViewDamperData;
           _model.viewDamper.Initialize(0);
        }
        
        public static void OnModelClear(this ITurretModel _model)
        {
            _model.yawTransform = null;
            _model.pitchTransform = null;
        }
        
        public static void Tick(this ITurretModel _model,float _deltaTime)
        {
            if (!_model.isModelAvailable()) return;

            var desiredRotationLS = float2.zero;
            if (_model is IAim target)
                desiredRotationLS = target.desiredRotationLS;
            
            var rotation = _model.viewDamper.Tick(_deltaTime, desiredRotationLS);
            if (_model.pitchTransform == _model.yawTransform)
            {
                _model.pitchTransform.localRotation = Quaternion.Euler(rotation.x, rotation.y, 0);
            }
            else
            {
                _model.yawTransform.localRotation = Quaternion.Euler(0, rotation.y, 0);
                _model.pitchTransform.localRotation = Quaternion.Euler(rotation.x, 0, 0);
            }
        }
    }

}