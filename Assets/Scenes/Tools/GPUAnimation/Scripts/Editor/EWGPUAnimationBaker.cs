using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Runtime.Optimize.GPUAnimation;

namespace UnityEditor.Extensions
{
    
    public class GPUAnimationBaker : EditorWindow
    {
        [MenuItem("Assets/Create/Optimize/GPU Animation/Data", false, 10)]
        static void ShowOptimizeWindow()
        {
            var window = GetWindow<GPUAnimationBaker>();
            window.titleContent = new GUIContent("GPU Animation Instance Baker", EditorGUIUtility.IconContent("AvatarSelector").image);
            window.UpdateWithSelections();

            if (window.m_TargetPrefab == null)
            {
                Debug.LogError("No Prefab Selected");
                window.Close();
            }
            
            if (window.m_TargetAnimations == null || window.m_TargetAnimations.Length == 0)
            {
                Debug.LogError("No Animation Selected");
                window.Close();
            }
        }
        
        GameObject m_TargetPrefab;
        SerializedObject m_SerializedWindow;
        [SerializeField] AnimationClip[] m_TargetAnimations;
        SerializedProperty m_AnimationProperty;
        string m_BoneExposeRegex="";
        EGPUAnimationMode m_Mode= EGPUAnimationMode._ANIM_BONE;

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
            m_AnimationProperty.Dispose();
            EditorApplication.update -= Tick;
        }
        private void Tick()
        {
            EditorUtility.SetDirty(this);
        }
        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            UpdateWithSelections();
            DrawGUI();
            EditorGUILayout.EndVertical();
        }

        void UpdateWithSelections()
        {
            //Select AnimationClipAsset  
            var clip = new List<AnimationClip>();
            foreach (var obj in Selection.objects)
            {
                if ((obj as AnimationClip) != null && AssetDatabase.IsMainAsset(obj))
                    clip.Add(obj as AnimationClip);
                else if (AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(obj)) as ModelImporter != null)
                    m_TargetPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GetAssetPath(obj));
            }

            if (clip.Count <= 0) return;
            m_TargetAnimations = clip.ToArray();
            m_SerializedWindow.Update();
        }
        
        void DrawGUI()
        {
            //Select Prefab
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Select FBX&Animation Data");
            m_TargetPrefab = (GameObject)EditorGUILayout.ObjectField(m_TargetPrefab, typeof(GameObject), false);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(m_AnimationProperty, true);
            m_SerializedWindow.ApplyModifiedProperties();

            ModelImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(m_TargetPrefab)) as ModelImporter;
            if (m_TargetPrefab == null || importer == null || m_TargetAnimations==null || m_TargetAnimations.Length==0)
            {
                EditorGUILayout.LabelField("<Color=#FF0000>Select FBX Model & Animations</Color>", UEGUIStyle_Window.m_ErrorLabel);
                return;
            }
            
            //Generate Animation 
            if (m_TargetAnimations == null || m_TargetAnimations.Length == 0 || m_TargetAnimations.Any(p => p == null))
                return;

            EditorGUILayout.BeginHorizontal();
            m_Mode = (EGPUAnimationMode)EditorGUILayout.EnumPopup("Type:",m_Mode);
            EditorGUILayout.EndHorizontal();

            if(m_Mode==EGPUAnimationMode._ANIM_VERTEX)
            {
                if (GUILayout.Button("Generate"))
                    GenerateVertexTexture(m_TargetPrefab, m_TargetAnimations);
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Expose Transform Regex:");
                m_BoneExposeRegex = GUILayout.TextArea(m_BoneExposeRegex);
                EditorGUILayout.EndHorizontal();
                if (GUILayout.Button("Generate"))
                {
                    if (importer.optimizeGameObjects)
                    {
                        EditorUtility.DisplayDialog("Error!", "Can't Bake Optimized FBX With Bone Instance", "Confirm");
                        return;
                    }
                    GenerateBoneInstanceMeshAndTexture(m_TargetPrefab, m_TargetAnimations, m_BoneExposeRegex);
                }
            }
        }

        static int GetInstanceParams(AnimationClip[] _clips, out AnimationTickerClip[] instanceParams)
        {
            var totalHeight = 0;
            instanceParams = new AnimationTickerClip[_clips.Length];
            for (var i = 0; i < _clips.Length; i++)
            {
                AnimationClip clip = _clips[i];

                AnimationTickerEvent[] instanceEvents = new AnimationTickerEvent[clip.events.Length];
                for (var j = 0; j < clip.events.Length; j++)
                    instanceEvents[j] = new AnimationTickerEvent(clip.events[j], clip.frameRate);
                var frameCount = (int)(clip.length * clip.frameRate);
                instanceParams[i] = new AnimationTickerClip(clip.name, totalHeight, clip.frameRate, clip.length, clip.isLooping, instanceEvents.ToArray());
                totalHeight += frameCount;
            }
            return totalHeight;
        }
        void GenerateVertexTexture(GameObject _targetFBX, AnimationClip[] _clips)
        {
            if (!UEAsset.SelectDirectory(_targetFBX, out string savePath, out string meshName))
            {
                Debug.LogWarning("Invalid Folder Selected");
                return;
            }
            var instantiatedObj = GameObject.Instantiate(m_TargetPrefab);
            var skinnedMeshRenderer = instantiatedObj.GetComponentInChildren<SkinnedMeshRenderer>();
            #region Bake Animation Atlas
            var vertexCount = skinnedMeshRenderer.sharedMesh.vertexCount;
            var totalVertexRecord = vertexCount * 2;
            var totalFrame = GetInstanceParams(_clips, out AnimationTickerClip[] instanceParams);

            var atlasTexture = new Texture2D(Mathf.NextPowerOfTwo(totalVertexRecord), Mathf.NextPowerOfTwo(totalFrame), TextureFormat.RGBAHalf, false)
                {
                    filterMode = FilterMode.Point,
                    wrapModeU = TextureWrapMode.Clamp,
                    wrapModeV = TextureWrapMode.Repeat
                };
            UBoundsIncrement.Begin();
            for (var i = 0; i < _clips.Length; i++)
            {
                var clip = _clips[i];
                var vertexBakeMesh = new Mesh();
                var length = clip.length;
                var frameRate = clip.frameRate;
                var frameCount = (int)(length * frameRate);
                var startFrame = instanceParams[i].frameBegin;
                for (var j = 0; j < frameCount; j++)
                {
                    clip.SampleAnimation(instantiatedObj, length * j / frameCount);
                    skinnedMeshRenderer.BakeMesh(vertexBakeMesh);
                    var vertices = vertexBakeMesh.vertices;
                    var normals = vertexBakeMesh.normals;
                    for (var k = 0; k < vertexCount; k++)
                    {
                        UBoundsIncrement.Iterate(vertices[k]);
                        var frame = startFrame + j;
                        var pixel = UGPUAnimation.GetVertexPositionPixel(k,frame);
                        atlasTexture.SetPixel(pixel.x,pixel.y, UColor.toColor(vertices[k]));
                        pixel = UGPUAnimation.GetVertexNormalPixel(k,frame);
                        atlasTexture.SetPixel(pixel.x,pixel.y, UColor.toColor(normals[k]));
                    }
                }
                vertexBakeMesh.Clear();
            }
            atlasTexture.Apply();
            #endregion

            #region Bake Mesh
            var instanceMesh = skinnedMeshRenderer.sharedMesh.Copy();
            instanceMesh.normals = null;
            instanceMesh.tangents = null;
            instanceMesh.boneWeights = null;
            instanceMesh.bindposes = null;
            instanceMesh.bounds = UBoundsIncrement.End();
            #endregion
            DestroyImmediate(instantiatedObj);

            var data = CreateInstance<GPUAnimationData>();
            data.m_Mode = EGPUAnimationMode._ANIM_VERTEX;
            data.m_AnimationClips = instanceParams;

            atlasTexture.name = meshName + "_AnimationAtlas";
            instanceMesh.name = meshName + "_InstanceMesh";
            data=UEAsset.CreateAssetCombination(savePath + meshName + "_GPU_Vertex.asset", data, new Object[]{atlasTexture,instanceMesh});
            var assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(data));
            foreach (var asset in assets)
            {
                var atlas = asset as Texture2D;
                var mesh = asset as Mesh;
                if (atlas)
                    data.m_BakeTexture = atlas;
                if (mesh)
                    data.m_BakedMesh = mesh;
            }
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
        }

        void GenerateBoneInstanceMeshAndTexture(GameObject _targetFBX, AnimationClip[] _clips, string exposeBones)
        {
            if (!UEAsset.SelectDirectory(_targetFBX, out string savePath, out string meshName))
            {
                Debug.LogWarning("Invalid Folder Selected");
                return;
            }
            var instantiatedObj = GameObject.Instantiate(m_TargetPrefab);
            var skinnedMeshRenderer = instantiatedObj.GetComponentInChildren<SkinnedMeshRenderer>();
            try
            {
                var bindPoses = skinnedMeshRenderer.sharedMesh.bindposes;
                var bones = skinnedMeshRenderer.bones;
                #region Record Expose Bone
                var exposeTransformParam = new List<GPUAnimationExposeBone>();
                if (exposeBones != "")
                {
                    var activeTransforms = instantiatedObj.GetComponentsInChildren<Transform>();
                    foreach (var activeTransform in activeTransforms)
                    {
                        if (!System.Text.RegularExpressions.Regex.Match(activeTransform.name, exposeBones).Success)
                            continue;
                        var relativeBoneIndex = -1;
                        var relativeBone = activeTransform;
                        while (relativeBone != null)
                        {
                            relativeBoneIndex = System.Array.FindIndex( bones,p => p == relativeBone);
                            if (relativeBoneIndex != -1)
                                break;
                            relativeBone = relativeBone.parent;
                        }
                        if (relativeBoneIndex == -1)
                            continue;

                        var rootWorldToLocal = skinnedMeshRenderer.transform.worldToLocalMatrix;

                        exposeTransformParam.Add(new GPUAnimationExposeBone()
                        {
                            index = relativeBoneIndex,
                            name = activeTransform.name,
                            position = rootWorldToLocal.MultiplyPoint(activeTransform.transform.position),
                            direction = rootWorldToLocal.MultiplyVector(activeTransform.transform.forward)
                        });
                    }
                }
                #endregion
                #region Bake Animation Atlas
                var transformCount = skinnedMeshRenderer.sharedMesh.bindposes.Length;
                var totalWidth = transformCount * 3;
                var totalFrame = GetInstanceParams(_clips, out AnimationTickerClip[] instanceParams);

                var atlasTexture = new Texture2D(Mathf.NextPowerOfTwo(totalWidth), Mathf.NextPowerOfTwo(totalFrame), TextureFormat.RGBAHalf, false)
                    {
                        filterMode = FilterMode.Point,
                        wrapModeU = TextureWrapMode.Clamp,
                        wrapModeV = TextureWrapMode.Repeat
                    };
                UBoundsIncrement.Begin();
                for (var i = 0; i < _clips.Length; i++)
                {
                    var clip = _clips[i];
                    var length = clip.length;
                    var frameRate = clip.frameRate;
                    var frameCount = (int)(length * frameRate);
                    var startFrame = instanceParams[i].frameBegin;
                    for (var j = 0; j < frameCount; j++)
                    {
                        clip.SampleAnimation(instantiatedObj, length * j / frameCount);
                        for (var k = 0; k < transformCount; k++)
                        {
                            var bindPoseToLocal = bindPoses[k];
                            var localToBoneAnimated = bones[k].localToWorldMatrix;
                            var bindPoseToBoneAnimated = localToBoneAnimated * bindPoseToLocal;
                            var frame = startFrame + j;
                            var pixel = UGPUAnimation.GetTransformPixel(k,0,frame);
                            atlasTexture.SetPixel(pixel.x,pixel.y, bindPoseToBoneAnimated.GetRow(0).toColor());
                            pixel = UGPUAnimation.GetTransformPixel(k,1,frame);
                            atlasTexture.SetPixel(pixel.x, pixel.y, bindPoseToBoneAnimated.GetRow(1).toColor());
                            pixel = UGPUAnimation.GetTransformPixel(k,2,frame);
                            atlasTexture.SetPixel(pixel.x, pixel.y, bindPoseToBoneAnimated.GetRow(2).toColor());
                        }

                        var boundsCheckMesh = new Mesh();
                        skinnedMeshRenderer.BakeMesh(boundsCheckMesh);
                        var vertices = boundsCheckMesh.vertices;
                        for (var k = 0; k < vertices.Length; k++)
                            UBoundsIncrement.Iterate(vertices[k].div(skinnedMeshRenderer.transform.localScale));

                        boundsCheckMesh.Clear();
                    }
                }
                atlasTexture.Apply();
                #endregion
                #region Bake Mesh
                var instanceMesh = skinnedMeshRenderer.sharedMesh.Copy();
                var transformWeights = instanceMesh.boneWeights;
                var uv1 = new Vector4[transformWeights.Length];
                var uv2 = new Vector4[transformWeights.Length];
                for (var i = 0; i < transformWeights.Length; i++)
                {
                    uv1[i] = new Vector4(transformWeights[i].boneIndex0, transformWeights[i].boneIndex1, transformWeights[i].boneIndex2, transformWeights[i].boneIndex3);
                    uv2[i] = new Vector4(transformWeights[i].weight0, transformWeights[i].weight1, transformWeights[i].weight2, transformWeights[i].weight3);
                }
                instanceMesh.SetUVs(1, uv1);
                instanceMesh.SetUVs(2, uv2);
                instanceMesh.boneWeights = null;
                instanceMesh.bindposes = null;
                instanceMesh.bounds = UBoundsIncrement.End();
                #endregion
                DestroyImmediate(instantiatedObj);

                var data = ScriptableObject.CreateInstance<GPUAnimationData>();
                data.m_Mode = EGPUAnimationMode._ANIM_BONE;
                data.m_AnimationClips = instanceParams;
                data.m_ExposeTransforms = exposeTransformParam.ToArray();

                atlasTexture.name = meshName + "_AnimationAtlas";
                instanceMesh.name = meshName + "_InstanceMesh";
                data = UEAsset.CreateAssetCombination(savePath + meshName + "_GPU_Transform.asset",data, new Object[]{atlasTexture,instanceMesh});
                var assets=AssetDatabase.LoadAllAssetsAtPath( AssetDatabase.GetAssetPath(data));
                foreach(var asset in assets)
                {
                    var atlas = asset as Texture2D;
                    var mesh = asset as Mesh;
                    if (atlas)
                        data.m_BakeTexture = atlas;
                    if (mesh)
                        data.m_BakedMesh = mesh;
                }
                EditorUtility.SetDirty(data);
                AssetDatabase.SaveAssets();
            }
            catch (System.Exception e)
            {
                Debug.LogError("Generate Failed:" + e.Message);
                DestroyImmediate(instantiatedObj);
            }
        }
    }
}