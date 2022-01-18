using System.Collections.Generic;
using UnityEngine;

namespace  ExampleScenes.Rendering.WaveInteraction
{
    public class WaveInteraction : MonoBehaviour
    {
        public MeshRenderer m_WaterMesh;
        public GameObject m_StepObj;
        public AnimationCurve m_StepCurve;
        [Header("Stationary")]
        public float m_StepStationaryTick=1.2f;
        public float m_StepStationarySpeed = .06f;
        public float m_StepStationaryDuration = 3.5f;
        public float m_StepStationaryWidth = .15f;
        public float m_StepStationaryOffset = .5f;
        [Header("Dynamic")]
        public float m_StepDynamicTick = .5f;
        public float m_StepDynamicSpeed = 5f;
        public float m_StepDynamicDuration = .2f;
        public float m_StepDynamicWidth = .15f;
        public float m_StepDynamicOffset = .05f;
        
        private WaterInteractions m_WaterInteractions;
        private Vector3 m_Destination;
        private readonly Counter m_StepStationaryClicker=new Counter();
        private readonly Counter m_StepDynamicClicker=new Counter();
        private void Awake()
        {
            m_StepStationaryClicker.Set(m_StepStationaryDuration);
            m_StepDynamicClicker.Set(m_StepDynamicDuration);
            m_WaterInteractions = new WaterInteractions(m_WaterMesh.material);
            m_WaterInteractions.Clear();
        }

        void Update()
        {
            float deltaTime = Time.deltaTime;
            if (Input.GetMouseButton(0)&&RayPlaneDistance(Vector3.up, 0f, Camera.main.ScreenPointToRay(Input.mousePosition),out Vector3 lPoint))
                m_Destination = lPoint;
            
            if (Input.GetMouseButtonDown(1)&&RayPlaneDistance(Vector3.up, 0f, Camera.main.ScreenPointToRay(Input.mousePosition),out Vector3 rPoint))
            {
                float duration = 1.5f;
                float speed = 8f;
                Debug.DrawRay(rPoint,Vector3.up,Color.white,duration);
                
                m_WaterInteractions.Click(rPoint,speed,duration,.17f,0f,Vector3.zero);
                m_WaterInteractions.Click(rPoint, speed, duration, .20f, -.1f,Vector3.zero);
                m_WaterInteractions.Click(rPoint, speed, duration, .23f, -.2f,Vector3.zero);
            }

            
            Vector3 src = m_StepObj.transform.position;
            Vector3 dest = Vector3.Lerp(src,m_Destination,deltaTime*10f);
            dest.y = m_StepCurve.Evaluate(m_StepStationaryClicker.m_TimeElapsedScale);
            m_StepObj.transform.position = dest;
            
            if (m_StepStationaryClicker.Tick(deltaTime))
            {
                m_WaterInteractions.Click(dest,m_StepStationarySpeed,m_StepStationaryDuration,m_StepStationaryWidth,m_StepStationaryOffset,Vector3.zero);
                m_StepStationaryClicker.Set(m_StepStationaryTick);
                m_StepDynamicClicker.Set(m_StepDynamicTick);
            }
            
            Vector3 offset = dest - src;
            float moveStrength =  new Vector2(offset.x,offset.z).magnitude /2f;
            if (m_StepDynamicClicker.Tick(moveStrength))
            {
                Vector3 direction = (m_Destination-src).normalized;
                Debug.DrawRay(m_Destination,direction,Color.blue,1f);
                Debug.DrawRay(m_Destination,Vector3.up,Color.green,1f);
                m_WaterInteractions.Click(dest+direction*deltaTime,m_StepDynamicSpeed,m_StepDynamicDuration,m_StepDynamicWidth,m_StepDynamicOffset,direction);
                m_StepStationaryClicker.Set(m_StepStationaryTick);
                m_StepDynamicClicker.Set(m_StepDynamicTick);
            }

            if(Input.GetKeyDown(KeyCode.R))
                m_WaterInteractions.Clear();
            
            m_WaterInteractions.Tick(deltaTime);
        }

        private static bool RayPlaneDistance(Vector3 planeNormal,float _planeDistance,Ray _ray,out Vector3 point)
        {
            float nrO = Vector3.Dot(planeNormal, _ray.origin);
            float nrD = Vector3.Dot(planeNormal, _ray.direction);
            float distance= (_planeDistance - nrO) / nrD;
            point = _ray.GetPoint(distance);
            return distance > 0;
        }
    }

