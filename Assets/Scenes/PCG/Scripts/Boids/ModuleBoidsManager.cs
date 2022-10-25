using System.Collections.Generic;
using PCG.Module.BOIDS.Bird;
using PCG.Module.BOIDS.Butterfly;
// using PolyGrid.Module.BOIDS.Fish;
using UnityEngine;

namespace PCG.Module.BOIDS
{
    public interface IBoidsPerching
    {
        int BoidsIdentity { get; }
    }
    
    public class ModuleBoidsManager : MonoBehaviour
    {
        public FBirdConfig m_BirdConfig;
        public FBOIDS_Bird m_Bird { get; private set; }
        public FButterflyConfig m_ButterflyConfig;
        public FBoids_Bufferfly m_Butterflies { get; private set; }
        // public FFishConfig m_FishConfig;
        // public FFishFlock m_FishFlock;

        private int m_ModuleCount = 0;
        private readonly Counter m_BirdSpawnCounter = new Counter(10f);
        public void Init()
        {
            m_Bird = new FBOIDS_Bird(m_BirdConfig,transform.Find("Bird"));
            m_Butterflies = new FBoids_Bufferfly(m_ButterflyConfig, transform.Find("Butterfly"));
            // m_FishFlock = new FFishFlock(m_FishConfig, transform.Find("Fish"));
        }

        public void Setup(GridManager _grid)
        {
        }
        
        public void Clear()
        {
            m_Bird.Dispose();
            m_Butterflies.Dispose();
            
            m_ModuleCount = 0;
        }

        public void Dispose()
        {
        }

        public void Tick(float _deltaTime)
        {
            m_Bird.Tick(_deltaTime);
            m_Butterflies.Tick(_deltaTime);
            // m_FishFlock.Tick(_deltaTime);

            //Spawn flock automaticly
            if (m_BirdSpawnCounter.Tick(_deltaTime))
            {
                m_BirdSpawnCounter.Replay();
                if((m_ModuleCount / 20 + 1) > m_Bird.m_Flocks.Count)
                    m_Bird.SpawnFlyingFlocks();
            }
        }

        public void SpawnTravelingBirds(Vector3 _startPosition,Vector3 _targetPosition,Vector3 _targetDirection,int _size=24,float _time=30f)
        {
            m_Bird.SpawnTravelingFlock(_startPosition,new FBoidsVertex(_targetPosition,Quaternion.LookRotation(_targetDirection,Vector3.up)),_size,_time);
            
        }
        public void OnModuleConstruct(IEnumerable<IModuleStructureElement> _structures)
        {
            foreach (var structure in _structures)
            {
                if(structure is IBirdPerchingRoot birdLandingSpot)
                    m_Bird.OnPerchingConstruct(birdLandingSpot);
                if(structure is IButterflyAttractions butterflyAttractions)
                    m_Butterflies.OnAttractionsConstruct(butterflyAttractions);
            }
        }

        public void OnModuleDeconstruct(IEnumerable<IModuleStructureElement> _structures)
        {
            foreach (var structure in _structures)
            {
                if(structure is IBirdPerchingRoot birdLandingSpot)
                    m_Bird.OnPerchingDeconstruct(birdLandingSpot);
                if(structure is IButterflyAttractions butterflyAttracting)
                    m_Butterflies.OnAttractionsDeconstruct(butterflyAttracting);
            }
        }
#if UNITY_EDITOR
        public bool m_DrawBirdLandings;
        public bool m_DrawButterflyAttractions;
        private void OnDrawGizmos()
        {
            m_Bird?.DrawGizmos(m_DrawBirdLandings);
            m_Butterflies?.DrawGizmos(m_DrawButterflyAttractions);
            m_BirdConfig?.DrawGizmos();
        }
#endif
    }

}