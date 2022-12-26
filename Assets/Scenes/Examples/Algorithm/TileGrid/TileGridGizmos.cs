using System;
using Geometry;
using Geometry.Validation;
using Procedural;
using Procedural.Tile;
using UnityEditor;
using UnityEngine;

namespace Examples.Algorithm.TileGrid
{
    using static KTileGrid;
    static class KTileGrid
    {
        public const int kSize = 2;
        public static readonly Vector3 kFlatCube = new Vector3(1f, 0f, 1f) * kSize;
        public const int kSenseRadius = 10;
    }
    
    [ExecuteInEditMode]
    public class TileGridGizmos : MonoBehaviour
    {
        private TileGraph m_Grid = new TileGraph(kSize);

        private void OnEnable() => SceneView.duringSceneGui += OnSceneGUI;
        private void OnDisable() => SceneView.duringSceneGui -= OnSceneGUI;

        private void OnSceneGUI(SceneView _sceneView)
        {
            GRay ray = _sceneView.camera.ScreenPointToRay(UnityEditor.Extensions.UECommon.GetScreenPoint(_sceneView));
            GPlane plane = new GPlane(Vector3.up, transform.position);
            UGeometryValidation.Ray.Projection(ray,plane,out var hitPoint);

            Handles.matrix = transform.localToWorldMatrix * Matrix4x4.Scale(Vector3.one);

            Int2 origin = m_Grid.GetNode(hitPoint);
            for(int i=-kSenseRadius;i<=kSenseRadius;i++)
                for(int j=-kSenseRadius;j<=kSenseRadius;j++)
                    Handles.DrawWireCube(m_Grid.ToNodePosition(new Int2(i,j)+origin),kFlatCube);

            Handles.color = Color.green;
            Handles.DrawWireDisc(m_Grid.ToNodePosition(origin),Vector3.up,5*kSize);
            foreach (var range in UTile.GetAxisRange(origin,5))
                Handles.DrawWireCube(m_Grid.ToNodePosition(range),Vector3.one);
            
        }
    }

}
