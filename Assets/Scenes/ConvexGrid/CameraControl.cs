using System;
using System.Collections;
using System.Collections.Generic;
using Procedural.Hexagon;
using TTouchTracker;
using UnityEngine;

namespace ConvexGrid
{
    public class CameraControl : IConvexGridControl
    {
        private Transform m_CameraRoot;
        public  Camera m_Camera { get; private set; }
        private Vector3 m_RootPosition;
        public float m_Pitch;
        public float m_Yaw;
        public float m_Offset;

        public void Init(Transform _transform)
        {
            m_Camera = _transform.Find("Camera").GetComponent<Camera>();
            Clear();
        }

        public void Move(Vector2 _delta)
        {
            m_RootPosition += Quaternion.Euler(0, m_Yaw, 0) * new Vector3(_delta.x,0,_delta.y);
        }

        public void Rotate(float _pitch,float _yaw)
        {
            m_Pitch =Mathf.Clamp(m_Pitch + _pitch, 1, 89);
            m_Yaw += _yaw;
        }

        public void Pinch(float _delta)
        {
            m_Offset = Mathf.Clamp( m_Offset + _delta,20f,80f);
        }
        
        public void Tick(float _deltaTime)
        {
            var rotation = Quaternion.Euler(m_Pitch,m_Yaw,0);
            var position = m_RootPosition + rotation * Vector3.forward * -m_Offset;
            m_Camera.transform.SetPositionAndRotation(Vector3.Lerp(m_Camera.transform.position,position,_deltaTime*10f),
                Quaternion.Slerp(m_Camera.transform.rotation,rotation,_deltaTime*10f));
        }

        public void OnSelectVertex(ConvexVertex _vertex)
        {
            m_RootPosition = _vertex.m_Coord.ToWorld();
        }

        public void OnAreaConstruct(ConvexArea _area)
        {
        }
        
        public void Clear()
        {
            m_RootPosition = Vector3.zero;
            m_Pitch = 45;
            m_Yaw = 0f;
            m_Offset = 50f;
        }
        
    }
}