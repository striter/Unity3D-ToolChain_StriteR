using System;
using Dome.Entity;
using Unity.Mathematics;
using UnityEngine;

namespace Dome.LocalPlayer
{
    [Serializable]
    public class FDomePlayerSpectator : ADomePlayerControl
    {
        public override Transform GetAnchor() => null;
        
        public override void Tick(FDomeLocalPlayer _player, float _deltaTime, IPlayerControl _entity)
        {
            
        }

        public override void Detach()
        {
        }
    }
}