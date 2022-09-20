using System;
using Rendering;
using UnityEngine;

namespace ExampleScenes.Rendering.GI
{
    public class GIImport : MonoBehaviour
    {
        public GIPersistent m_Src;
        public GIPersistent m_Dst;
        
        private bool m_Switch;
        private Counter kSwitchTimer = new Counter(.5f);
        private MeshRenderer[] m_Renderers;
        private void Awake()
        {
            m_Renderers = GetComponentsInChildren<MeshRenderer>();
            TouchConsole.InitDefaultCommands();
            TouchConsole.Command("Switch",KeyCode.Space).Button(Switch);
            m_Switch = false;
            m_Src.m_Data.Apply(m_Renderers);
        }

        void Switch()
        {
            m_Switch = !m_Switch;
            kSwitchTimer.Replay();
        }

        private void Update()
        {
            if (!kSwitchTimer.m_Playing)
                return;
            kSwitchTimer.Tick(Time.deltaTime);
            var src = m_Switch ? m_Src : m_Dst;
            var dst = m_Switch ? m_Dst : m_Src;

            GlobalIlluminationOverrideData.Apply(m_Renderers,src.m_Data,dst.m_Data,kSwitchTimer.m_TimeElapsedScale);
        }
    }

}