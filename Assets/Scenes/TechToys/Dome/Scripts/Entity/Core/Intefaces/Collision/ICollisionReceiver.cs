using System.Collections.Generic;
using Dome.Entity;
using Dome.Model;
using UnityEngine;

namespace Dome.Collision
{
    
    public interface ICollisionReceiver : IEntity
    {
        public Collider[] colliders { get; set; }
        public static Dictionary<int, int> kColliderIndexes = new Dictionary<int, int>();
    }
    
    public static class ICollisionReceiver_Extension
    {
        public static void OnModelSet(this ICollisionReceiver _receiver,IModel _model)
        {
            _receiver.colliders = _model.modelRoot.GetComponentsInChildren<Collider>();
            foreach (var collider in _receiver.colliders)
                ICollisionReceiver.kColliderIndexes.Add(collider.GetInstanceID(),_receiver.id);
        }

        public static void OnModelClear(this ICollisionReceiver _receiver)
        {
            foreach (var collider in _receiver.colliders)
                ICollisionReceiver.kColliderIndexes.Remove(collider.GetInstanceID());
            _receiver.colliders = null;
        }
        
    }
    
}