using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsTest
{
    public class ActiveRagdollCharacter : PhysicsCharacterBase
    {
        public Transform m_SkinHips, m_PhysicsHips;
        List<TransformSynchronize> m_Synchornizes = new List<TransformSynchronize>();
        struct TransformSynchronize
        {
            public Transform m_SyncTarget;
            public Transform m_SyncSource;
            public TransformSynchronize(Transform _syncTarget, Transform _syncSource)
            {
                m_SyncTarget = _syncTarget;
                m_SyncSource = _syncSource;
            }
            public void Sync()
            {
                m_SyncTarget.transform.position = m_SyncSource.transform.position;
                m_SyncTarget.transform.rotation = m_SyncSource.transform.rotation;
                m_SyncTarget.transform.localScale = m_SyncSource.transform.localScale;
            }
        }
        private void Start()
        {
            Transform[] sourceTransforms = m_SkinHips.GetComponentsInChildren<Transform>();
            foreach (var sourceTransform in sourceTransforms)
            {
                Transform physicsSync = m_PhysicsHips.FindInAllChild(sourceTransform.name);
                m_Synchornizes.Add(new TransformSynchronize(sourceTransform, physicsSync));
            }
        }
        public override void OnTakeControl()
        {
            base.OnTakeControl();
        }
        public override void OnRemoveControl()
        {
            base.OnRemoveControl();
        }
        private void Update()
        {
            m_Synchornizes.Traversal(p => p.Sync());
        }
        public override void Tick(float _deltaTime)
        {
        }
        public override void FixedTick(float _deltaTime)
        {
        }
    }
}