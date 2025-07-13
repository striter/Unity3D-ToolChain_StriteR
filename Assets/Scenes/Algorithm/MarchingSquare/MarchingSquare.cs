using System;
using System.Collections.Generic;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using TPool;
using System.Linq.Extensions;
using Runtime.TouchTracker;
using UnityEngine;

namespace Examples.Algorithm.MarchingSquare
{
    public class MarchingSquare : MonoBehaviour
    {
        public Int2 m_Size=Int2.kOne;
        private GameObjectPool<Int2, Node> m_Nodes;
        private GameObjectPool<Int2, Square> m_Squares;
        private void Awake()
        {
            m_Nodes = new GameObjectPool<Int2, Node>(new Node(transform.Find("Nodes/Node")));
            m_Squares = new GameObjectPool<Int2, Square>(new Square(transform.Find("Squares/Square")));
            TouchConsole.Command("Clear",KeyCode.R).Button(()=>Initialize(m_Size));
            TouchConsole.Command("Random",KeyCode.T).Button(Random);
            
            Initialize(m_Size);
        }
        
        void Initialize(Int2 _size)
        {
            m_Nodes.Clear();
            for (int i = 0; i < _size.x; i++)
            {
                for (int j = 0; j < _size.y; j++)
                {
                    Int2 nodeID = new Int2(i, j);
                    m_Nodes.Spawn(nodeID);
                }
            }

            m_Squares.Clear();
            for (int i = 0; i < _size.x - 1; i++)
            {
                for (int j = 0; j < _size.y - 1; j++)
                {
                    Quad<Int2> nodes=new Quad<Int2>(new Int2(i,j),new Int2(i,j+1),new Int2(i+1,j+1),new Int2(i+1,j));
                    Int2 squareID = new(i, j);
                    m_Squares.Spawn(squareID).Init(squareID, nodes);
                }
            }

            foreach (var square in m_Squares)
                square.Refresh(m_Nodes.m_Dic);
        }

        void Random()
        {
            int halfLength = m_Nodes.Count / 2;
            int randomCount = halfLength * URandom.RandomInt(halfLength);
            for (int i = 0; i < randomCount; i++)
            {
                SwitchNode(m_Nodes.m_Dic.RandomKey());
            }
            
        }

        private void Update()
        {
            var trackData = TouchTracker.Execute(Time.unscaledDeltaTime);
            foreach (var click in trackData.ResolveClicks(.1f))
            {
                var ray = (GRay)Camera.main.ScreenPointToRay(click);
                if (!ray.IntersectPoint(GPlane.kUp,out var point))
                    continue;
                var switchNode=m_Nodes.Last(p=>p.transform.position,point,true);
                SwitchNode(switchNode.identity);
            }
        }

        void SwitchNode(Int2 _index)
        {
            var switchNode = m_Nodes[_index];
            switchNode.Switch();
            foreach (var square in m_Squares.Collect(p=>p.m_Nodes.IterateContains(switchNode.identity)))
                square.Refresh(m_Nodes.m_Dic);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (m_Squares == null)
                return;
            foreach (var squares in m_Squares)
                UGizmos.DrawString(squares.m_Byte.ToString(), squares.m_Position);
        }
#endif
    }

    public class Node : APoolElement<Int2>
    {
        public bool m_Available { get; private set; }
        private MeshRenderer m_Renderer;
        private MaterialPropertyBlock m_Properties;
        private static readonly int ID_Color = Shader.PropertyToID("_Color");

        public Node(Transform _transform) : base(_transform)
        {
            m_Properties = new MaterialPropertyBlock();
            m_Renderer = _transform.GetComponent<MeshRenderer>();
        }

        public override void OnPoolSpawn()
        {
            base.OnPoolSpawn();
            transform.localPosition = new Vector3(identity.x, 0f, identity.y);
            Set(false);
        }

        public void Switch() => Set(!m_Available);

        void Set(bool _available)
        {
            m_Available = _available;
            m_Properties.SetColor(ID_Color, m_Available ? Color.green : Color.red);
            m_Renderer.SetPropertyBlock(m_Properties);
        }

    }
    public class Square:ITransform
    {
        public Transform transform { get; }
        
        public Vector3 m_Position;
        public Quad<Int2> m_Nodes;
        public byte m_Byte;
        private readonly MeshRenderer m_Renderer;
        private readonly MeshFilter m_Filter;
        private readonly Mesh m_Mesh;
        public Square(Transform _transform)
        {
            transform = _transform;
            m_Renderer = _transform.GetComponent<MeshRenderer>();
            m_Filter = _transform.GetComponent<MeshFilter>();
            m_Mesh = new Mesh(){hideFlags = HideFlags.HideAndDontSave,name = "Marching Shape"};
            m_Filter.sharedMesh = m_Mesh;
        }

        public void Init(Int2 _identity,Quad<Int2> _nodes)
        {
            m_Nodes = _nodes;
            m_Byte = byte.MinValue;
            transform.position = new Vector3(_identity.x + .5f, 0f, _identity.y + .5f);
        }

        public void Refresh(Dictionary<Int2,Node> _nodes)
        {
            Quad<bool> squareByte = m_Nodes.Convert(p => _nodes[p].m_Available);
            m_Byte = squareByte.ToByte();
            if (m_Byte==byte.MinValue)
            {
                m_Renderer.enabled = false;
                return;
            }

            bool b = squareByte[EQuadCorner.B];
            bool l = squareByte[EQuadCorner.L];
            bool f= squareByte[EQuadCorner.F];
            bool r = squareByte[EQuadCorner.R];
            bool bl = ( b || l ) && (!b || !l);
            bool lf = ( l || f ) && (!l || !f);
            bool fr = ( f || r ) && (!f || !r);
            bool rb = ( r || b ) && (!r || !b);
            
            m_Renderer.enabled = true;
            m_Mesh.Clear();
            List<Vector3> vertices = new List<Vector3>();
            if(b) vertices.Add(Vector3.back*.5f+Vector3.left*.5f);
            if(bl) vertices.Add(Vector3.left*.5f);
            if(l) vertices.Add(Vector3.forward*.5f+Vector3.left*.5f);
            if(lf) vertices.Add(Vector3.forward*.5f);
            if(f) vertices.Add(Vector3.forward*.5f+Vector3.right*.5f);
            if(fr) vertices.Add(Vector3.right*.5f);
            if(r) vertices.Add(Vector3.back*.5f+Vector3.right*.5f);
            if(rb) vertices.Add(Vector3.back*.5f);
            List<int> indices = new List<int>();
            switch (vertices.Count)
            {
                case 3:  indices.AddRange(new[]{0,1,2});  break;
                case 4:  indices.AddRange(new[]{0,1,2,2,3,0});  break;
                case 5:  indices.AddRange(new[]{0,1,2,2,3,0,3,4,0});  break;
                case 6:  indices.AddRange(new[]{0,1,2,2,3,0,0,3,4,4,5,0});  break;
            }
            
            m_Mesh.SetVertices(vertices);
            m_Mesh.SetIndices(indices,MeshTopology.Triangles,0,true);
        }

    }
}