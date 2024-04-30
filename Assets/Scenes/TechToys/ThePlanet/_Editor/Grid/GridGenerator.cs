
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using System.Linq.Extensions;

namespace TechToys.ThePlanet.Grid
{
    using UnityEditor;
    using UnityEditor.Extensions;
    using UnityEditor.Extensions.EditorPath;
    
    [ExecuteInEditMode]
    public class GridGenerator : MonoBehaviour
    {
        public EGridType m_GridType;
        [MFoldout(nameof(m_GridType),EGridType.DisorderedGrid)] public DisorderedHexagonGridGenerator m_DisorderedHexagonGrid = new DisorderedHexagonGridGenerator();
        [MFoldout(nameof(m_GridType),EGridType.SphericalGrid)] public SphericalGridGenerator m_SphericalGrid = new SphericalGridGenerator();
        private Dictionary<EGridType, IGridGenerator> m_Grids;

        private void OnEnable()
        {
            if (Application.isPlaying)
                return;
            
            m_Grids = new Dictionary<EGridType, IGridGenerator>()
            {
                {EGridType.DisorderedGrid,m_DisorderedHexagonGrid},
                {EGridType.SphericalGrid,m_SphericalGrid}
            }; 
            Setup();
            EditorApplication.update += EditorTick;
            SceneView.duringSceneGui += OnSceneGUI;
        }
        

        private void OnValidate()
        {
            if(m_Grids==null)
                return;
            
            Setup();
            Clear();
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
                return;
        
            Clear();
            EditorApplication.update -= EditorTick;
            SceneView.duringSceneGui -= OnSceneGUI;
        }
        
        void Setup()
        {
            m_Grids.Values.Traversal(p =>
            {
                p.transform = transform;
                p.Setup();
            });
        }

        private void EditorTick() => m_Grids.Values.Traversal(p => p.Tick(UTime.deltaTime));
        private void Clear()=>m_Grids.Values.Traversal(p => p.Clear());
        private void OnSceneGUI(SceneView _sceneView)=>m_Grids.Values.Traversal(p => p.OnSceneGUI(_sceneView));
        
                
        private void OnDrawGizmos()
        {
            m_Grids.Values.Traversal(p => p.OnGizmos());
        }

        public void Output()
        {
            if (!UEAsset.SaveFilePath(out string filePath, "asset")) 
                return;

            GridCollection data = ScriptableObject.CreateInstance<GridCollection>();
            m_Grids[m_GridType].Output(data);
            UEAsset.CreateOrReplaceMainAsset(data,UEPath.FileToAssetPath( filePath));
        }
    }

    [CustomEditor(typeof(GridGenerator))]
    public class GridGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Bake"))
                (target as GridGenerator).Output();
            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif