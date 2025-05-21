using Examples.Algorithm.SpatialHashGrid;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Procedural.Tile;
using Unity.Mathematics;
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

        #if UNITY_EDITOR
        private void OnEnable() => SceneView.duringSceneGui += OnSceneGUI;
        private void OnDisable() => SceneView.duringSceneGui -= OnSceneGUI;

        private void OnSceneGUI(SceneView _sceneView)
        {
            GRay ray = _sceneView.camera.ScreenPointToRay(UnityEditor.Extensions.UECommon.GetScreenPoint(_sceneView));
            GPlane plane = new GPlane(Vector3.up, transform.position);
            ray.IntersectPoint(plane,out var hitPoint);

            Handles.matrix = transform.localToWorldMatrix * Matrix4x4.Scale(Vector3.one);

            m_Grid.PositionToNode(hitPoint,out var origin);
            for(int i=-kSenseRadius;i<=kSenseRadius;i++)
            for (int j = -kSenseRadius; j <= kSenseRadius; j++)
            {
                m_Grid.NodeToPosition(new int2(i,j)+origin,out var pos);
                Handles.DrawWireCube(pos,kFlatCube);
            }

            Handles.color = Color.green;
            m_Grid.NodeToPosition(origin,out var disk);
            Handles.DrawWireDisc(disk,Vector3.up,5*kSize);
            foreach (var range in UTile.GetAxisRange(origin, 5))
            {
                m_Grid.NodeToPosition(range, out var pos);
                Handles.DrawWireCube(pos,Vector3.one);
            }
            
        }
        #endif
    }

}
