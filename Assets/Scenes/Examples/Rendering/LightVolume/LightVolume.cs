using Geometry.Bezier;
using UnityEngine;

//This stuff is inspired by deferred rendering
//*its requiring depth prepass to work properly
namespace Examples.Rendering.LightVolume
{
    class VolumeTransforming
    {
        private GBezierCurveQuadratic m_MovingCurve;
        private Counter m_Counter;
        private bool m_Forward;
        public VolumeTransforming(Vector3 _src,Vector3 _dst,Vector3 _control,float _time)
        {
            m_MovingCurve = new GBezierCurveQuadratic(_src,_dst,_control);
            m_Counter = new Counter(_time);
            m_Forward = true;
        }
        public Vector3 Tick(float _deltaTime)
        {
            m_Counter.Tick(_deltaTime);
            if (!m_Counter.m_Playing)
            {
                m_Counter.Replay();
                m_Forward = !m_Forward;
            }

            return m_MovingCurve.Evaluate(m_Forward?m_Counter.m_TimeElapsedScale:m_Counter.m_TimeLeftScale);
        }
    }
    
    public class LightVolume : MonoBehaviour
    {
        private MeshRenderer[] renderers;
        private VolumeTransforming[] transforms;
        private void Awake()
        {
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            renderers = GetComponentsInChildren<MeshRenderer>();
            transforms = new VolumeTransforming[renderers.Length];
            foreach (var (index,renderer) in renderers.LoopIndex())
            {
                block.SetColor(KShaderProperties.kColor,URandom.RandomColor()*1.5f);
                renderer.SetPropertyBlock(block);
                var startPosition = renderer.transform.position;
                var endPosition = renderer.transform.position + URandom.Random2DDirection().ToVector3_XZ()*10f+Vector3.down*2;
                transforms[index] = new VolumeTransforming(startPosition,endPosition,(startPosition+endPosition)+ URandom.Random2DDirection().ToVector3_XZ()*5f,5f+URandom.Random01()*5f);
            }
        }

        private void Update()
        {
            for (int i = 0; i < renderers.Length; i++)
                renderers[i].transform.position = transforms[i].Tick(Time.deltaTime);
        }
    }

}