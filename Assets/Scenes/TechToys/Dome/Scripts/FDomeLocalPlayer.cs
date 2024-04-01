using System;
using Dome.Entity;
using Dome.UI;
using UnityEngine;

namespace Dome.LocalPlayer
{
    public class FDomeLocalPlayer : ADomeController
    {
        public ETeam m_ControlTeam;
        
        public FDomePlayerSpectator kSpectatorMode = new FDomePlayerSpectator();
        public FDomePlayerCommander kCommanderMode = new FDomePlayerCommander();
        public FDomePlayerARC kARCMode = new FDomePlayerARC();
        public FDomePlayerTurret kTurretMode = new FDomePlayerTurret();
        public new T Refer<T>() where T : ADomeController => ADomeController.Refer<T>();
        [Readonly] public int m_ControllingID;
        public ADomePlayerControl m_Control { get; private set; }
        
        public override void OnInitialized()
        {
            m_ControllingID = -1;
            m_Control = null;
        }

        public override void OnCreated()
        {
            base.OnCreated();
            TakeControl(-1);
        }

        public override void Tick(float _deltaTime)
        {
            var entity = m_ControllingID>=0 ? Refer<FDomeEntities>().Get(m_ControllingID) : null;
            m_Control?.Tick(this,_deltaTime,entity as IPlayerControl);
        }

        void OnGameStateChanged(EGameState _state)
        {
            var controllingTeam = Refer<FDomeGameRules>().GetTeam(m_ControlTeam);
            if (controllingTeam == null)
                return;
            
            switch (_state)
            {
                case EGameState.GameStart: {
                    TakeControl(controllingTeam.m_CommanderID);
                }
                    break;
            }
        }

        public override void Dispose()
        {
            m_Control = null;
        }
        
        public void TakeControl(int _id)
        {
            IPlayerControl prevEntity = null;
            if (m_Control != null) {
                prevEntity = Refer<FDomeEntities>().Get(m_ControllingID) as IPlayerControl;
                m_Control.Detach();
            }
            
            m_ControllingID = _id;
            var curEntity = Refer<FDomeEntities>().Get(m_ControllingID) as IPlayerControl;
            m_Control = GetController(curEntity);
            m_Control.Attach(this,curEntity,prevEntity);
            FireEvent(KDomeEvents.kOnEntityControlChanged,m_Control);
        }

        ADomePlayerControl GetController(IPlayerControl _entity)
        {
            if(_entity == null)
                return kSpectatorMode;
            if (_entity is ADomeARC)
                return kARCMode;
            if (_entity is FDomeCommander)
                return kCommanderMode;
            if (_entity is FDomeTurret)
                return kTurretMode;
            throw new InvalidCastException();
        }

        public bool m_DrawGizmos;
        private void OnDrawGizmos()
        {
            if(m_DrawGizmos)
                m_Control?.OnDrawGizmos(this);
        }
    }
}