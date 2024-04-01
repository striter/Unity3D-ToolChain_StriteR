using System;
using Dome.Model;
using Unity.Mathematics;

namespace Dome.Entity
{
    public interface IEntity : IMove , IEffect
    {
        public const int kInvalidID = -1;
        public static FDomeEntities kEntities;
        public static FDomeGameRules kGameRules;
        
        public string define { get; set; }
        public int id { get;}
    }

    public static class IEntity_Extension
    {
        public static float3 targetPosition(this IEntity _this)
        {
            if (_this is IModel model && model.isModelAvailable()) return model.GetNode("FX_Target").position;
            return _this.position;
        }

        public static void OnInitialize(this IEntity _entity, EntityInitializeParameters _parameters)
        {
            _entity.define = _parameters.defineID;
        }

        public static void OnDispose(this IEntity _entity, EntityInitializeParameters _parameters)
        {
            _entity.define = String.Empty;
        }
    }
}