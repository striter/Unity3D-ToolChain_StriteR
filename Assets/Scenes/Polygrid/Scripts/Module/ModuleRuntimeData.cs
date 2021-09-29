using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace  PolyGrid.Module
{
    [Serializable]
    public struct OrientedModuleMeshData
    {
        public Vector3[] m_Vertices;
        public Vector2[] m_UVs;
        public int[] m_Indexes;
        public Vector3[] m_Normals;
        public Color[] m_Colors;
    }

    [Serializable]
    public class ModuleRuntimeData : ScriptableObject
    {
        public EModuleType m_Type;
        public ECornerStatus m_AvailableStatus;
        public OrientedModuleMeshData[] m_OrientedMeshesTop;
        public OrientedModuleMeshData[] m_OrientedMeshesMedium;
        public OrientedModuleMeshData[] m_OrientedMeshesBottom;
        
        public OrientedModuleMeshData[] this[ECornerStatus _status]
        {
            get
            {
                if (!m_AvailableStatus.IsFlagEnable(_status))
                    _status = ECornerStatus.Body;
                switch (_status)
                {
                    default: throw new InvalidEnumArgumentException();
                    case ECornerStatus.Rooftop: return m_OrientedMeshesTop;
                    case ECornerStatus.Bottom: return m_OrientedMeshesBottom;
                    case ECornerStatus.Body: return m_OrientedMeshesMedium;
                }
            }
            set
            {
                switch (_status)
                {
                    default: throw new InvalidEnumArgumentException();
                    case ECornerStatus.Rooftop: m_OrientedMeshesTop = value; break;
                    case ECornerStatus.Bottom: m_OrientedMeshesBottom= value; break;
                    case ECornerStatus.Body: m_OrientedMeshesMedium= value; break;
                }
            }
        }
    }
}