using System.Collections.Generic;
using TPool;
using TPoolStatic;
using UnityEngine;

namespace PCG.Module.BOIDS.Bird
{
    public class FBOIDS_Bird : BoidsFlock<FBirdBehaviour, FBirdTarget>
    {
        private readonly FBirdConfig m_BirdConfig;
        public readonly Dictionary<int, FBirdPerchingRoot> m_PerchingRoots = new Dictionary<int, FBirdPerchingRoot>();
        public readonly Dictionary<int, FBirdFlock> m_Flocks = new Dictionary<int, FBirdFlock>();
        private readonly TObjectPoolClass<int, FBirdPoop> m_Poops;

        private readonly RangeFloat kRandomTraverllers = new RangeFloat(60f, 30f);
        private Counter m_TraverlingCounter = new Counter(20f, false);
        public FBOIDS_Bird(FBirdConfig _birdConfig, Transform _transform) : base(_transform)
        {
            m_BirdConfig = _birdConfig;
            m_Poops = new TObjectPoolClass<int, FBirdPoop>(_transform.Find("Poop"));
        }

        public void Clear()
        {
            base.Dispose();
            foreach (var landingControl in m_PerchingRoots.Values)
                landingControl.Clear();
            m_Flocks.Clear();
        }
        public override void Dispose()
        {
            base.Dispose();
            m_PerchingRoots.Clear();
            Clear();
        }

        FBirdFlock SpawnBirdFlock(int _flockSize, float _elapseTime = 0f)
        {
            int[] actors = new int[_flockSize];
            for (int i = 0; i < _flockSize; i++)
                actors[i] = SpawnActor().m_Identity;

            var flocks = new FBirdFlock(actors, _elapseTime);
            m_Flocks.Add(flocks.leader, flocks);
            return flocks;
        }

        void RecycleFlock(int _flockID)
        {
            var flock = m_Flocks[_flockID];
            foreach (var member in flock.members)
                RecycleActor(member);
            m_Flocks.Remove(_flockID);
        }

        public void SpawnIdleAFlocks()
        {
            var flock = SpawnBirdFlock(kFlockRange.Random());
            for (int i = 0; i < flock.members.Length; i++)
            {
                var actor = this[flock.members[i]];
                foreach (var perching in m_PerchingRoots.Values)
                {
                    if (!perching.LandingAvailable)
                        continue;

                    var vertex = perching.SwitchRandomSpot(actor.m_Identity);
                    actor.Initialize(vertex);
                    (actor.m_Behaviour as FBirdBehaviour).Initialize(EBirdBehaviour.Perching);
                    (actor.m_Target as FBirdTarget).Initialize(flock, perching);
                    break;
                }
            }
        }

        private static readonly RangeInt kFlockRange = new RangeInt(8, 4);
        public void SpawnFlyingFlocks()
        {
            var randomDirection = URandom.Random2DDirection();
            var direction = new Vector3(randomDirection.x, 0f, randomDirection.y);
            var srcPosition = m_BirdConfig.flyingConfig.borderOrigin + direction * m_BirdConfig.flyingConfig.borderRange;
            var flock = SpawnBirdFlock(kFlockRange.Random());
            foreach (var member in flock.members)
            {
                var actor = this[member];
                actor.Initialize(new FBoidsVertex() { position = srcPosition + URandom.RandomDirection() * .5f, rotation = Quaternion.LookRotation(-direction, Vector3.up) });
                (actor.m_Behaviour as FBirdBehaviour).Initialize(EBirdBehaviour.Flying);
                (actor.m_Target as FBirdTarget).Initialize(flock);
            }
        }

        public void SpawnTravelingFlock(Vector3 _position, FBoidsVertex _target, int _flockSize, float _elapseTime)
        {
            var flock = SpawnBirdFlock(_flockSize, _elapseTime);
            foreach (var member in flock.members)
            {
                var actor = this[member];
                actor.Initialize(new FBoidsVertex() { position = _position + URandom.RandomDirection() * .5f, rotation = Quaternion.LookRotation(Vector3.up, Vector3.forward) });
                (actor.m_Behaviour as FBirdBehaviour).Initialize(EBirdBehaviour.Traveling);
                (actor.m_Target as FBirdTarget).Initialize(flock);
                (actor.m_Target as FBirdTarget).SetTarget(_target);
            }
        }

