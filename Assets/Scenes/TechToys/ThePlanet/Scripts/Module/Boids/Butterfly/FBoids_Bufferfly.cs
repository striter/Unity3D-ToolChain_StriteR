using System;
using System.Collections.Generic;
using System.ComponentModel;
using TechToys.ThePlanet.Module.BOIDS.Bird;
using TechToys.ThePlanet;
using TObjectPool;
using UnityEngine;

namespace TechToys.ThePlanet.Module.BOIDS.Butterfly
{
    public interface IButterflyAttractions : IBoidsPerching
    {
        List<FBoidsVertex> m_ButterflyPositions { get; }
    }

    public class FButterflyAttractionControl
    {
        private readonly List<int> m_Butterflies = new List<int>();
        private readonly IButterflyAttractions m_Attractions;
        public FButterflyAttractionControl(IButterflyAttractions _attractions)
        {
            m_Attractions = _attractions;
        }
        public bool Available() => m_Attractions.m_ButterflyPositions.Count > 0 && m_Butterflies.Count == 0;
        public FBoidsVertex AssignSpot(int _index)
        {
            m_Butterflies.Add(_index);
            return m_Attractions.m_ButterflyPositions.RandomElement();
        }
        public IEnumerable<int> DesignActors()
        {
            foreach (var butterfly in m_Butterflies)
                yield return butterfly;
            ClearActors();
        }

        public void ClearActors()
        {
            m_Butterflies.Clear();
        }

    }

    public class FBoids_Bufferfly : BoidsFlock<FButterflyBehaviour, FBoidsTargetEmpty>
    {
        private readonly FButterflyConfig m_Config;
        protected override FButterflyBehaviour GetController() => new FButterflyBehaviour(m_Config);
        protected override FBoidsTargetEmpty GetTarget() => new FBoidsTargetEmpty();
        protected override IBoidsAnimation GetAnimation() => new FBoidsMeshAnimation(m_Config.animConfig);

        private readonly Dictionary<int, FButterflyAttractionControl> m_Controls = new Dictionary<int, FButterflyAttractionControl>();
        private readonly Counter m_Counter = new Counter(5f);
        public FBoids_Bufferfly(FButterflyConfig _config, Transform _transform) : base(_transform)
        {
            m_Config = _config;
        }

        public override void Dispose()
        {
            base.Dispose();
            foreach (var butterfly in m_Controls.Values)
                butterfly.ClearActors();
            m_Controls.Clear();
        }

        public override void Tick(float _deltaTime)
        {
            base.Tick(_deltaTime);
            if (!m_Counter.TickTrigger(_deltaTime))
                return;
            m_Counter.Replay();
            SpawnButterFlyActor();
        }

        public void SpawnButterFlyActor()
        {
            foreach (var attraction in m_Controls.Values)
            {
                if (!attraction.Available())
                    continue;

                int random = 2 + URandom.RandomInt(5);
                for (int i = 0; i < random; i++)
                {
                    var actor = SpawnActor();
                    var spot = attraction.AssignSpot(actor.identity);
                    actor.Initialize(spot);
                }
                break;
            }
        }

        public void ClearActors()
        {
            base.Dispose();
            foreach (var butterfly in m_Controls.Values)
                butterfly.ClearActors();
        }


        public void OnAttractionsConstruct(IButterflyAttractions _attraction)
        {
            if (m_Controls.ContainsKey(_attraction.Identity))
                return;
            _attraction.SetDirty += SetAttractionDirty;
            m_Controls.Add(_attraction.Identity, new FButterflyAttractionControl(_attraction));
        }

        public void OnAttractionsDeconstruct(IButterflyAttractions _attraction)
        {
            if (!m_Controls.ContainsKey(_attraction.Identity))
                return;

            foreach (var identity in m_Controls[_attraction.Identity].DesignActors())
                RecycleActor(identity);

            _attraction.SetDirty -= SetAttractionDirty;
            m_Controls.Remove(_attraction.Identity);
        }

        void SetAttractionDirty(int _attractionID)
        {
            foreach (var identity in m_Controls[_attractionID].DesignActors())
                RecycleActor(identity);
        }
    }

    public sealed class FButterflyBehaviour : BoidsBehaviour<EButterFlyBehaviour>
    {
        private readonly FButterflyConfig m_Config;
        public FButterflyBehaviour(FButterflyConfig _config)
        {
            m_Config = _config;
        }

        public override void Initialize(BoidsActor _actor, FBoidsVertex _vertex)
        {
            base.Initialize(_actor, _vertex);
            SetBehaviour(EButterFlyBehaviour.Floating);
        }
        protected override IBoidsState CreateBehaviour(EButterFlyBehaviour _behaviourType)
        {
            switch (_behaviourType)
            {
                default: throw new InvalidEnumArgumentException();
                // case EButterFlyBehaviour.Startle:  return TSPool<State.Startle<EButterFlyBehaviour>>.Spawn().Init(m_Config.startleConfig,m_Config.flockingConfig,EButterFlyBehaviour.Floating);
                case EButterFlyBehaviour.Floating: return new States.Floating().Init(m_Config.floatingConfig);
                    // case EButterFlyBehaviour.Landing:return TSPool<State.Bird.HoverLanding<EButterFlyBehaviour>>.Spawn().Spawn(m_Config.landConfig,EButterFlyBehaviour.Stopped);
                    // case EButterFlyBehaviour.Stopped:return TSPool<State.Idle>.Spawn().Init(m_Config.idleConfig);
            }
        }
    }
}