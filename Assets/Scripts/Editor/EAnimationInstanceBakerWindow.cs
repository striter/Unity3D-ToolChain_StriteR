using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Rendering.Optimize;
namespace TEditor
{
    public class EAnimationInstanceBakerWindow : EditorWindow
    {
        GameObject m_TargetPrefab;
        SerializedObject m_SerializedWindow;
        [SerializeField]
        AnimationClip[] m_TargetAnimations;
        SerializedProperty m_AnimationProperty;
        string m_BoneExposeRegex="";

        void OnEnable()
        {
            m_TargetAnimations = null;
            m_SerializedWindow = new SerializedObject(this);
            m_AnimationProperty = m_SerializedWindow.FindProperty(nameof(m_TargetAnimations));
            EditorApplication.update += Tick;
        }
        void OnDisable()
        {
            m_TargetPrefab = null;
            m_TargetAnimations = null;
            m_SerializedWindow.Dispose();
            EditorApplication.update -= Tick;
        }
        private void Tick()
        {
            EditorUtility.SetDirty(this);
        }
        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            DrawGUI();
            EditorGUILayout.EndVertical();
        }
        void DrawGUI()
        {
            //Select Prefab
            bool havePrefab = m_TargetPrefab == null;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField("Select Source FBX");
            m_TargetPrefab = (GameObject)EditorGUILayout.ObjectField(m_TargetPrefab, typeof(GameObject), false);
            EditorGUILayout.EndHorizontal();
            if (m_TargetPrefab == null)
                return;
            //Check ModelImporters   
            ModelImporter importer = ModelImporter.GetAtPath(AssetDatabase.GetAssetPath(m_TargetPrefab)) as ModelImporter;
            if (importer == null)
            {
                EditorGUILayout.TextField("Target Asset Must Be A FBX Model");
                return;
            }

            //Select AnimationClipAsset  
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField("Animations For Bake");
            if (GUILayout.Button("Import Animation From Selection"))
            {
                List<AnimationClip> clip = new List<AnimationClip>();
                foreach (Object obj in Selection.objects)
                {
                    if ((obj as AnimationClip) != null && AssetDatabase.IsMainAsset(obj))
                        clip.Add(obj as AnimationClip);
                }
                m_TargetAnimations = clip.ToArray();
                m_SerializedWindow.Update();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(m_AnimationProperty, true);
            m_SerializedWindow.ApplyModifiedProperties();
            //Generate Animation 
            if (m_TargetAnimations == null || m_TargetAnimations.Length == 0 || m_TargetAnimations.Any(p => p == null))
                return;

            GUILayout.TextField("Generate Vertex Anim Instance Data(Highest Performance)");
            if (GUILayout.Button("Per Vertex Anim Instance Data Generate"))
                GenerateVertexTexture(m_TargetPrefab, m_TargetAnimations);


            GUILayout.TextField("Generate Bone Instance Anim Data(Lower Performance,Bone Expose Included)");
            EditorGUILayout.BeginHorizontal();
            GUILayout.TextField("Bone Expose Regex:");
            m_BoneExposeRegex = GUILayout.TextArea(m_BoneExposeRegex);
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Per Bone Anim Instance Data Generate"))
            {
                if (importer.optimizeGameObjects)
                {
                    EditorUtility.DisplayDialog("Error!", "Can't Bake Optimized FBX With Bone Instance", "Confirm");
                    return;
                }

                GenerateBoneInstanceMeshAndTexture(m_TargetPrefab, m_TargetAnimations, m_BoneExposeRegex);
            }
        }

        static int GetInstanceParams(AnimationClip[] _clips, out AnimationInstanceParam[] instanceParams)
        {
            int totalHeight = 0;
            instanceParams = new AnimationInstanceParam[_clips.Length];
            for (int i = 0; i < _clips.Length; i++)
            {
                AnimationClip clip = _clips[i];

                AnimationInstanceEvent[] instanceEvents = new AnimationInstanceEvent[clip.events.Length];
                for (int j = 0; j < clip.events.Length; j++)
                    instanceEvents[j] = new AnimationInstanceEvent(clip.events[j], clip.frameRate);
                int frameCount = (int)(clip.length * clip.frameRate);
                instanceParams[i] = new AnimationInstanceParam(clip.name, totalHeight, clip.frameRate, clip.length, clip.isLooping, instanceEvents.ToArray());
                totalHeight += frameCount;
            }
            return totalHeight;
        }
        void GenerateVertexTexture(GameObject _targetFBX, AnimationClip[] _clips)
        {
            if (!TEditor.SelectDirectory(_targetFBX, out string savePath, out string meshName))
            {
                Debug.LogWarning("Invalid Folder Selected");
                return;
            }
            GameObject instantiatedObj = GameObject.Instantiate(m_TargetPrefab);
            SkinnedMeshRenderer skinnedMeshRenderer = instantiatedObj.GetComponentInChildren<SkinnedMeshRenderer>();
            MeshBoundsChecker boundsCheck = new MeshBoundsChecker();
            #region Bake Animation Atlas
            int vertexCount = skinnedMeshRenderer.sharedMesh.vertexCount;
            int totalVertexRecord = vertexCount * 2;
            int totalFrame = GetInstanceParams(_clips, out AnimationInstanceParam[] instanceParams);

            Texture2D atlasTexture = new Texture2D(Mathf.NextPowerOfTwo(totalVertexRecord), Mathf.NextPowerOfTwo(totalFrame), TextureFormat.RGBAHalf, false);
            atlasTexture.filterMode = FilterMode.Point;
            atlasTexture.wrapModeU = TextureWrapMode.Clamp;
            atlasTexture.wrapModeV = TextureWrapMode.Repeat;

            for (int i = 0; i < _clips.Length; i++)
            {
                AnimationClip clip = _clips[i];
                Mesh vertexBakeMesh = new Mesh();
                float length = clip.length;
                float frameRate = clip.frameRate;
                int frameCount = (int)(length * frameRate);
                int startFrame = instanceParams[i].m_FrameBegin;
                for (int j = 0; j < frameCount; j++)
                {
                    clip.SampleAnimation(instantiatedObj, length * j / frameCount);
                    skinnedMeshRenderer.BakeMesh(vertexBakeMesh);
                    Vector3[] vertices = vertexBakeMesh.vertices;
                    Vector3[] normals = vertexBakeMesh.normals;
                    for (int k = 0; k < vertexCount; k++)
                    {
                        boundsCheck.CheckBounds(vertices[k]);
                        atlasTexture.SetPixel(k * 2, startFrame + j, TColor.VectorToColor(vertices[k]));
                        atlasTexture.SetPixel(k * 2 + 1, startFrame + j, TColor.VectorToColor(normals[k]));
                    }
                }
                vertexBakeMesh.Clear();
            }
            atlasTexture.Apply();
            #endregion

            #region Bake Mesh
            Mesh instanceMesh = skinnedMeshRenderer.sharedMesh.Copy();
            instanceMesh.normals = null;
            instanceMesh.tangents = null;
            instanceMesh.boneWeights = null;
            instanceMesh.bindposes = null;
            instanceMesh.bounds = boundsCheck.GetBounds();
            #endregion
            DestroyImmediate(instantiatedObj);

            AnimationInstanceData instanceData = ScriptableObject.CreateInstance<AnimationInstanceData>();
            instanceData.m_Animations = instanceParams;
            instanceData=TEditor.CreateAssetCombination( new KeyValuePair<AnimationInstanceData, string>(instanceData,savePath + meshName + "_VertexInstance.asset"), new KeyValuePair<Object, string>( atlasTexture,meshName+"_AnimationAtlas"),new KeyValuePair<Object,string>(instanceMesh,meshName+"_InstanceMesh"));
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(instanceData));
            foreach (var asset in assets)
            {
                Texture2D atlas = asset as Texture2D;
                Mesh mesh = asset as Mesh;
                if (atlas)
                    instanceData.m_AnimationAtlas = atlas;
                if (mesh)
                    instanceData.m_InstancedMesh = mesh;
            }
            AssetDatabase.SaveAssets();
        }

