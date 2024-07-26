using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry;
using TObjectPool;
using Runtime.TouchTracker;
using UnityEngine;
using TDataPersistent;
using Unity.Mathematics;
using Gizmos = UnityEngine.Gizmos;

namespace Examples.Algorithm.MarchingCube
{
    public class MarchingCube : SingletonMono<MarchingCube>
    {
        public Int3 m_Size;
        public Mesh[] m_Meshes;

        public bool m_DrawAllCubeElements;
        private Mesh m_Mesh;
        private MeshFilter m_MeshFilter;
        private MeshCollider m_MeshCollider;
        
        private MarchingCubeActor m_Actor;

        private readonly Dictionary<Int3, Grid> m_GridPool = new Dictionary<Int3, Grid>();
        private readonly Dictionary<Int3, Cube> m_CubePool = new Dictionary<Int3, Cube>();
        private readonly Dictionary<byte, MarchingCubeMesh> m_CubeMeshes = new Dictionary<byte, MarchingCubeMesh>();

        private readonly MarchingCubePersistent m_Persistent = new MarchingCubePersistent();
        protected override void Awake()
        {
            base.Awake();
            TouchConsole.InitDefaultCommands();
            TouchConsole.Command("Reset",KeyCode.R).Button(Clear);
            m_Actor = new MarchingCubeActor(transform.Find("Actor"));
            m_Mesh = new Mesh(){name="Marching Cube"};
            m_Mesh.MarkDynamic();
            m_MeshFilter = GetComponent<MeshFilter>();
            m_MeshCollider = GetComponent<MeshCollider>();
            
            foreach (var mesh in m_Meshes)
                m_CubeMeshes.Add(Convert.ToByte(mesh.name),new MarchingCubeMesh(){vertices = mesh.vertices,indices = mesh.GetIndices(0),normals = mesh.normals,uvs = mesh.uv});
            
            Setup();
        }

        void Clear()
        {
            m_Persistent.m_Activations = Array.Empty<GridActivation>();
            m_Persistent.SavePersistentData();
            Setup();
        }
        
        void Setup()
        {
            var _size = m_Size;
            
            foreach (var grid in m_GridPool.Values)
                ObjectPool<Grid>.Recycle(grid);
            m_GridPool.Clear();
            foreach (var cube in m_CubePool.Values)
                ObjectPool<Cube>.Recycle(cube);
            m_CubePool.Clear();
            
            for(int i=0;i<=_size.x;i++)
                for(int j=0;j<=_size.y;j++)
                for (int k = 0; k <= _size.z; k++)
                {
                    Int3 id = new Int3(i, j, k);
                    bool active = id.x != 0 && id.x != _size.x &&
                                  id.y != 0 && id.y != _size.y &&
                                  id.z != 0 && id.z != _size.z;

                    m_GridPool.Add(id,ObjectPool<Grid>.Spawn().Spawn(id,active));
                }
                
            for(int i=0;i<_size.x;i++)
                for(int j=0;j<_size.y;j++)
                for (int k = 0; k < _size.z; k++)
                {
                    Int3 id = new Int3(i, j, k);
                    m_CubePool.Add(id,ObjectPool<Cube>.Spawn().Spawn(id));
                }

            m_Persistent.ReadPersistentData();
            if(m_Persistent.m_Activations!=null)
                foreach (var activation in m_Persistent.m_Activations)
                    m_GridPool[activation.identity].Switch(activation.activation);
                
            Refresh();
        }
        
        private void Update()
        {
            float unscaledDeltaTime = Time.unscaledDeltaTime;
            float deltaTime = Time.deltaTime;
            m_Actor.TickInput(unscaledDeltaTime);
            m_Actor.Tick(deltaTime);
        }

