using System;
using System.Collections.Generic;
using System.Linq.Extensions;
using System.Reflection;
using UnityEngine;

namespace Dome
{
    using static ADomeController;
    public abstract class ADomeController:MonoBehaviour
    {
        public static Dictionary<Type, ADomeController> m_Controllers; 
        protected static T Refer<T>() where T : ADomeController => m_Controllers[typeof(T)] as T;
        
        public abstract void OnInitialized();
        public virtual void OnCreated(){}
        public abstract void Tick(float _deltaTime);
        public abstract void Dispose();

        public void FireEvent(string _eventName,params object[] _parameters)
        {
            foreach (var controller in m_Controllers.Values)
            {
                var method = controller.GetType().GetMethod(_eventName,BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (method != null)
                    method.Invoke(controller, _parameters);
            }    
        }
    }

    // [ExecuteInEditMode]
    public class DomeManager : MonoBehaviour
    {
        private void Awake()
        {
            m_Controllers = new Dictionary<Type, ADomeController>();
            foreach (var controller in transform.GetComponentsInChildren<ADomeController>())
                m_Controllers.Add(controller.GetType(),controller);

            m_Controllers.Values.Traversal(_p=>_p.OnInitialized());
            m_Controllers.Values.Traversal(_p=>_p.OnCreated());
        }

        private void Update()
        {
            float deltaTime = UTime.deltaTime;
            m_Controllers.Values.Traversal(p=>p.Tick(deltaTime));
        }

        private void OnDestroy()
        {
            m_Controllers.Values.Traversal(p=>p.Dispose());
            FAssets.Dispose();
        }
    }
}