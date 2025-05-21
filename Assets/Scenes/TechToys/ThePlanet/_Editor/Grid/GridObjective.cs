using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace TechToys.ThePlanet
{
    public interface IGridGenerator
    {
        Transform transform { get; set; }
        void Setup();
        void Tick(float _deltaTime);
        void Clear();
#if UNITY_EDITOR
        void OnSceneGUI(SceneView _sceneView);
#endif
        void OnGizmos();
        void Output(GridCollection _collection);
    }

    public enum EGridType
    {
        DisorderedGrid,
        SphericalGrid,
    }
}