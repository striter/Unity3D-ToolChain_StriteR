using System;
using Dome.Entity;
using Unity.Mathematics;

namespace Dome.LocalPlayer
{
    [Serializable]
    public class FDomePlayerSpectator : ADomePlayerControl
    {
        [ScriptableObjectEdit] public FDomeCameraData_FreeSpectator m_Constrains;
        [Readonly] public float3 m_Position;
        [Readonly] public float3 m_Rotation;

        public override void Attach(FDomeLocalPlayer _player, IPlayerControl _entity, IPlayerControl _lastEntity)
        {
            base.Attach(_player, _entity,_lastEntity);
            m_Constrains.m_RotationDamper.Initialize(m_Rotation);
            m_Constrains.m_PositionDamper.Initialize(m_Position);
        }

        public override void Tick(FDomeLocalPlayer _player,float _deltaTime,IPlayerControl _entity)
        {
            var camera = _player.Refer<FDomeCamera>();
            camera.SetPositionRotation(m_Position,quaternion.Euler( m_Rotation));
        }

        public override void Detach()
        {
        }
    }
}