        public void SpawnTravelingFlock_TMP(Vector3 _position, FBoidsVertex _target, int _flockSize, float _elapseTime)
        {

            var travelDirection = URandom.Random2DDirection().ToVector3_XZ();
            var travelOffset = travelDirection * m_BirdConfig.flyingConfig.borderRange;
            var travelCenter = m_BirdConfig.flyingConfig.borderOrigin + Vector3.Cross(Vector3.up, travelDirection) * URandom.Random01() * m_BirdConfig.flyingConfig.borderRange * .5f;
            _target = new FBoidsVertex(travelCenter + travelOffset,
           Quaternion.LookRotation(travelDirection, Vector3.up));

            var flock = SpawnBirdFlock(_flockSize, _elapseTime);
            foreach (var member in flock.members)
            {
                var actor = this[member];
                actor.Initialize(new FBoidsVertex() { position = _position + URandom.RandomDirection() * .5f, rotation = Quaternion.LookRotation(Vector3.up, Vector3.forward) });
                (actor.m_Behaviour as FBirdBehaviour).Initialize(EBirdBehaviour.Traveling2);
                (actor.m_Target as FBirdTarget).Initialize(flock);
                (actor.m_Target as FBirdTarget).SetTarget(_target);
            }
        }

        void SpawnRandomTravellers()
        {
            var travelDirection = URandom.Random2DDirection().ToVector3_XZ();
            var travelOffset = travelDirection * m_BirdConfig.flyingConfig.borderRange;
            var travelCenter = m_BirdConfig.flyingConfig.borderOrigin + Vector3.Cross(Vector3.up, travelDirection) * URandom.Random01() * m_BirdConfig.flyingConfig.borderRange * .5f;
            SpawnTravelingFlock(
                travelCenter - travelOffset,
                new FBoidsVertex(travelCenter + travelOffset,
                Quaternion.LookRotation(travelDirection, Vector3.up)),
                kFlockRange.Random(), 45f);
        }

        public override void Tick(float _deltaTime)
        {
            base.Tick(_deltaTime);
            if (m_TraverlingCounter.Tick(_deltaTime))
            {
                SpawnRandomTravellers();
                m_TraverlingCounter.Set(kRandomTraverllers.Random());
            }

            TSPoolList<int>.Spawn(out var poops);
            m_Poops.m_Dic.Keys.FillList(poops);
            foreach (var poop in poops)
                m_Poops[poop].Tick(_deltaTime);
            TSPoolList<int>.Recycle(poops);

            TSPoolList<int>.Spawn(out var recycleFlock);
            foreach (var flock in m_Flocks.Keys)
            {
                if (!m_Flocks[flock].Tick(_deltaTime))
                    continue;
                recycleFlock.Add(flock);
            }

            foreach (var flock in recycleFlock)
                RecycleFlock(flock);
            TSPoolList<int>.Recycle(recycleFlock);
        }

        private void Poop(int _actor)
        {
            var poopActor = this[_actor];
            var target = poopActor.m_Target as FBirdTarget;
            Transform perchingRoot = null;
            if (target.m_Perching != null)
                perchingRoot = target.m_Perching?.m_Root.Transform;
            m_Poops.Spawn().Initialize(poopActor.Position, poopActor.Rotation, perchingRoot);
        }
        protected override FBirdBehaviour GetController() => new FBirdBehaviour(m_BirdConfig, Poop);
        protected override FBirdTarget GetTarget() => new FBirdTarget(this);
        protected override IBoidsAnimation GetAnimation() => new FBoidsMeshAnimation(m_BirdConfig.animConfig);

        public void OnPerchingConstruct(IBirdPerchingRoot _perchingRoot)
        {
            if (m_PerchingRoots.ContainsKey(_perchingRoot.BoidsIdentity))
                return;

            m_PerchingRoots.Add(_perchingRoot.BoidsIdentity, new FBirdPerchingRoot(_perchingRoot));
        }

        public void OnPerchingDeconstruct(IBirdPerchingRoot _perchingRoot)
        {
            if (!m_PerchingRoots.ContainsKey(_perchingRoot.BoidsIdentity))
                return;

            StartleRootedFlocks(_perchingRoot.BoidsIdentity);
            m_PerchingRoots.Remove(_perchingRoot.BoidsIdentity);
        }

        public void StartleRootedFlocks(int _rootIdentity)
        {
            foreach (var flock in CollectAffectedFlock(_rootIdentity))
                foreach (var member in m_Flocks[flock].members)
                    (this[member].m_Behaviour as FBirdBehaviour).Startle();
        }

        IEnumerable<int> CollectAffectedFlock(int _root)
        {
            TSPoolHashset<int>.Spawn(out var iteratedSet);
            foreach (var actor in m_PerchingRoots[_root].ActiveActors())
            {
                var flock = (this[actor].m_Target as FBirdTarget).m_Leader.m_Identity;
                if (iteratedSet.Contains(flock))
                    continue;
                yield return flock;
            }

            TSPoolHashset<int>.Recycle(iteratedSet);
        }

#if UNITY_EDITOR
        public override void DrawGizmos(bool _drawRelativePoints)
        {
            base.DrawGizmos(_drawRelativePoints);
            if (!_drawRelativePoints)
                return;
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = Color.white;
            foreach (var landing in m_PerchingRoots.Values)
                landing.DrawGizmos();
        }
#endif
    }
}
