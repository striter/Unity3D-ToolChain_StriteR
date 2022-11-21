#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace PCG
{
    public interface IGridGenerator
    {
        Transform transform { get; set; }
        void Setup();
        void Tick(float _deltaTime);
        void Clear();
        void OnSceneGUI(SceneView _sceneView);
        void OnGizmos();
        void Output(GridCollection _collection);
    }

    public enum EGridType
    {
        DisorderedGrid,
        SphericalGrid,
    }
}
#endif