using System.Collections;
using System.Collections.Generic;
using OPhysics;
using UnityEngine;

namespace PhysicsTest
{
    public class ActiveRagdollCharacter_Human_StaticAnimator : ActiveRagdollCharacter_HumanBase
    {
        public Transform m_StaticAnimatorHips;
        public Animator m_StaticAnimator;
        static readonly int HS_B_Walk = Animator.StringToHash("b_walk");
        public Rigidbody m_Hips;
        List<StaticAnimatorSynchonize> m_StaticAnimatorSynchonize=new List<StaticAnimatorSynchonize>();
        struct StaticAnimatorSynchonize
        {
            public Transform m_SyncSource;
            public ConfigurableJoint_Helper m_SyncJoint;
            public StaticAnimatorSynchonize(Transform _source,ConfigurableJoint _target)
            {
                m_SyncSource = _source;
                m_SyncJoint = new ConfigurableJoint_Helper(_target);
            }
            public void Sync()
            {
                m_SyncJoint.SetTargetRotation(m_SyncSource.localRotation);
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
        public override void OnTakeControl()
        {
            base.OnTakeControl();
            PCInputManager.Instance.GetKeyBinding(enum_Binding.Jump).Add(SwitchAnim);
        }
        public override void OnRemoveControl()
        {
            base.OnRemoveControl();
            PCInputManager.Instance.GetKeyBinding(enum_Binding.Jump).Remove(SwitchAnim);
        }

        bool m_Walking = false;
        void SwitchAnim()
        {
            m_Walking = !m_Walking;
            m_StaticAnimator.SetBool(HS_B_Walk, m_Walking);
        }

        protected override void Update()
        {
            base.Update();
            m_StaticAnimatorSynchonize.Traversal(sync => sync.Sync());
        }
        public override void Tick(float _deltaTime)
        {
            base.Tick(_deltaTime);
            m_Hips.rotation=Quaternion.Slerp(m_Hips.rotation, Quaternion.Euler(0,m_Yaw,0),_deltaTime*15f);
        }
    }
}