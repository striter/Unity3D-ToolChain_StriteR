using System;
using CameraController;
using Dome.Entity;
using Unity.Mathematics;
using UnityEngine;

namespace Dome.LocalPlayer
{

    [Serializable]   
    public class FDomePlayerARC : ADomePlayerControl<ADomeARC>
    {
        public override void Tick(FDomeLocalPlayer _player,float _deltaTime,IPlayerControl _entity)
        {
            var playerInput = _player.Refer<FDomeInput>().playerInputs;
            var cameraTransform = _player.Refer<FDomeCamera>().transform;

            float3 aimDirection = math.mul(cameraTransform.rotation, kfloat3.forward);
            m_Entity.TickTargetChasing(FDomeEntityFilters.GetAngleToViewDirection(cameraTransform.position,aimDirection));
            m_Entity.input = playerInput.entityInput;
            if (playerInput.changeClicked)
            {
                var team = _player.Refer<FDomeGameRules>().GetTeam(_player.m_ControlTeam);
                _player.TakeControl(team.m_CommanderID);
            }
        }

        public override void Detach()
        {
            
        }
    }
}