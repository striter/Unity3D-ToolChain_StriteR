using System;
using Dome.Entity;
using Unity.Mathematics;
namespace Dome.LocalPlayer
{
    [Serializable]
    public class FDomePlayerTurret : ADomePlayerControl<FDomeTurret>
    {
        [ScriptableObjectEdit] public FDomeCameraData_ThirdPerson m_Constrains;
        [Readonly] public float2 m_Rotation;
        public override void Attach(FDomeLocalPlayer _player, IPlayerControl _entity, IPlayerControl _lastEntity)
        {
            base.Attach(_player, _entity, _lastEntity);
            m_Constrains.m_PositionDamper.Initialize(m_Entity.position);
            
            m_Rotation = 0;
            var viewRotationWS = new float2(m_Rotation.y, m_Rotation.x + m_Entity.yaw);
            m_Constrains.m_RotationDamper.Initialize(viewRotationWS.to3xy());
        }

        public override void Tick(FDomeLocalPlayer _player,float _deltaTime,IPlayerControl _entity)
        {
            base.Tick(_player,_deltaTime,_entity);
            var playerInput = _player.Refer<FDomeInput>().playerInputs;
            
            var rotateInput = playerInput.rotate;
            m_Rotation += rotateInput * m_Constrains.rotationSensitive;
            m_Rotation.y = m_Constrains.rotationClamp.Clamp(m_Rotation.y);

            var viewRotationWS = new float2(m_Rotation.y, m_Rotation.x + m_Entity.yaw);
            var cameraRotation = quaternion.Euler( m_Constrains.m_RotationDamper.Tick(_deltaTime,viewRotationWS.to3xy())*kmath.kDeg2Rad);
            var cameraPosition = m_Constrains.m_PositionDamper.Tick(_deltaTime,m_Entity.position + math.mul(cameraRotation, m_Constrains.positionOffset));
            _player.Refer<FDomeCamera>().SetPositionRotation(cameraPosition, math.mul(cameraRotation , quaternion.Euler(m_Constrains.rotationOffset *kmath.kDeg2Rad)));
            
            float3 aimDirection = math.mul(cameraRotation, kfloat3.forward);
            float2 rotationLS = default;
            if (playerInput.secondary.Press())
            {
                rotationLS = viewRotationWS;
                rotationLS.y -= m_Entity.yaw;
            }
            
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