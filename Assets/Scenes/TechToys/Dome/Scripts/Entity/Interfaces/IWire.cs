using System.Linq.Extensions;
using Dome.Model;
using Runtime;
using UnityEngine;

namespace Dome.Entity
{
    public interface IWire : IModel , ITeam , IEntity
    {
        public Transform wireNode { get; set; }
    }

    public interface IWireStart : IWire
    {
        
    }

    public interface IWireEnd : IWire
    {
        public IWireStart connecting { get; set; }
        public RopeRenderer wireRoot { get; set; }
    }

    public static class IWire_Extension
    {
        public static void OnModelSet(this IWire _entity,IModel _)
        {
            _entity.wireNode = _entity.modelRoot.transform.FindInAllChild(p => p.name == "Rig_Wire")??_entity.transform;    //Could be attached to the root.
        }
    }
    
    public static class IWireModel_Extension
    {
        public static bool isWireAttaching(this IWireEnd _entity) => _entity.wireRoot != null;
        private static readonly string kWire = FAssets.PrecacheAsset("Wire");
        public static void OnModelSet(this IWireEnd _entity,IModel _)
        {
            var nearestSource = IEntity.kEntities.GetEntities<IWireStart>(FDomeEntityFilters.FilterTeams[_entity.team])
                .MinElement(FDomeEntityFilters.GetDistanceToOrigin(_entity.position));
            if (nearestSource==null) return;
            AttachWire(_entity,nearestSource as IWireStart);
        }

        public static void Tick(this IWireEnd _entity, float _deltaTime)
        {
            if (!_entity.isWireAttaching()) return;
            _entity.wireRoot.transform.position = _entity.wireNode.position;
            _entity.wireRoot.m_EndPosition =_entity.connecting.wireNode.position;
        }
        
        public static void OnModelClear(this IWireEnd _entity)
        {
            _entity.DetachWire();
        }

        public static void AttachWire(this IWireEnd _entity, IWireStart _target)
        {
            _entity.DetachWire();
            _entity.connecting = _target;
            _entity.wireRoot = FAssets.GetModel(kWire).GetComponent<RopeRenderer>();
            _entity.wireRoot.transform.position = _entity.position;
            _entity.wireRoot.m_EndPosition = _target.wireNode.position;

            _entity.wireRoot.m_Extend = 0;
            _entity.wireRoot.Initialize();
            _entity.wireRoot.m_Extend = 6;
        }

        public static void DetachWire(this IWireEnd _entity)
        {
            if (!_entity.isWireAttaching()) return;
            FAssets.ClearModel(kWire, _entity.wireRoot.gameObject);
            _entity.connecting = null;
            _entity.wireRoot = null;
        }
    }
}