        void Refresh()
        {
            m_Mesh.Clear();

            TSPoolList<Vector3>.Spawn(out var vertices);
            TSPoolList<Vector3>.Spawn(out var normals);
            TSPoolList<int>.Spawn(out var indices);
            TSPoolList<Vector2>.Spawn(out var uvs);

            foreach (var cube in m_CubePool.Values)
            {
                cube.Refresh(m_GridPool);

                var srcByte = cube.m_GridAvailable.ToByte();
                if ( !MarchingCubeDefines.GetOrientedModule(srcByte,out var module,out var orientation))
                    continue;
                if (!m_CubeMeshes.ContainsKey(module))
                {
                    Debug.LogWarning("Invalid Cube Mesh Found:"+  srcByte + " "+module);
                    continue;
                }

                var mesh = m_CubeMeshes[module];
                var rotation = KRotation.kRotate3DCW[orientation];
                var position = cube.position;
                
                int indexStart = vertices.Count;
                vertices.AddRange(mesh.vertices.Select(p=>   position + (Vector3)math.mul(rotation , p) * MarchingCubeDefines.kGridSize));
                normals.AddRange(mesh.normals.Select(p=> (Vector3)math.mul(rotation,p)));
                indices.AddRange(mesh.indices.Select(p=>p+indexStart));
                uvs.AddRange(mesh.uvs);
            }

            m_Mesh.SetVertices(vertices);
            m_Mesh.SetNormals(normals);
            m_Mesh.SetUVs(0,uvs);
            m_Mesh.SetIndices(indices,MeshTopology.Triangles,0);
            
            m_MeshFilter.sharedMesh = m_Mesh;
            m_MeshCollider.sharedMesh = m_Mesh;
        }
        

        public void TerrainValidate(bool _construct, Vector3 _point)
        {
            var grid = m_GridPool.Values.Collect(p=>p.m_Available&&p.m_Active!=_construct).Closest(_point,p=>p.position);
            if (grid==null)
                return;
            
            grid.Switch(_construct);

            foreach (var relative in MarchingCubeDefines.kGridRelativeQubes)
            {
                var identity = relative + grid.m_Identity;
                if (!m_CubePool.ContainsKey(identity))
                    continue;
                
                m_CubePool[identity].Refresh(m_GridPool);
            }

            m_Persistent.m_Activations = m_GridPool.Values.Select(p => new GridActivation() {identity = p.m_Identity,activation = p.m_Active}).ToArray();
            m_Persistent.SavePersistentData();
            Refresh();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;
            
            foreach (var grid in m_GridPool.Values)
            {
                Gizmos.color = grid.m_CubeActive ? Color.green : Color.red;
                Gizmos.DrawWireSphere(grid.position,.1f);
            }

            Gizmos.color = Color.white.SetA(.5f);
            foreach (var cube in m_CubePool.Values)
            {
                var cubeByte = cube.m_GridAvailable.ToByte();
                if( MarchingCubeDefines.GetOrientedModule(cubeByte,out var module,out var orientation) && (m_DrawAllCubeElements || !m_CubeMeshes.ContainsKey(module)))
                    UGizmos.DrawString($"{cubeByte},{module},{orientation}", cube.position);
            }
        }
#endif
    }

    public class MarchingCubeActor:ITransform
    {
        private readonly Transform m_CameraAttacher;
        public Transform transform { get; }

        private Vector3 position;
        private Vector2 pitchYaw;
        
        private Vector2 m_MoveDelta;
        public MarchingCubeActor(Transform _transform)
        {
            transform = _transform;
            position = Vector3.zero;
            pitchYaw = Vector2.zero;
            m_CameraAttacher = _transform.Find("CameraAttacher");
        }

        public void TickInput(float _unscaledDeltaTime)
        {
            var tracks= UTouchTracker.Execute(_unscaledDeltaTime);

            tracks.ResolveClicks(.2f).Traversal(position=>Click(tracks.Count>1, position));
            
            tracks.Joystick_Stationary(
                (position,active)=>{ TouchConsole.DoSetJoystick(position,active);if(!active) m_MoveDelta=Vector2.zero; },
                (normalized)=>{m_MoveDelta = normalized;TouchConsole.DoTrackJoystick(normalized);},
                TouchConsole.kJoystickRange,
                TouchConsole.kJoystickRadius);

            Vector2 rotateDelta = tracks.Input_ScreenMove(TouchConsole.kScreenDeltaRange);
            rotateDelta /= 50f;
            pitchYaw.x = Mathf.Clamp(pitchYaw.x-rotateDelta.y,-60f,60f);
            pitchYaw.y += rotateDelta.x;
            
        }

        void Click(bool _construct,Vector2 _screenPos)
        {
            if (!Physics.Raycast(Camera.main.ScreenPointToRay(_screenPos), out var hit))
                return;
            MarchingCube.Instance.TerrainValidate(_construct,hit.point);
        }

        public void Tick(float _deltaTime)
        {
            Quaternion horizontalRotation=Quaternion.Euler(0f,pitchYaw.y,0f);
            Quaternion rotation = Quaternion.Euler(pitchYaw.x,pitchYaw.y,0f);

            Vector3 right = horizontalRotation * Vector3.right;
            Vector3 forward = horizontalRotation * Vector3.forward;
            position +=  (forward * m_MoveDelta.y+right*m_MoveDelta.x) * _deltaTime * 3f;
            
            m_CameraAttacher.rotation=rotation;
            transform.SetPositionAndRotation(position,rotation);
        }

    }

