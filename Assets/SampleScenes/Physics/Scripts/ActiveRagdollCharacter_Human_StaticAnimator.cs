using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsTest
{
    public class ActiveRagdollCharacter_Human_StaticAnimator : ActiveRagdollCharacter_HumanBase
    {
        public Transform m_StaticAnimatorHips;
        List<StaticAnimatorSynchonize> m_StaticAnimatorSynchonize=new List<StaticAnimatorSynchonize>();
        struct StaticAnimatorSynchonize
        {
            public Transform m_SyncSource;
            public TPhysics.ConfigurableJoint_Helper m_SyncJoint;
            public StaticAnimatorSynchonize(Transform _source,ConfigurableJoint _target)
            {
                m_SyncSource = _source;
                m_SyncJoint = new TPhysics.ConfigurableJoint_Helper(_target);
                _target.rotationDriveMode = RotationDriveMode.Slerp;
                _target.slerpDrive = new JointDrive() { positionDamper = 0, positionSpring = 50000 };
            }
            public void Sync()
            {
                //m_SyncJoint.SetTargetRotation(m_SyncSource.localRotation);
            }
        }
        protected override void Start()
        {
            base.Start();
            ConfigurableJoint[] joints = m_PhysicsHips.GetComponentsInChildren<ConfigurableJoint>();
            foreach(var joint in joints)
            {
                Transform syncTransform = m_StaticAnimatorHips.FindInAllChild(joint.gameObject.name);
                if(syncTransform)
                    m_StaticAnimatorSynchonize.Add(new StaticAnimatorSynchonize(syncTransform, joint));
            }
        }
        protected override void Update()
        {
            base.Update();
            m_StaticAnimatorSynchonize.Traversal(sync => sync.Sync());
        }
    }
}