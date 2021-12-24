using UnityEngine;
using Boids;
using Boids.Bird;
using Boids.Butterfly;

public class BoidsManager : MonoBehaviour
{
    private ABoidsFlock[] m_Flocks;
    private FBirdFlock m_BirdFlock;
    private FButterflyFlock m_ButterflyFlock;
    private void Awake()
    {
        m_ButterflyFlock = GetComponentInChildren<FButterflyFlock>();
        m_BirdFlock = GetComponentInChildren<FBirdFlock>();
        m_Flocks = new ABoidsFlock[]{m_ButterflyFlock, m_BirdFlock};
        m_Flocks.Traversal(p=>p.Init());
        UIT_TouchConsole.InitDefaultCommands();
        UIT_TouchConsole.Command("Clear",KeyCode.R).Button( Clear);
        
        UIT_TouchConsole.Header("Birds");
        UIT_TouchConsole.Command("Spawn",KeyCode.Alpha1).Drag((status,position) =>
        {
            Debug.LogWarning(status);
            if (status)
                return;
            m_BirdFlock.Spawn(position);
        });
        UIT_TouchConsole.Command("Clear",KeyCode.Alpha1).Button(m_BirdFlock.Clear);
        UIT_TouchConsole.Command("Startle",KeyCode.Alpha3).Button(m_BirdFlock.Startle);
        
        UIT_TouchConsole.Header("Butterflies");
        UIT_TouchConsole.Command("Spawn",KeyCode.Alpha4).Button( m_ButterflyFlock.Spawn);
        UIT_TouchConsole.Command("Clear",KeyCode.Alpha4).Button( m_ButterflyFlock.Spawn);
        UIT_TouchConsole.Command("Recall",KeyCode.Alpha5).Button(m_ButterflyFlock.Recall);
        UIT_TouchConsole.Command("Startle",KeyCode.Alpha6).Button(m_ButterflyFlock.Startle);
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
