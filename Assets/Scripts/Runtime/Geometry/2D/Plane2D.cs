using System;
using Unity.Mathematics;
using UnityEngine;

namespace Geometry
{
    [Serializable]
    public partial struct G2Plane :ISerializationCallbackReceiver
    {
        public static readonly G2Plane kDefault = new(new float2(0,1),0f);
        
        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            normal = math.normalize(normal);
            Ctor();
        }
        public static implicit operator float3(G2Plane _plane)=>_plane.normal.to3xy(_plane.distance);
        public bool IsPointFront(float2 _point) =>  math.dot(_point.to3xy(-1),this)>0;
    }

}