        void GenerateBoneInstanceMeshAndTexture(GameObject _targetFBX, AnimationClip[] _clips, string exposeBones)
        {
            if (!TEditor.SelectDirectory(_targetFBX, out string savePath, out string meshName))
            {
                Debug.LogWarning("Invalid Folder Selected");
                return;
            }
            GameObject _instantiatedObj = GameObject.Instantiate(m_TargetPrefab);
            SkinnedMeshRenderer _skinnedMeshRenderer = _instantiatedObj.GetComponentInChildren<SkinnedMeshRenderer>();
            try
            {
                MeshBoundsChecker boundsCheck = new MeshBoundsChecker();
                Matrix4x4[] bindPoses = _skinnedMeshRenderer.sharedMesh.bindposes;
                Transform[] bones = _skinnedMeshRenderer.bones;
                #region Record Expose Bone
                List<AnimationInstanceExposeBone> exposeBoneParam = new List<AnimationInstanceExposeBone>();
                if (exposeBones != "")
                {
                    Transform[] activeTransforms = _instantiatedObj.GetComponentsInChildren<Transform>();
                    for (int i = 0; i < activeTransforms.Length; i++)
                    {
                        if (!System.Text.RegularExpressions.Regex.Match(activeTransforms[i].name, exposeBones).Success)
                            continue;
                        int relativeBoneIndex = -1;
                        Transform relativeBone = activeTransforms[i];
                        while (relativeBone != null)
                        {
                            relativeBoneIndex = System.Array.FindIndex( bones,p => p == relativeBone);
                            if (relativeBoneIndex != -1)
                                break;
                            relativeBone = relativeBone.parent;
                        }
                        if (relativeBoneIndex == -1)
                            continue;

                        Matrix4x4 rootWorldToLocal = _skinnedMeshRenderer.transform.worldToLocalMatrix;

                        exposeBoneParam.Add(new AnimationInstanceExposeBone()
                        {
                            m_BoneIndex = relativeBoneIndex,
                            m_BoneName = activeTransforms[i].name,
                            m_Position = rootWorldToLocal.MultiplyPoint(activeTransforms[i].transform.position),
                            m_Direction = rootWorldToLocal.MultiplyVector(activeTransforms[i].transform.forward)
                        });
                    }
                }
                #endregion
                #region Bake Animation Atlas
                int boneCount = _skinnedMeshRenderer.sharedMesh.bindposes.Length;
                int totalWdith = boneCount * 3;
                int totalFrame = GetInstanceParams(_clips, out AnimationInstanceParam[] instanceParams);
                List<AnimationInstanceEvent> instanceEvents = new List<AnimationInstanceEvent>();

                Texture2D atlasTexture = new Texture2D(Mathf.NextPowerOfTwo(totalWdith), Mathf.NextPowerOfTwo(totalFrame), TextureFormat.RGBAHalf, false);
                atlasTexture.filterMode = FilterMode.Point;
                atlasTexture.wrapModeU = TextureWrapMode.Clamp;
                atlasTexture.wrapModeV = TextureWrapMode.Repeat;

                for (int i = 0; i < _clips.Length; i++)
                {
                    AnimationClip clip = _clips[i];
                    float length = clip.length;
                    float frameRate = clip.frameRate;
                    int frameCount = (int)(length * frameRate);
                    int startFrame = instanceParams[i].m_FrameBegin;
                    for (int j = 0; j < frameCount; j++)
                    {
                        clip.SampleAnimation(_instantiatedObj, length * j / frameCount);
                        for (int k = 0; k < boneCount; k++)
                        {
                            Matrix4x4 curFrameBoneMatrix = _skinnedMeshRenderer.transform.worldToLocalMatrix * bones[k].localToWorldMatrix * bindPoses[k];
                            atlasTexture.SetPixel(k * 3, startFrame + j, TColor.VectorToColor(curFrameBoneMatrix.GetRow(0)));
                            atlasTexture.SetPixel(k * 3 + 1, startFrame + j, TColor.VectorToColor(curFrameBoneMatrix.GetRow(1)));
                            atlasTexture.SetPixel(k * 3 + 2, startFrame + j, TColor.VectorToColor( curFrameBoneMatrix.GetRow(2)));
                        }

                        Mesh boundsCheckMesh = new Mesh();
                        _skinnedMeshRenderer.BakeMesh(boundsCheckMesh);
                        Vector3[] verticies = boundsCheckMesh.vertices;
                        for (int k = 0; k < verticies.Length; k++)
                            boundsCheck.CheckBounds(verticies[k].Divide(_skinnedMeshRenderer.transform.localScale));

                        boundsCheckMesh.Clear();
                    }
                }
                atlasTexture.Apply();
                #endregion
                #region Bake Mesh
                Mesh instanceMesh = _skinnedMeshRenderer.sharedMesh.Copy();
                BoneWeight[] boneWeights = instanceMesh.boneWeights;
                Vector4[] uv2 = new Vector4[boneWeights.Length];
                Vector4[] uv3 = new Vector4[boneWeights.Length];
                for (int i = 0; i < boneWeights.Length; i++)
                {
                    uv2[i] = new Vector4(boneWeights[i].boneIndex0, boneWeights[i].boneIndex1, boneWeights[i].boneIndex2, boneWeights[i].boneIndex3);
                    uv3[i] = new Vector4(boneWeights[i].weight0, boneWeights[i].weight1, boneWeights[i].weight2, boneWeights[i].weight3);
                }
                instanceMesh.SetUVs(1, uv2);
                instanceMesh.SetUVs(2, uv3);
                instanceMesh.boneWeights = null;
                instanceMesh.bindposes = null;
                instanceMesh.bounds = boundsCheck.GetBounds();
                #endregion
                DestroyImmediate(_instantiatedObj);

                AnimationInstanceData instanceData = ScriptableObject.CreateInstance<AnimationInstanceData>();
                instanceData.m_Animations = instanceParams;
                instanceData.m_ExposeBones = exposeBoneParam.ToArray();

                instanceData =  TEditor.CreateAssetCombination(new KeyValuePair<Object, string>(instanceData, savePath + meshName + "_BoneInstance.asset"), new KeyValuePair<Object, string>(atlasTexture, meshName + "_AnimationAtlas"), new KeyValuePair<Object, string>(instanceMesh, meshName + "_InstanceMesh")) as AnimationInstanceData;
                Object[] assets=AssetDatabase.LoadAllAssetsAtPath( AssetDatabase.GetAssetPath(instanceData));
                foreach(var asset in assets)
                {
                    Texture2D atlas = asset as Texture2D;
                    Mesh mesh = asset as Mesh;
                    if (atlas)
                        instanceData.m_AnimationAtlas = atlas;
                    if (mesh)
                        instanceData.m_InstancedMesh = mesh;
                }
                AssetDatabase.SaveAssets();
            }
            catch (System.Exception e)
            {
                Debug.LogError("Generate Failed:" + e.Message);
                DestroyImmediate(_instantiatedObj);
            }
        }
    }
}