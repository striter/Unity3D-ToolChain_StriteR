using UnityEngine;

namespace Boids
{
    public class BoidsManager : MonoBehaviour
    {
        private BoidsFlock[] m_Flocks;
        private void Awake()
        {
            m_Flocks = GetComponentsInChildren<BoidsFlock>();
            m_Flocks.Traversal(p=>p.Init());
            UIT_TouchConsole.InitDefaultCommands();
            UIT_TouchConsole.Command("Clear",KeyCode.R).Button( Clear);
            UIT_TouchConsole.Command("More Birds",KeyCode.Alpha1).Button(() =>
            {
                m_Flocks[0].Spawn(10);
            });
            UIT_TouchConsole.Command("More Butterflies",KeyCode.Alpha2).Button( ()=>
            {
                m_Flocks[1].Spawn(10);
            });
            
            UIT_TouchConsole.Command("Birds Startle",KeyCode.Alpha3).Button( ()=>
            {
                m_Flocks[0].Startle();
            });            
            UIT_TouchConsole.Command("Butterfly Startle",KeyCode.Alpha4).Button( ()=>
            {
                m_Flocks[1].Startle();
            });
        }

        void Clear()
        {
            m_Flocks.Traversal(p=>p.Clear());
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            m_Flocks.Traversal(p=>p.Tick(deltaTime));
        }
    }
}