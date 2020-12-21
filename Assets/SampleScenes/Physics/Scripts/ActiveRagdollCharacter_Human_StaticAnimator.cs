using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsTest
{
    public class ActiveRagdollCharacter_Human_StaticAnimator : ActiveRagdollCharacter_HumanBase
    {
        public Transform m_StaticAnimatorHips;
        public Animation m_Animation;
        public Rigidbody m_Hips;
        List<StaticAnimatorSynchonize> m_StaticAnimatorSynchonize=new List<StaticAnimatorSynchonize>();
        struct StaticAnimatorSynchonize
        {
            public Transform m_SyncSource;
            public TPhysics.ConfigurableJoint_Helper m_SyncJoint;
            public StaticAnimatorSynchonize(Transform _source,ConfigurableJoint _target)
            {
                m_SyncSource = _source;
                m_SyncJoint = new TPhysics.ConfigurableJoint_Helper(_target);
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

        bool playing = false;
        void SwitchAnim()
        {
            playing = !playing;
            if (playing)
                m_Animation.Play();
            else
                m_Animation.Stop();
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