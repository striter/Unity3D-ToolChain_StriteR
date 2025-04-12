using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Render.Debug
{
    [CreateAssetMenu(menuName = "Optimize/OverdrawProfiler/Data", fileName = "OverdrawProfilerData")]
    public class OverdrawProfilerData : ScriptableObject
    {
        public CullingMask m_Mask;
        public RenderPassEvent m_Event = RenderPassEvent.AfterRendering;
        public RangeInt m_Stack = new(3, 5);
        public Color m_Color = Color.red;
        
        [InspectorButton]
        public void Profile() => OverdrawProfiler.Init(this);
    }
}