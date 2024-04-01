using System.Collections.Generic;
using Dome.Collision;
using Dome.Model;
using Unity.Mathematics;
using UnityEngine;

namespace Dome.Entity
{
    public class ADomeProjectile : ADomeEntity , IVelocityMove , IModel , IOwner , ITeam , ICollisionCaster , IEffect , IProjectile
    {
        public string modelPath { get; set; }
        public GameObject modelRoot { get; set; }
        public MeshRenderer[] meshRenderers { get; set; }
        public Material[] restoreMaterials { get; set; }
        public Dictionary<string, Transform> modelNodes { get; set; }
        public float3 lastPosition { get; set; }
        public float3 velocity { get; set; }
        public int ownerId { get; set; }
        public ETeam team { get; set; }
        public float timeToRecycle { get; set; }
        
        public float kStartSpeed => 120f;
        public ECollisionCasterData kProjectileSize => default;
        public float kMaxLastingTime => 5f;
        public string kMuzzleParticleName => "Bullet_BlazingRed_Big_MuzzleFlare";
        public string kProjectileTrail => "Bullet_BlazingRed_Big_Projectile";
        public string kProjectileImpact => "Bullet_BlazingRed_Big_Impact";

    }
}