    public class WaterInteractions
    {
        #region Keywords
        private const string KW_WaveInteraction = "_WAVEINTERACTION";
        private const int C_WaveCount = 16;
        private static readonly int ID_AdditionalWavesShape = Shader.PropertyToID("_AdditionalWavesShape");
        private static readonly int ID_AdditionalWavesDirections = Shader.PropertyToID("_AdditionalWavesDir");
        private static readonly int ID_AdditionalWavesCount = Shader.PropertyToID("_AdditionalWavesCount");
        #endregion

        private struct WaveInstance
        {
            public Vector3 m_StartPosition;
            public float m_TimeInit;
            public float m_Speed;
            public float m_Width;
            public float m_Duration;
            public float m_MaxDistance;
            public Vector3 m_MoveDirection;
            public bool Collect(float _timeElapsed,ref Vector4 shape,ref Vector4 direction)
            {
                float passedTime = _timeElapsed - m_TimeInit;
                if (passedTime < 0)
                    return false;
                float leftScale =1.0f- passedTime/m_Duration;

                float width = m_Width*Mathf.Lerp(0.15f,1f, leftScale);
                float begin=Mathf.Min(m_Speed*passedTime,m_MaxDistance);
                float end = Mathf.Min( begin + width,m_MaxDistance);
                shape= new Vector4(m_StartPosition.x,m_StartPosition.z,begin*begin,end*end);
                direction = new Vector4(m_MoveDirection.x, m_MoveDirection.z, 0, 0);
                return true;
            }

            public bool Expired(float _timeElapsed)
            {
                return (_timeElapsed - m_TimeInit) >= m_Duration;
            }
        }

        float m_TimeElapsed;
        bool m_Toggle;
        List<WaveInstance> m_WaveInstances;
        private Material m_WaveMaterial;
        private Vector4[] m_Shapes;
        private Vector4[] m_Directions;
        public WaterInteractions(Material _waveMaterial)
        {
            m_WaveMaterial = _waveMaterial;
            m_WaveInstances = new List<WaveInstance>();
            m_Shapes = new Vector4[C_WaveCount];
            m_Directions = new Vector4[C_WaveCount];
        }

        public void Clear()
        {
            m_TimeElapsed = 0;
            m_WaveInstances.Clear();
            Apply();
        }
        
        public void Release()
        {
            m_WaveInstances.Clear();
            m_WaveInstances = null;
            m_Shapes = null;
            m_Directions = null;
            m_WaveMaterial = null;
        }

        public void Tick(float _deltaTime)
        {
            m_TimeElapsed += _deltaTime;
            var elapsedWave = m_WaveInstances.FindAll(p => p.Expired(m_TimeElapsed));
            foreach (WaveInstance waveInstance in elapsedWave)
                m_WaveInstances.Remove(waveInstance);
            
            Apply();
        }
        
        public void Click(Vector3 _position, float _speed,float _duration, float _width,float _offsetTime,Vector3 _direction)
        {
            m_WaveInstances.Add(new WaveInstance()
            {
                m_StartPosition = _position,
                m_Speed = _speed,
                m_Duration=_duration,
                m_TimeInit = m_TimeElapsed-_offsetTime,
                m_Width = _width,
                m_MaxDistance = _speed*_duration,
                m_MoveDirection = _direction
            });
        }

        void Apply()
        {
            int totalWaveCount =C_WaveCount;
            bool enable = totalWaveCount > 0;
            if (m_Toggle != enable)
            {
                m_Toggle=enable;
                m_WaveMaterial.EnableKeyword(KW_WaveInteraction,m_Toggle);
            }

            if (!m_Toggle)
                return;
            Vector4 shape = Vector4.zero;
            Vector4 direction = Vector4.zero;
            int waveCount = 0;
            for (int i = 0; i < m_WaveInstances.Count; i++)
            {
                if(!m_WaveInstances[i].Collect(m_TimeElapsed,ref shape,ref direction))
                    continue;
                
                m_Shapes[waveCount]=shape;
                m_Directions[waveCount] = direction;
                waveCount++;
                if (waveCount == C_WaveCount - 1)
                    break;
            }
            m_WaveMaterial.SetInt(ID_AdditionalWavesCount,waveCount);
            m_WaveMaterial.SetVectorArray(ID_AdditionalWavesShape,m_Shapes);
            m_WaveMaterial.SetVectorArray(ID_AdditionalWavesDirections,m_Directions);
        }

    }
}
