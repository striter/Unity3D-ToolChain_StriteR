using UnityEngine;
using Boids;
using Boids.Bird;
using Boids.Butterfly;
using Boids.Fish;

public class BoidsManager : MonoBehaviour
{
    private ABoidsFlock[] m_Flocks;
    private FBirdFlock m_BirdFlock;
    private FButterflyFlock m_ButterflyFlock;
    private FFishFlock m_FishFlock;
    private void Awake()
    {
        m_ButterflyFlock = GetComponentInChildren<FButterflyFlock>();
        m_BirdFlock = GetComponentInChildren<FBirdFlock>();
        m_FishFlock = GetComponentInChildren<FFishFlock>();
        m_Flocks = new ABoidsFlock[]{m_ButterflyFlock, m_BirdFlock,m_FishFlock};
        m_Flocks.Traversal(p=>p.Init());
        TouchConsole.Header("Birds");
        var spawn=TouchConsole.Command("Spawn",KeyCode.Alpha1);
        spawn.Button(m_BirdFlock.Spawn);
        TouchConsole.Command("Startle",KeyCode.Alpha2).Button(m_BirdFlock.Startle);
        TouchConsole.Command("Clear",KeyCode.Alpha3).Button(m_BirdFlock.Clear);
        
        TouchConsole.Header("Butterflies");
        TouchConsole.Command("Spawn",KeyCode.Q).Button( m_ButterflyFlock.Spawn);
        TouchConsole.Command("Startle",KeyCode.W).Button(m_ButterflyFlock.Startle);
        TouchConsole.Command("Recall",KeyCode.E).Button(m_ButterflyFlock.Recall);
        TouchConsole.Command("Clear",KeyCode.R).Button(m_ButterflyFlock.Clear );
        
        TouchConsole.Header("Fish");
        TouchConsole.Command("Spawn",KeyCode.A).Button( m_FishFlock.Spawn);
        TouchConsole.Command("Clear",KeyCode.S).Button(m_FishFlock.Clear );
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        m_Flocks.Traversal(p=>p.Tick(deltaTime));
    }
}
