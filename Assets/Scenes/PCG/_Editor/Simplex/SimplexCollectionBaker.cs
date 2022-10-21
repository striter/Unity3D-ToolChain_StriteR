#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Geometry.Voxel;
using MeshFragment;
using PCG.Simplex;
using PCG.Module.Cluster;
using UnityEditor;
using UnityEditor.Extensions;
using UnityEngine;

namespace PCG.Baking
{
    public class SimplexCollectionBaker : MonoBehaviour
    {
        public void OnDrawGizmos()
        {
            int index = 0;
            foreach (var child in transform.GetSubChildren())
            {
                child.localPosition = Vector3.right * index * 30;

                foreach (var indexer in DSimplex.kIndexes)
                {
                    if(indexer.index==-1)
                        continue;
                    var simplexElementRoot = child.GetChild(indexer.index);
                    Gizmos.matrix = simplexElementRoot.localToWorldMatrix;
                    Gizmos.color = Color.white;
                    Gizmos.DrawWireCube(Vector3.up*.5f,Vector3.one);
                    Gizmos_Extend.DrawString(Vector3.zero, indexer.srcByte.ToString(),.5f);
                    for (int i = 0; i < 8; i++)
                    {
                        Gizmos.color = (UByte.PosValid(indexer.srcByte, i) ? Color.green : Color.red).SetAlpha(.5f);
                        Gizmos.DrawWireSphere(KQube.kUnitQubeBottomed[i],.05f);
                    }
                }

                index++;
            }
        }
    }

    [CustomEditor(typeof(SimplexCollectionBaker))]
    public class SimplexCollectionBakerEditor : Editor
    {
        private SimplexCollectionBaker m_Baker;
        private void OnEnable()=>m_Baker=(target as SimplexCollectionBaker);
        private void OnDisable()=>m_Baker = null;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.BeginVertical();
            GUILayout.Space(10f);
            GUILayout.Label("Template",UEGUIStyle_Window.m_TitleLabel);
            if (GUILayout.Button("Default Simplex"))
                ImportModule();
            
            GUILayout.Space(10f);
            GUILayout.Label("Persistent",UEGUIStyle_Window.m_TitleLabel);
            if(GUILayout.Button("Output"))
                Bake();
            
            GUILayout.EndVertical();
        }
        private void Bake()
        {
            if (!UEAsset.SaveFilePath(out string filePath, "asset",$"SimplexCollection_Default")) 
                return;

            SimplexCollection collectionData = CreateInstance<SimplexCollection>();
            List<Material> materials = new List<Material>();

            var root = m_Baker.transform;
            SimplexData[] data = new SimplexData[root.childCount];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = new SimplexData();
                var bakeRoot = root.GetChild(i);
                List<FMeshFragmentCluster> elements = new List<FMeshFragmentCluster>();
                foreach (var indexer in DSimplex.kIndexes)
                {
                    if (indexer.index==-1||indexer.orientation > 0)
                        continue;
                    var simplexRoot = bakeRoot.GetChild(indexer.index);
                    elements.Add(UMeshFragmentEditor.BakeMeshFragment(simplexRoot, ref materials,
                        DModuleCluster.ObjectToOrientedVertex));
                }

                data[i].m_Name = bakeRoot.name;
                data[i].m_ModuleData = elements.ToArray();
            }

            collectionData.m_SimplexData = data.ToArray();
            collectionData.m_MaterialLibrary = materials.ToArray();
            var assetPath = UEPath.FileToAssetPath(filePath);
            UEAsset.CreateOrReplaceMainAsset( collectionData,assetPath);
        }
        
        private void ImportModule()
        {
            var simplexRoot = new GameObject("Default").transform;
            simplexRoot.SetParent(m_Baker.transform);
            simplexRoot.transform.localPosition = Vector3.zero;
            
            int localWidth = -4;
            int localHeight = 0;
            foreach (var moduleByte in DSimplex.kIndexes)
            {
                if(moduleByte.index==-1 || moduleByte.orientation>0)
                    continue;
                
                var simplexElementRoot = new GameObject(moduleByte.srcByte.ToString()).transform;
                simplexElementRoot.SetParent(simplexRoot);
                simplexElementRoot.localPosition = Vector3.right * (3f * localWidth) + Vector3.forward * (3f * localHeight);
                // var relation = KQube.False;
                // relation.SetByteElement(moduleByte.srcByte);
                // for (int j = 0; j < 8; j++)
                // {
                //     if (!relation[j])
                //         continue;
                //
                //     var subCube = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                //     subCube.SetParent(simplexElementRoot);
                //     subCube.localScale = Vector3.one * .5f;
                //     subCube.localPosition = KQube.kHalfUnitQubeBottomed[j] + Vector3.up * .25f;
                // }
                
                localWidth++;
                if (localWidth > 3)
                {
                    localWidth = -4;
                    localHeight++;
                }
            }
            Undo.RegisterCreatedObjectUndo(simplexRoot.gameObject,"Module Data Baker");
            Selection.activeObject = simplexRoot.gameObject;
        }
    }
}
#endif
