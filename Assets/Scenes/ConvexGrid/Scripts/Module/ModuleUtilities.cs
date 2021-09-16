using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Geometry.Pixel;
using Geometry.Voxel;
using Procedural;
using UnityEngine;

namespace ConvexGrid
{
    public static class UModule
    {
        public static readonly GQuad unitGQuad = new GQuad( Vector3.right+Vector3.back,Vector3.back+Vector3.left, Vector3.left+Vector3.forward ,Vector3.forward+Vector3.right).Resize<GQuad,Vector3>(.5f);
        public static readonly G2Quad unitG2Quad = unitGQuad.ConvertToG2Quad(p=>p.ToCoord());
        public static readonly GQube unitQube = unitGQuad.ExpandToQUbe(Vector3.up,0f);
        
        public static readonly Quaternion[] m_QuadRotations = {Quaternion.Euler(0f,180f,0f),Quaternion.Euler(0f,270f,0f),Quaternion.Euler(0f,0f,0f),Quaternion.Euler(0f,90f,0f)};

        public static Vector3 ObjectToModuleVertex(Vector3 _srcVertexOS)
        {
            var uv=unitG2Quad.GetUV<G2Quad, Vector2>(new Vector2(_srcVertexOS.x,_srcVertexOS.z));
            return new Vector3(uv.x,_srcVertexOS.y,uv.y);
        }
        public static Vector3 ModuleToObjectVertex(int index,Vector3 _srcVertexMS, G2Quad[] _moduleShapes,float _height)
        {
            ref var quad = ref _moduleShapes[index % 4];
            
            var point = quad.GetPoint<G2Quad,Vector2>(new Vector2(_srcVertexMS.x,_srcVertexMS.z));
            var offset = index < 4 ? -1 : 0;
            return new Vector3(point.x,offset*_height + _srcVertexMS.y*_height,point.y);
        }
    }
}