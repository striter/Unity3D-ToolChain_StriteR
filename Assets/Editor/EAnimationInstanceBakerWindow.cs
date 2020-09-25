using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TEditor
{
    public class EAnimationInstanceBakerWindow : EditorWindow
    {
        GameObject m_TargetPrefab;
        SerializedObject m_SerializedWindow;
        [SerializeField]
        AnimationClip[] m_TargetAnimations;
        SerializedProperty m_AnimationProperty;
        private void OnEnable()
        {
            m_TargetAnimations = null;
            m_SerializedWindow = new SerializedObject(this);
            m_AnimationProperty = m_SerializedWindow.FindProperty("m_TargetAnimations");
        }
        private void OnDisable()
        {
            m_TargetPrefab = null;
            m_TargetAnimations = null;
            m_SerializedWindow.Dispose();
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

            if (EditorGUILayout.PropertyField(m_AnimationProperty, true))
                m_SerializedWindow.ApplyModifiedProperties();
            //Generate Animation 
            if (m_TargetAnimations == null || m_TargetAnimations.Length == 0 || m_TargetAnimations.Any(p => p == null))
                return;
            if (GUILayout.Button("Generate Anim Vertex Texture"))
                GenerateVertexTexture(m_TargetPrefab, m_TargetAnimations);
            if (GUILayout.Button("Generate Anim Bone Texture & Mesh"))
                GenerateBoneInstanceMeshAndTexture(m_TargetPrefab, m_TargetAnimations);
        }

        static int GetInstanceParams(AnimationClip[] _clips, out AnimationInstanceParam[] instanceParams)
        {
            int totalHeight = 0;
            instanceParams = new AnimationInstanceParam[_clips.Length];
            for (int i = 0; i < _clips.Length; i++)
            {
                AnimationClip clip = _clips[i];
                int frameCount = (int)(clip.length * clip.frameRate);
                instanceParams[i] = new AnimationInstanceParam(clip.name, totalHeight, clip.frameRate, clip.length, clip.isLooping);
                totalHeight += frameCount;
            }
            return totalHeight;
        }

        void GenerateVertexTexture(GameObject _targetFBX, AnimationClip[] _clips)
        {
            string savePath = ETPath.GetAssetPath(EditorUtility.OpenFolderPanel("Select Vertex Instance Data Save Folder", "", ""));
            string meshName = ETPath.GetPathName(AssetDatabase.GetAssetPath(_targetFBX));
            GameObject instantiatedObj = GameObject.Instantiate(m_TargetPrefab);
            SkinnedMeshRenderer skinnedMeshRenderer = instantiatedObj.GetComponentInChildren<SkinnedMeshRenderer>();
            AnimationInstanceBoundsCheck boundsCheck = new AnimationInstanceBoundsCheck();
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
                int frameCount = (int)(length * clip.frameRate);
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
                        atlasTexture.SetPixel(k * 2, startFrame + j, vertices[k].ToColor());
                        atlasTexture.SetPixel(k * 2 + 1, startFrame + j, normals[k].ToColor());
                    }
                }
                vertexBakeMesh.Clear();
            }
            atlasTexture.Apply();
            #endregion

            #region Bake Mesh
            Mesh bakeBoneMesh = skinnedMeshRenderer.sharedMesh.Copy();
            bakeBoneMesh.normals = null;
            bakeBoneMesh.tangents = null;
            bakeBoneMesh.boneWeights = null;
            bakeBoneMesh.bindposes = null;
            bakeBoneMesh.bounds = boundsCheck.GetBounds();
            Mesh bakeMesh = CreateAsset(bakeBoneMesh, savePath + meshName + "_VertexInstance_BakeMesh.asset");
            #endregion
            DestroyImmediate(instantiatedObj);

            AnimationInstanceData animationInstanceData = ScriptableObject.CreateInstance<AnimationInstanceData>();
            animationInstanceData.m_InstanceMesh = bakeMesh;
            animationInstanceData.m_AnimationParams = instanceParams;
            animationInstanceData.m_AnimationAtlas = CreateAsset(atlasTexture, savePath + meshName + "_VertexInstance_AnimationAtlas.asset");
            CreateAsset(animationInstanceData, savePath + meshName + "_VertexInstance_Data.asset");
        }

        void GenerateBoneInstanceMeshAndTexture(GameObject _targetFBX, AnimationClip[] _clips)
        {
            string savePath = ETPath.GetAssetPath(EditorUtility.OpenFolderPanel("Select Bone Instance Data Save Folder", "", ""));

            GameObject _instantiatedObj = GameObject.Instantiate(m_TargetPrefab);
            SkinnedMeshRenderer _skinnedMeshRenderer = _instantiatedObj.GetComponentInChildren<SkinnedMeshRenderer>();
            string meshName = ETPath.GetPathName(AssetDatabase.GetAssetPath(_targetFBX));
            AnimationInstanceBoundsCheck boundsCheck = new AnimationInstanceBoundsCheck();
            #region Bake Animation Atlas
            int boneCount = _skinnedMeshRenderer.sharedMesh.bindposes.Length;
            int totalWdith = boneCount * 3;
            int totalFrame = GetInstanceParams(_clips, out AnimationInstanceParam[] instanceParams);

            Texture2D atlasTexture = new Texture2D(Mathf.NextPowerOfTwo(totalWdith), Mathf.NextPowerOfTwo(totalFrame), TextureFormat.RGBAHalf, false);
            atlasTexture.filterMode = FilterMode.Point;
            atlasTexture.wrapModeU = TextureWrapMode.Clamp;
            atlasTexture.wrapModeV = TextureWrapMode.Repeat;

            Matrix4x4[] bindPoses = _skinnedMeshRenderer.sharedMesh.bindposes;
            Transform[] bones = _skinnedMeshRenderer.bones;
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
                        atlasTexture.SetPixel(k * 3, startFrame + j, curFrameBoneMatrix.GetRow(0).ToColor());
                        atlasTexture.SetPixel(k * 3 + 1, startFrame + j, curFrameBoneMatrix.GetRow(1).ToColor());
                        atlasTexture.SetPixel(k * 3 + 2, startFrame + j, curFrameBoneMatrix.GetRow(2).ToColor());
                    }

                    Mesh boundsCheckMesh = new Mesh();
                    _skinnedMeshRenderer.BakeMesh(boundsCheckMesh);
                    Vector3[] verticies = boundsCheckMesh.vertices;
                    for (int k = 0; k < verticies.Length; k++)
                        boundsCheck.CheckBounds(verticies[k]);

                    boundsCheckMesh.Clear();
                }
            }
            atlasTexture.Apply();
            #endregion
            #region Bake Mesh
            Mesh bakeBoneMesh = _skinnedMeshRenderer.sharedMesh.Copy();
            BoneWeight[] boneWeights = bakeBoneMesh.boneWeights;
            Vector4[] uv2 = new Vector4[boneWeights.Length];
            Vector4[] uv3 = new Vector4[boneWeights.Length];
            for (int i = 0; i < boneWeights.Length; i++)
            {
                uv2[i] = new Vector4(boneWeights[i].boneIndex0, boneWeights[i].boneIndex1, boneWeights[i].boneIndex2, boneWeights[i].boneIndex3);
                uv3[i] = new Vector4(boneWeights[i].weight0, boneWeights[i].weight1, boneWeights[i].weight2, boneWeights[i].weight3);
            }
            bakeBoneMesh.SetUVs(1, uv2);
            bakeBoneMesh.SetUVs(2, uv3);
            bakeBoneMesh.boneWeights = null;
            bakeBoneMesh.bindposes = null;
            bakeBoneMesh.bounds = boundsCheck.GetBounds();
            Mesh bakeMesh = CreateAsset(bakeBoneMesh, savePath + meshName + "_BoneInstance_BakeMesh.asset");
            #endregion
            DestroyImmediate(_instantiatedObj);

            AnimationInstanceData animationInstanceData = ScriptableObject.CreateInstance<AnimationInstanceData>();
            animationInstanceData.m_InstanceMesh = bakeMesh;
            animationInstanceData.m_AnimationParams = instanceParams;
            animationInstanceData.m_AnimationAtlas = CreateAsset(atlasTexture, savePath + meshName + "_BoneInstance_AnimationAtlas.asset");
            CreateAsset(animationInstanceData, savePath + meshName + "_BoneInstance_Data.asset");
        }

        public static T CreateAsset<T>(T asset, string path) where T : Object
        {
            if (AssetDatabase.LoadAssetAtPath<Object>(path) != null)
                AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(asset, path);
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }
        public class AnimationInstanceBoundsCheck
        {
            Vector3 m_BoundsMin;
            Vector3 m_BoundsMax;
            public AnimationInstanceBoundsCheck()
            {
                m_BoundsMin = Vector3.zero;
                m_BoundsMax = Vector3.zero;
            }
            public void CheckBounds(Vector3 vertice)
            {
                m_BoundsMin = Vector3.Min(m_BoundsMin, vertice);
                m_BoundsMax = Vector3.Max(m_BoundsMax, vertice);
            }
            public Bounds GetBounds() => new Bounds((m_BoundsMin + m_BoundsMax) / 2, m_BoundsMax - m_BoundsMin);
        }
    }


}