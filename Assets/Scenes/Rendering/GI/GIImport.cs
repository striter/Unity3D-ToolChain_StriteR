using System;
using Rendering;
using UnityEngine;

namespace ExampleScenes.Rendering.GI
{
    public class GIImport : MonoBehaviour
    {
        public EnvironmentCollection m_Src;
        public EnvironmentCollection m_Dst;
        
        private bool m_Switch;
        private Counter m_SwitchTimer = new Counter(.5f);
        private MeshRenderer[] m_Renderers;
        private void Awake()
        {
            m_Renderers = GetComponentsInChildren<MeshRenderer>();
            UIT_TouchConsole.InitDefaultCommands();
            UIT_TouchConsole.Command("Switch",KeyCode.Space).Button(SwitchPersistent);
            m_Switch = false;
            m_Src.Apply(m_Renderers);
        }

        void SwitchPersistent()
        {
            m_Switch = !m_Switch;
            m_SwitchTimer.Replay();
        }

        private void Update()
        {
            if (!m_SwitchTimer.m_Counting)
                return;
            m_SwitchTimer.Tick(Time.deltaTime);
            var src = m_Switch ? m_Src : m_Dst;
            var dst = m_Switch ? m_Dst : m_Src;

            EnvironmentCollection.Interpolate(m_Renderers,src,dst,m_SwitchTimer.m_TimeElapsedScale);
        }
    }

}