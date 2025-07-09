using System;
using System.Linq.Extensions;
using CameraController;
using CameraController.Demo;
using Unity.Mathematics;
using UnityEngine;

namespace TechToys.CharacterControl 
{
    interface ICharacterControl
    {
        public void Initialize();
        public void Dispose();
        
        public void Tick(float _deltaTime);
        public void LateTick(float _deltaTime);
    }
    public class CharacterControl : MonoBehaviour
    {
        private ICharacterControl[] m_Controllers;
        private void Awake()
        {
            m_Controllers = GetComponentsInChildren<ICharacterControl>();
            m_Controllers.Traversal(p=>p.Initialize());
        }

        private void OnDestroy()
        {
            m_Controllers.Traversal(p=>p.Dispose());
        }

        void Update()
        {
            var deltaTime = Time.deltaTime;
            m_Controllers.Traversal(p=>p.Tick(deltaTime));
        }

        private void LateUpdate()
        {
            var time = Time.deltaTime;
            m_Controllers.Traversal(p=>p.LateTick(time));
        }
    }

}