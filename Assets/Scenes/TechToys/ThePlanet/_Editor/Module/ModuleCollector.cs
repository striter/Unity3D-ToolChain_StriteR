using System.Linq;
using System.Linq.Extensions;

#if UNITY_EDITOR
namespace TechToys.ThePlanet.Baking
{
    using Module;
    using Module.Cluster;
    using Module.Prop;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.Extensions;
    using UnityEditor.Extensions.EditorPath;
    public class ModuleCollector : MonoBehaviour
    {
        public EClusterType m_ClusterType;
        private readonly string kClusterRoot = "ClusterRoot";
        private readonly string kPathRoot = "PathRoot";
        private readonly string kDecorationRoot = "DecorationRoot";
        
        public void ImportCluster(Mesh[] _importMeshes,EClusterType _clusterType)
        {
            m_ClusterType = _clusterType;
            var clusterRoot = new GameObject(kClusterRoot).transform;
            clusterRoot.transform.SetParent(transform);
            clusterRoot.localPosition = Vector3.zero;

            int localWidth=0,localHeight=0;
            foreach (var status in DModuleCluster.GetPredefinedStatus(m_ClusterType))
            {
                Transform parent = new GameObject(status.ToString()).transform;
                parent.SetParent(clusterRoot);
                parent.localPosition=Vector3.zero;
                localWidth = -4;
                foreach (var moduleByte in UModuleClusterByte.IterateClusterBytes(status))
                {
                    var name = DBaking.GetPartPileName(status, moduleByte);
                    var bakerModel = ImportClusterModel(_importMeshes,moduleByte,name);
                    bakerModel.name = name;
                    bakerModel.SetParent(parent);
                    bakerModel.localPosition = Vector3.right * (3f * localWidth) + Vector3.forward * (3f * localHeight);
                
                    localWidth++;
                    if (localWidth > 3)
                    {
                        localWidth = -4;
                        localHeight++;
                    }
                }
                localHeight++;
            }
            
            
            var pathRoot = new GameObject(kPathRoot).transform;
            pathRoot.SetParent(transform);
            pathRoot.localPosition = Vector3.back * 2;
            localWidth = -4;
            foreach (var quadByte in UModulePropByte.IteratePathBytes())
            {
                var pathModel = new GameObject().transform;
                pathModel.gameObject.AddComponent<ModulePathCollector>().Import(_importMeshes, quadByte);
                pathModel.SetParent(pathRoot);
                pathModel.localScale=Vector3.one*2f;
                pathModel.localPosition = Vector3.right * 3 * localWidth++;
            }
            
            transform.DestroyChildrenComponent<Collider>();
        }
        Transform ImportClusterModel(Mesh[] _importMeshes,byte _qubeByte,string _moduleName)
        {
            var voxelModel = new GameObject(_moduleName).transform;
            voxelModel.gameObject.AddComponent<ModuleClusterUnitCollector>().Import(_importMeshes,_qubeByte,_moduleName);
            return voxelModel;
        }

        public void ImportDecoration(Mesh[] _meshes)
        {
            Transform parent = transform.Find(kDecorationRoot);
            if (parent == null)
            {
                parent=new GameObject(kDecorationRoot).transform;
                parent.SetParent(transform);
                parent.localPosition = Vector3.back * 4;
            }
            
            Transform decorations = new GameObject("Default").transform;
            decorations.SetParent(parent);
            decorations.localPosition = Vector3.back * 5;
            decorations.gameObject.AddComponent<ModuleDecorationCollector>().Import(m_ClusterType,_meshes);
            
            Undo.RegisterCreatedObjectUndo(decorations.gameObject,"Module Decoration Baker");
            Selection.activeObject = decorations;
        }
        
        public ModuleData Export(List<Mesh> _meshLibrary, List<Material> _materialLibrary)
        {
            ModuleData data = ScriptableObject.CreateInstance<ModuleData>();
            data.name = gameObject.name;

            uint statusMask = 0;
            var clusterRoot = transform.Find(kClusterRoot);
            data.m_ClusterData = new ModuleClusterData[UEnum.GetEnums<EClusterStatus>().Length];
            Debug.Assert(kClusterRoot!=null,$"{transform.name}:{kClusterRoot} not found");
            foreach (var status in DModuleCluster.GetPredefinedStatus(m_ClusterType))
            {
                var parent = clusterRoot.Find(status.ToString());
                if (!parent)
                {
                    Debug.LogWarning($"Cluster Skipped:{gameObject.name}|{status}");
                    continue;
                }

                data.m_ClusterData[UEnum.GetIndex(status)] = new ModuleClusterData(){
                    m_Units = parent.GetComponentsInChildren<ModuleClusterUnitCollector>().Select(p=>p.Export(_materialLibrary)).ToArray()};
                statusMask += (uint)status;
            }

            
            var pathTransform = transform.Find(kPathRoot);
            if (pathTransform != null)
            {
                data.m_Paths = new ModulePathData()
                {
                    m_Units = pathTransform.GetComponentsInChildren<ModulePathCollector>()
                        .Select(p => p.Export(ref _materialLibrary)).ToArray()
                };
            }
            var decorationTransform = transform.Find(kDecorationRoot);
            if (decorationTransform != null)
            {
                ModuleDecorationCollection collection = new ModuleDecorationCollection();
                collection.decorationSets =  decorationTransform.GetComponentsInChildren<ModuleDecorationCollector>().Select(p=>p.Export(_meshLibrary,ref _materialLibrary)).ToArray();
                data.m_Decorations = collection;
            }

            data.m_ClusterType = m_ClusterType;
            return data;
        }
    }

    [CustomEditor(typeof(ModuleCollector))]
    public class ModuleDataBakerEditor : Editor
    {
        private ModuleCollector m_Baker;
        private void OnEnable() => m_Baker = target as ModuleCollector;
        private void OnDisable() => m_Baker = null;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.BeginVertical();
            

            if (GUILayout.Button("Import Decoration"))
                ImportDecoration();
            if (GUILayout.Button("Default Decoration"))
                m_Baker.ImportDecoration(null);

            
            if (GUILayout.Button("Select Models"))
            {
                List<GameObject> selections = new List<GameObject>();
                foreach (var statusChild in m_Baker.transform.GetSubChildren())
                foreach (var moduleChild in statusChild.GetSubChildren())
                foreach (var modelChild in moduleChild.GetSubChildren())
                    selections.Add(modelChild.gameObject);
                Selection.objects = selections.ToArray();
            }
            
            if(GUILayout.Button("Clear Default Cubes"))
                foreach (var cube in m_Baker.transform.GetComponentsInChildren<Transform>().Collect(p=>p.name=="Cube"))
                    Undo.DestroyObjectImmediate(cube.gameObject);
            
            GUILayout.EndVertical();
        }
        
        void ImportDecoration()
        {
            if (!UEAsset.SelectFilePath(out var filePath, "FBX"))
                return;

            var assetPath=UEPath.FileToAssetPath(filePath);
            var importer=AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (importer == null)
                return;

            m_Baker.ImportDecoration(AssetDatabase.LoadAllAssetsAtPath(assetPath).Collect(p => p is Mesh).Select(p => p as Mesh).ToArray());
        }

    }
}
#endif