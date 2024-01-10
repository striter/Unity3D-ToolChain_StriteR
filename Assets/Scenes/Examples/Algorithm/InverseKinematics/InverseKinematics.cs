using System;
using System.Collections;
using System.Collections.Generic;
using Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Gizmos = UnityEngine.Gizmos;

namespace Examples.Algorithm.InverseKinematics
{
    public class InverseKinematics : MonoBehaviour
    {
        [Header("2D")]
        public float m_Radius1 = 5;
        public float m_Radius2 = 2f;

        public float2 m_Control = new float2(1, 1);

        private void DrawGizmos2D()
        {
            float2 control = m_Control + umath.Rotate2D(math.sin(UTime.time) * kmath.kPIMul2).mul( new float2(0,1));
            float2 p = Solve(control  , m_Radius1, m_Radius2);
            
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(Vector3.zero,0.1f);
            Gizmos.DrawSphere(control.to3xz(),0.1f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(p.to3xz(),0.1f);
            Gizmos.color = Color.white;
            
            Gizmos.DrawLine(Vector3.zero,p.to3xz());
            Gizmos.DrawLine(p.to3xz(),control.to3xz());
            
            new G2Circle(0,m_Radius1).DrawGizmos();
            new G2Circle(control,m_Radius2).DrawGizmos();
        }
        
        [Header("3D")] 
        public float m_Radius31 = 5;
        public float m_Radius32 = 2f;
        public float3 m_Control3 = new float3(0, -2, 2);
        public float3 m_Direction = new float3(0, 0, 1);

        private void DrawGizmos3D()
        {
            float3 control = m_Control3 + (float3)(Quaternion.Euler(math.sin(UTime.time) * 180,math.cos(UTime.time) * 180,0f)* new Vector3(0,1,0));
            float3 point = Solve(control, m_Radius31, m_Radius32, m_Direction);

            Gizmos.color = Color.white;
            Gizmos.DrawSphere(Vector3.zero,0.1f);
            Gizmos.DrawSphere(control,0.1f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(point,0.1f);
            Gizmos.color = Color.white;
            
            Gizmos.DrawLine(Vector3.zero,point);
            Gizmos.DrawLine(point,control);
            UGizmos.DrawArrow(Vector3.zero,m_Direction,.5f,.1f);
            
            new GSphere(0,m_Radius1).DrawGizmos();
            new GSphere(control,m_Radius2).DrawGizmos();
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.matrix = Matrix4x4.identity;
            DrawGizmos2D();
            Gizmos.matrix = Matrix4x4.Translate(new float3(20f, 0f, 0f));
            DrawGizmos3D();
        }


        public static float2 Solve(float2 _delta, float _r1, float _r2)
        {
            float h = math.dot(_delta,_delta);
            float w = h + _r1*_r1 - _r2*_r2;
            float s = math.max(4.0f*_r1*_r1*h - w*w,0.0f);
            return (w*_delta + new float2(-_delta.y,_delta.x)*math.sqrt(s)) * 0.5f/h;
        }
        
        public static float3 Solve(float3 _delta, float _r1, float _r2,float3 _dir)
        {
            float3 q = _delta*( 0.5f + 0.5f*(_r1*_r1-_r2*_r2)/math.dot(_delta,_delta) );
            float s = _r1*_r1 - math.dot(q,q);
            s = math.max( s, 0.0f );
            q += math.sqrt(s)*math.normalize(math.cross(_delta,_dir));
            return q;
        }
    }

}