using System;
using System.Collections.Generic;
using Dome.Entity;
using Unity.Mathematics;
using UnityEngine;

namespace Dome
{
    public enum EGameState
    {
        MainMenu,
        Countdown,
        GameStart,
        GameEnd,
    }

    public static class KDomeLayers
    {
        public static int kEntities = LayerMask.NameToLayer("Entity");
        public static int kHighlight = LayerMask.NameToLayer("Highlight");
    }

    public struct EntityInitializeParameters
    {
        public string defineID;
        public TR transformTR;
        public ETeam team;
        public int ownerId;
        public EntityDefines defines;
    }
    
    public struct EntityDefines
    {
        public string modelPath;
        public float maxHealth;
        public int cost;
        public int2 size;
        public Type type;

        public static readonly EntityDefines kDefault = new EntityDefines() {
            maxHealth = 100,
            cost = 1,
            size = 1,
            modelPath = FAssets.PrecacheAsset("ERROR"),
        };
    }
    
    public static class KDomeEntities
    {
        public static readonly Dictionary<string, EntityDefines> kEntities = new() {
            {"Commander", new EntityDefines(){type = typeof(FDomeCommander)}},
            {"Bullet",new (){type = typeof(ADomeProjectile)}},
            {"Missile",new (){type = typeof(ADomeProjectile)}},
        };
        
        public static class ARC
        {
            public static readonly string kRoot = "ARC";
            public static readonly Type kDefaultType = typeof(ADomeARC);
            static ARC()
            {
                kEntities.Add("AAntiAir",new() {modelPath = $"{kRoot}/AA", cost = 5,maxHealth = 100,type = kDefaultType});
                kEntities.Add("AMBT",new() {modelPath = $"{kRoot}/MBT", cost = 5 , maxHealth = 100,type = kDefaultType});
                kEntities.Add("AScout",new() {modelPath = $"{kRoot}/Scout", cost = 3 , maxHealth = 100,type = kDefaultType});
            }
        }

        public static class Turrets
        {
            public static readonly string kRoot = "Turret";
            public static readonly Type kDefaultType = typeof(FDomeTurret);
            static Turrets()
            {
                kEntities.Add("TCannon",new() {modelPath = $"{kRoot}/Cannon", cost = 3,maxHealth = 500,type = kDefaultType});
                kEntities.Add("TGatling",new() {modelPath = $"{kRoot}/Gatling2x", cost = 3,maxHealth = 500,type = kDefaultType});
            }
        }

        public static class Building
        {
            public static readonly string kRoot = "Building";
            public static readonly Type kDefaultType = typeof(ADomeStructure);

            static Building()
            {
                kEntities.Add("BCommandStation",new() {modelPath = $"{kRoot}/Start_A", cost = 2,size = 2,maxHealth = 1000,type = typeof(FDomeCommandStation)});
                kEntities.Add("BResourceTower",new() {modelPath = $"{kRoot}/POC", cost = 2,size = 2,maxHealth = 800,type = kDefaultType});
                kEntities.Add("BCapture", new() {modelPath = $"{kRoot}/Booth_C", cost = 2, size = 2,maxHealth = 80,type = kDefaultType});
                // kEntities.Add("BStart",new() {modelPath = $"{kRoot}/Start_B", cost = 2,size = 2});
            }
        }

    }
    
    public static class KDomeEvents
    {
        public static readonly string kOnEntitySpawn = "OnEntitySpawn";
        public static readonly string kOnEntityRecycle = "OnEntityRecycle";

        public static readonly string kOnEntityControlChanged = "OnEntityControlChanged";
        public static readonly string kOnGameStateChanged = "OnGameStateChanged";
        
    }
}