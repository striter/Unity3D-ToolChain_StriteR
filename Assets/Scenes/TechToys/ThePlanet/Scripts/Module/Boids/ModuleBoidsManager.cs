using System;
using System.Collections.Generic;
using TechToys.ThePlanet.Module.BOIDS.Bird;
using TechToys.ThePlanet.Module.BOIDS.Butterfly;
// using PolyGrid.Module.BOIDS.Fish;
using UnityEngine;

namespace TechToys.ThePlanet.Module.BOIDS
{
    public interface IBoidsPerching
    {
        int Identity { get; }
        Action<int> SetDirty { get; set; }
    }
    
    public class ModuleBoidsManager : MonoBehaviour,IModuleControl
    {
        public FBirdConfig m_BirdConfig;
        public FBOIDS_Bird m_Bird { get; private set; }
        public FButterflyConfig m_ButterflyConfig;
        public FBoids_Bufferfly m_Butterflies { get; private set; }
        // public FFishConfig m_FishConfig;
        // public FFishFlock m_FishFlock;

        private int m_ModuleCount = 0;
        private readonly Counter m_BirdSpawnCounter = new Counter(10f);
        public GridManager m_Grid { get; set; }

        public void Init()
        {
            m_Bird = new FBOIDS_Bird(m_BirdConfig,transform.Find("Bird"));
            m_Butterflies = new FBoids_Bufferfly(m_ButterflyConfig, transform.Find("Butterfly"));
            // m_FishFlock = new FFishFlock(m_FishConfig, transform.Find("Fish"));
        }

        public void Setup()
        {
            m_Bird.SpawnFlyingFlocks(true);
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
            if (m_BirdSpawnCounter.TickTrigger(_deltaTime))
            {
                m_BirdSpawnCounter.Replay();
                if((m_ModuleCount / 20 + 1) > m_Bird.m_Flocks.Count)
                    m_Bird.SpawnFlyingFlocks(false);
            }
        }

        public void OnModuleConstruct(IModuleStructureElement _structure)
        {
            if(_structure is IBirdPerchingRoot birdLandingSpot)
                m_Bird.OnPerchingConstruct(birdLandingSpot);
            if(_structure is IButterflyAttractions butterflyAttractions)
                m_Butterflies.OnAttractionsConstruct(butterflyAttractions);
        }

        public void OnModuleDeconstruct(IModuleStructureElement _structure)
        {
            if(_structure is IBirdPerchingRoot birdLandingSpot)
                m_Bird.OnPerchingDeconstruct(birdLandingSpot);
            if(_structure is IButterflyAttractions butterflyAttracting)
                m_Butterflies.OnAttractionsDeconstruct(butterflyAttracting);
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

        [InspectorButton]
        public void CreateNewBirdConfig() =>UnityEditor.Extensions.UEAsset.CreateScriptableInstanceAtCurrentRoot<FBirdConfig>("Bird");   

        [InspectorButton]
        public void CreateNewButterflyConfig() => UnityEditor.Extensions.UEAsset.CreateScriptableInstanceAtCurrentRoot<FButterflyConfig>("Butterfly"); 
        
#endif
    }

}