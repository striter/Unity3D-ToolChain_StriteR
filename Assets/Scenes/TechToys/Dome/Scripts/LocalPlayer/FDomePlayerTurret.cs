using System;
using Dome.Entity;
using Unity.Mathematics;
namespace Dome.LocalPlayer
{
    [Serializable]
    public class FDomePlayerTurret : ADomePlayerControl<FDomeTurret>
    {
        public override void Tick(FDomeLocalPlayer _player,float _deltaTime,IPlayerControl _entity)
        {
            base.Tick(_player,_deltaTime,_entity);
            var cameraTransform = _player.Refer<FDomeCamera>().transform;
            var playerInput = _player.Refer<FDomeInput>().playerInputs;
            var cameraRotation = cameraTransform.rotation;
            var cameraPosition = cameraTransform.position;
            
            float3 aimDirection = math.mul(cameraRotation, kfloat3.forward);
            
            m_Entity.TickTargetChasing(FDomeEntityFilters.GetAngleToViewDirection(cameraPosition,aimDirection));
            
            if (playerInput.changeClicked)
            {
                var team = _player.Refer<FDomeGameRules>().GetTeam(_player.m_ControlTeam);
                _player.TakeControl(team.m_CommanderID);
            }

            m_Entity.input = playerInput.entityInput;
        }

        public override void Detach()
        {
            
        }
    }
}