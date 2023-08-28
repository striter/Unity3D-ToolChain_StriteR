using System.Collections.Generic;
using Dome.Model;
using TPool;
using Unity.Mathematics;
using UnityEngine;

namespace Dome.Entity
{
    public class DomeEntityContainer : APoolTransform<int>
    {
        public ADomeEntity m_Entity;
        
        public DomeEntityContainer(Transform _transform) : base(_transform)
        {
            
        }

        public void Initialize(ADomeEntity _entity,EntityInitializeParameters _parameters)
        {
            m_Entity = _entity;
            m_Entity.OnInitialize(this,_parameters);
            m_Entity.OnCreate();
        }

        public void Tick(float _deltaTime)
        {
            m_Entity.Tick(_deltaTime);
        }

        public override void OnPoolRecycle()
        {
            base.OnPoolRecycle();
            m_Entity.OnRecycle();
            m_Entity = null;
        }

        public override void OnPoolDispose()
        {
            base.OnPoolDispose();
            if (m_Entity == null)
                return;
            m_Entity.Dispose();
            m_Entity = null;
        }
    }
    
    public abstract class ADomeEntity : IEntity , IFireEvent
    {
        public static EntityDefines GetDefines(string _defineName)
        {
            if(KDomeEntities.kEntities.TryGetValue(_defineName,out var initializeParam))
                return initializeParam;
            Debug.LogWarning($"Invalid Define Found For {_defineName}");
            return EntityDefines.kDefault;
        }

        public object[] kDefaultParameters { get; }
        protected ADomeEntity() {
            kDefaultParameters = new object[] {this};
        }
        
        
        private DomeEntityContainer m_Parent;
        public virtual void OnInitialize(DomeEntityContainer _container,EntityInitializeParameters _parameters)
        {
            m_Parent = _container;
            this.FireEvents("OnInitialize",_parameters);
        }

        public virtual void OnCreate()
        {
            this.FireEvents("OnCreate");
        }
        
        public virtual void Tick(float _deltaTime)
        {
            this.FireEvents("Tick",_deltaTime);
        }

        public virtual void OnRecycle()
        {
            this.FireEvents("OnRecycle");
        }
        
        public virtual void Dispose() 
        {
            this.FireEvents("OnDispose");
        }

        
        public string define { get; set; }
        public int id => m_Parent.identity;
        public Transform transform => m_Parent.transform;
        public float3 position { get; set; }
        public quaternion rotation { get; set; }
        public List<int> relativeEffects { get; set; }
    }


    public interface IEntityPersistent<T> where T:struct
    {
        public T Data { get; }
    }
    
    public abstract class ADomeEntity<T,Y> : ADomeEntity where T:MonoBehaviour,IEntityPersistent<Y> where Y:struct
    {
        private T m_Persistent;
        protected Y Data => m_Persistent ? m_Persistent.Data : UReflection.GetDefaultData<Y>();
        public override void OnCreate()
        {
            base.OnCreate();
            if (this is IModel model)
            {
                if (model.modelRoot) 
                    m_Persistent = model.modelRoot.GetComponent<T>();
            }
        }
    }
}