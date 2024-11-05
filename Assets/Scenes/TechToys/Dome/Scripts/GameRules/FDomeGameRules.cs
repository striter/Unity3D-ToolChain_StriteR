using System.Linq.Extensions;
using Dome.Entity;
using Dome.LocalPlayer;
using Runtime.Random;
using UnityEngine;

namespace Dome
{
    public class FDomeGameRules : ADomeController
    {
        [Readonly] public Ref<EGameState> m_GameState;
        private FDomeTeam[] m_Teams;
        public FDomeTeam GetTeam(ETeam _team) => m_Teams.Find(p => p.m_TeamNumber == _team);

        public override void OnInitialized()
        {
            IAim.kGameRules = Refer<FDomeGameRules>();
            
            m_GameState = EGameState.MainMenu;
            TouchConsole.InitDefaultCommands();
            TouchConsole.NewPage("GameRules");
            TouchConsole.Command("GameState").EnumSelection(m_GameState,SetGameState);
            TouchConsole.Command("Gatling Placement",KeyCode.Q).Button(() => {
                if(Refer<FDomeLocalPlayer>().m_Control is FDomePlayerCommander commander)
                    commander.SetPlacement("TGatling");
            });
            TouchConsole.Command("Cannon Placement",KeyCode.E).Button(() => {
                if(Refer<FDomeLocalPlayer>().m_Control is FDomePlayerCommander commander)
                    commander.SetPlacement("TCannon");
            });
            TouchConsole.Command("AA Placement",KeyCode.T).Button(() => {
                if(Refer<FDomeLocalPlayer>().m_Control is FDomePlayerCommander commander)
                    commander.SetPlacement("AAntiAir");
            });
            var factory = Refer<FDomeEntities>();
            m_Teams = new[]
            {
                new FDomeTeam(ETeam.Blue, this, factory),
                new FDomeTeam(ETeam.Red, this, factory),
            };
            
            FAssets.PrecachePrefabsAtPath(KDomeEntities.ARC.kRoot);
            FAssets.PrecachePrefabsAtPath(KDomeEntities.Turrets.kRoot);
            FAssets.PrecachePrefabsAtPath(KDomeEntities.Building.kRoot);
        }

        public override void OnCreated()
        {
            base.OnCreated();
            SetGameState(EGameState.GameStart);
        }

        public void SetGameState(EGameState _state)
        {
            if (m_GameState.value == _state)
                return;
            m_GameState = _state;

            if (_state == EGameState.GameStart)
            {
                var initialTechPoints = Refer<FDomeGrid>().initialTechPoints.DeepCopy();
                UShuffle.Shuffle(initialTechPoints,initialTechPoints.Length,1);
                for (int i = 0; i < m_Teams.Length; i++)
                    m_Teams[i].RoundStart(initialTechPoints[i]);

                // m_Teams.Traversal(p=>p.OnRoundStart);
            }
            else if (_state == EGameState.GameEnd)
            {
                m_Teams.Traversal( p=>p.RoundFinish());
            }
            
            FireEvent(KDomeEvents.kOnGameStateChanged,m_GameState.value);
        }

        public override void Tick(float _deltaTime)
        {
            m_Teams.Traversal(p=>p.Tick(_deltaTime));
        }
        
        public override void Dispose()
        {
        }


    }
    
}