using Unity.Mathematics;
using UnityEngine;

namespace Dome.Entity
{
    public interface ILive : IEntity
    {
        public float kMaxHealth { get; set; }
        public float maxHealth { get; set; }
    }

    public static class ILive_Extension
    {
        public static void OnInitialize(this ILive _live,EntityInitializeParameters _parameters)
        {
            _live.kMaxHealth = _parameters.defines.maxHealth;
        }
    }
    
   

}
