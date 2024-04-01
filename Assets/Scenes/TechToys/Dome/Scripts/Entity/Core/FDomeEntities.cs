using System;
using System.Collections.Generic;
using System.Linq.Extensions;
using Dome.Entity;
using TPool;

namespace Dome.Entity
{
    public class FDomeEntities : ADomeController
    {
        private ObjectPoolClass<int,DomeEntityContainer> m_Entities;
        private List<DomeEntityContainer> m_IterateEntities = new List<DomeEntityContainer>();
        
        public override void OnInitialized()
        {
            IEntity.kEntities = this;
            m_Entities = new ObjectPoolClass<int, DomeEntityContainer>(transform.Find("Entity"));
        }

        public override void Tick(float _deltaTime)
        {
            m_IterateEntities.Clear();
            m_IterateEntities.AddRange(m_Entities.m_Dic.Values);
            m_IterateEntities.Traversal(p=>p.Tick(_deltaTime));
        }

        public override void Dispose()
        {
            m_Entities.Dispose();
        }

        public ADomeEntity Get(int _id)
        {
            if (!m_Entities.Contains(_id))
                return null;
            //Where's Assertions
            return m_Entities[_id].m_Entity;
        }
        
        public ADomeEntity Spawn(string _defineName,TR _tr,ETeam _team = default,IEntity _owner = null)
        {
            var define = ADomeEntity.GetDefines(_defineName);
            var initializeParam = new EntityInitializeParameters() {
                defineID = _defineName,
                transformTR = _tr,
                team = _team,
                ownerId =  _owner?.id ?? IEntity.kInvalidID,
                defines = define,
            };
            var entity = UReflection.CreateInstance<ADomeEntity>(define.type);
            m_Entities.Spawn().Initialize(entity,initializeParam);
            FireEvent(KDomeEvents.kOnEntitySpawn,entity);
            return entity;
        }

        public void Recycle(int _id)
        {
            var entity = m_Entities[_id].m_Entity;
            m_Entities.Recycle(_id);
            FireEvent(KDomeEvents.kOnEntityRecycle,entity);
        }

        public IEnumerable<ADomeEntity> GetEntities(Predicate<ADomeEntity> _filter = null)
        {
            foreach (var container in m_Entities)
            {
                var entity = container.m_Entity;
                if (_filter == null || _filter(entity))
                    yield return entity;
            }
        }
        public IEnumerable<ADomeEntity> GetEntities<T>(Predicate<ADomeEntity> _filter = null)
        {
            foreach (var container in m_Entities)
            {
                var entity = container.m_Entity;
                if (_filter == null || _filter(entity))
                    if(entity is T)
                        yield return entity;
            }
        }
        public IEnumerable<T> GetEntityWithMixin<T>(Predicate<ADomeEntity> _filter = null)
        {
            foreach (var container in m_Entities)
            {
                var entity = container.m_Entity;
                if (_filter == null || _filter(entity))
                    if(entity is T target)
                        yield return target;
            }
        }
        
    }

}