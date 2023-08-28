using System;
using System.Collections.Generic;
using Dome.Collision;
using Dome.Model;
using Runtime;
using UnityEngine;

namespace Dome.Entity
{
    [Serializable]
    public struct FDomeStructureData
    {
        public EntityExtentData extentData;

        public static FDomeStructureData kDefault = new FDomeStructureData() {
            extentData = EntityExtentData.kDefault,
        };
    }

    public class DomeStructure : MonoBehaviour , IEntityPersistent<FDomeStructureData>
    {
        public FDomeStructureData m_Data = FDomeStructureData.kDefault;
        public FDomeStructureData Data => m_Data;
    }
    
    public class ADomeStructure : ADomeEntity<DomeStructure,FDomeStructureData> , ILive  , ITeam , ISelection ,IModel,  IOwner , IWire , ICollisionReceiver
    {
        public string modelPath { get; set; }
        public float kMaxHealth { get; set; }
        public float maxHealth { get; set; }
        public GameObject modelRoot { get; set; }
        public MeshRenderer[] meshRenderers { get; set; }
        public Material[] restoreMaterials { get; set; }
        public Dictionary<string, Transform> modelNodes { get; set; }

        public ETeam team { get; set; }
        public ESelections selectionFlags { get; set; }
        public GameObject hoveringObj { get; set; }
        public Animation hoveringAnimation { get; set; }
        public EntityExtentData kExtentData => Data.extentData;
        public int ownerId { get; set; }
        public Transform wireNode { get; set; }
        public Collider[] colliders { get; set; }
    }
}