    public class Grid
    {
        public Int3 m_Identity;
        public bool m_Available { get; private set; }
        public bool m_CubeActive => m_Available && m_Active;
        public bool m_Active { get; private set; }
        public Vector3 position { get; private set; }
        public Grid Spawn(Int3 _identity,bool _available)
        {
            m_Available = _available;
            m_Identity = _identity;
            position = MarchingCubeDefines.GetGridPosition(_identity);
            m_Active = true;
            return this;
        }
        public void Switch(bool _active) => m_Active = _active;
    }

    public class Cube
    {
        private Qube<Int3> m_Grids;
        public Qube<bool> m_GridAvailable;
        public Vector3 position;
        public Cube Spawn(Int3 _identity)
        {
            position = MarchingCubeDefines.GetCubePosition(_identity);
            m_Grids = Qube<Int3>.Convert( MarchingCubeDefines.kQubeRelativeGrids,p=>_identity+p);
            m_GridAvailable = KQube.kTrue;
            return this;
        }
        public void Refresh(Dictionary<Int3,Grid> _grids)
        {
            m_GridAvailable = Qube<bool>.Convert(m_Grids, p => _grids[p].m_CubeActive);
        }
    }


    [Serializable]
    public struct GridActivation
    {
        public Int3 identity;
        public bool activation;
    }
    
    public class MarchingCubePersistent:CDataSave<MarchingCubePersistent>
    {
        public GridActivation[] m_Activations;

        public MarchingCubePersistent()
        {
            m_Activations = Array.Empty<GridActivation>();
        }
    }
    
    public struct MarchingCubeMesh
    {
        public Vector3[] vertices;
        public Vector3[] normals;
        public int[] indices;
        public Vector2[] uvs;
    }
    
    public static class MarchingCubeDefines
    {
        public const float kGridSize = 2f;
        private const float kCubeOffset = kGridSize/2;
        public static Vector3 GetGridPosition(Int3 _identity) => new Vector3(_identity.x,_identity.y,_identity.z)*kGridSize;
        public static Vector3 GetCubePosition(Int3 _identity)=>new Vector3(_identity.x,_identity.y,_identity.z)*kGridSize+Vector3.one*kCubeOffset;

        public static Qube<Int3> kQubeRelativeGrids = new Qube<Int3>(
            Int3.Zero, Int3.kForward, Int3.kForward + Int3.kRight, Int3.kRight,
            Int3.kUp, Int3.kUp + Int3.kForward, Int3.kUp+Int3.kForward + Int3.kRight, Int3.kUp + Int3.kRight);
        public static Qube<Int3> kGridRelativeQubes=new Qube<Int3>(-Int3.kForward-Int3.kRight,-Int3.kRight,Int3.Zero,-Int3.kForward
        ,-Int3.kForward-Int3.kRight-Int3.kUp,-Int3.kRight-Int3.kUp,Int3.Zero-Int3.kUp,-Int3.kForward-Int3.kUp);



        static readonly (byte module, int orientation)[] kOrientedBytes=new (byte module, int orientation)[256];

        public static bool GetOrientedModule(byte _src, out byte _module, out int _orientation)
        {
            _module = byte.MinValue;
            _orientation = 0;
            if (_src == byte.MinValue || _src == byte.MaxValue)
                return false;

            _module = kOrientedBytes[_src].module;
            _orientation = kOrientedBytes[_src].orientation;
            return true;
        }
        
        static MarchingCubeDefines()
        {
            List<byte> iteratedBytes = new List<byte>();

            Qube<bool> byteQube = default;
            for (int i = byte.MinValue+1; i <= byte.MaxValue-1; i++)
            {
                var cubeByte = (byte)i;
                byteQube.SetByteElement(cubeByte);
                bool predicted = false;

                var module = cubeByte;
                var rotationCW = 0;
                for (byte j = 1; j <= 3; j++)
                {
                    var orientedPrediction = byteQube.RotateYawCW(j);
                    var modulePrediction = orientedPrediction.ToByte();
                    if (iteratedBytes.Contains(modulePrediction))
                    {
                        module = modulePrediction;
                        rotationCW = 4 - j;
                        predicted = true;
                        break;
                    }
                }

                if(!predicted)
                    iteratedBytes.Add(module);

                kOrientedBytes[i]=(module,rotationCW);
            }
        }
    }
    
}