using Rendering.PostProcess;
using UnityEngine;
using Runtime.TouchTracker;
using System.Linq.Extensions;

namespace Examples.Rendering.PostProcess
{
    public class PostProcess : MonoBehaviour
    {
        PostProcess_Opaque m_Controller;
        Camera m_ControlCamera;
        Vector3 m_AreaOrigin;
        float m_AreaRadius;
        private void Awake()
        {
            m_Controller = GetComponentInChildren<PostProcess_Opaque>();
            m_ControlCamera = m_Controller.GetComponent<Camera>();
            TouchConsole.InitDefaultCommands();
            foreach(var postEffects in GetComponentsInChildren<MonoBehaviour>().CollectAs<MonoBehaviour,IPostProcessBehaviour>())
            {
                TouchConsole.NewPage(postEffects.GetType(). Name);
                TouchConsole.InitSerializeCommands(postEffects,null);
            }
        }

        private void Update()
        {
            var tracks = TouchTracker.Execute(Time.unscaledDeltaTime);
            if (tracks.Input_SingleDrag(out var output))
            {
                if (m_ControlCamera.InputRayCheck(output.origin, out RaycastHit hit1) && m_ControlCamera.InputRayCheck(output.current,out var hit2))
                {
                    var data = m_Controller.GetData();
                    m_AreaOrigin = (hit1.point + hit2.point) / 2;
                    m_AreaRadius = Vector3.Distance( hit2.point,hit1.point)/2f;
                    data.areaData.radius = m_AreaRadius;
                    data.areaData.origin = m_AreaOrigin;
                    m_Controller.SetEffectData(data);
                }
            }
            
            foreach (var click in tracks.ResolveClicks())
            {
                if (m_ControlCamera.InputRayCheck(click, out RaycastHit _hit))
                    m_Controller.StartDepthScanCircle(_hit.point, 10f, 1f);
            }
        }
    }
}
