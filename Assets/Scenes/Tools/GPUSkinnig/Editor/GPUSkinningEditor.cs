using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry;
using Runtime.Optimize.GPUAnimation;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Extensions;
using UnityEngine;

namespace Examples.Tools.Optimize.GPUSkinning
{

    public class GPUSkinningEditor : MonoBehaviour
    {
        private static readonly string kPrefix = "[GPUSkinning]";
        [MenuItem("Assets/Create/Optimize/GPU Skinning/Output Mesh", false, 11)]
        static void ShowOptimizeWindow()
        {
            var obj = Selection.activeObject;
            GameObject targetPrefab = null;
            if (AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(obj)) as ModelImporter != null)
                targetPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GetAssetPath(obj));
            if (!targetPrefab)
            {
                Debug.LogError($"{kPrefix}No FBX Selected");
                return;
            }
         
            var instantiatedObj = GameObject.Instantiate(targetPrefab);
            var skinnedMeshRenderer = instantiatedObj.GetComponentInChildren<SkinnedMeshRenderer>();
            if (!skinnedMeshRenderer.sharedMesh)
            {
                Debug.LogError($"{kPrefix}No Mesh Found for {skinnedMeshRenderer.name}");
                return;
            }
            if (!UEAsset.SaveAssetPath(out var savePath,"asset",$"{skinnedMeshRenderer.name}_GPU_Skinning"))
            {
                Debug.LogWarning($"{kPrefix}Invalid Folder Selected");
                return;
            }

            var data = ScriptableObject.CreateInstance<GPUSkinningData>();
            data.m_Mesh = GenerateBoneInstanceMesh(skinnedMeshRenderer, out var boundSpheresLS);
            var root = instantiatedObj.transform;
            
            var count = skinnedMeshRenderer.bones.Length;
            data.m_Bones = new List<GPUSkinningBoneData>();
            for (var i = 0; i < count; i++)
            {
                var bone = skinnedMeshRenderer.bones[i];
                var boneTransform = root.Find(bone.GetRelativePath(root));
                if (boneTransform == null)
                {
                    Debug.LogError($"{kPrefix}Bone Not Found:" + bone.GetRelativePath(root));
                    return;
                }

                data.m_Bones.Add(new GPUSkinningBoneData
                {
                    relativePath = bone.GetRelativePath(root),
                    bindPose = skinnedMeshRenderer.sharedMesh.bindposes[i],
                    bounds = boundSpheresLS[i]
                });
            }
            data.name = skinnedMeshRenderer.name + "_GPU_Skinning";
            DestroyImmediate(instantiatedObj);
            
            UEAsset.CreateAssetCombination( savePath,data);
        }

        static Mesh GenerateBoneInstanceMesh(SkinnedMeshRenderer skinnedMeshRenderer,out List<GSphere> boundSpheresOS)
        {
            boundSpheresOS = new();
            var instanceMesh = skinnedMeshRenderer.sharedMesh.Copy();
            var transformWeights = instanceMesh.boneWeights;
            var indexes = new Vector4[transformWeights.Length];
            var weights = new Vector4[transformWeights.Length];
            boundSpheresOS.Resize(skinnedMeshRenderer.bones.Length,()=>new GSphere(0,-1));
            var bakeMesh = new Mesh();
            skinnedMeshRenderer.BakeMesh(bakeMesh);
            var bakedVertices = bakeMesh.vertices;
            for (var i = 0; i < transformWeights.Length; i++)
            {
                indexes[i] = new Vector4(transformWeights[i].boneIndex0, transformWeights[i].boneIndex1, transformWeights[i].boneIndex2, transformWeights[i].boneIndex3);
                weights[i] = new Vector4(transformWeights[i].weight0, transformWeights[i].weight1, transformWeights[i].weight2, transformWeights[i].weight3);
                
                var vertexOS =  bakedVertices[i];
                for(var j = 0; j < 4; j++)
                {
                   var boneIndex = (int)indexes[i][j];
                   var weight = weights[i][j];
                   if (weight <= 0) 
                       continue;
                   var transformMatrix = skinnedMeshRenderer.bones[boneIndex].localToWorldMatrix * skinnedMeshRenderer.sharedMesh.bindposes[boneIndex];
                   var bonePose = math.mul(transformMatrix, new float4(vertexOS, 1f)).xyz;
                   
                   var bounds = boundSpheresOS[boneIndex];
                   if (bounds.radius < 0)
                       bounds = new GSphere(bonePose,0);
                   
                   boundSpheresOS[boneIndex] = bounds.Encapsulate(bonePose);
                    // Debug.DrawLine(bonePose, skinnedMeshRenderer.bones[boneIndex].position,Color.blue.SetA(.05f),5f);
                }
            }

            for (var i = 0; i < boundSpheresOS.Count; i++)
            {
                var bounds = boundSpheresOS[i];
                bounds.center = skinnedMeshRenderer.bones[i].worldToLocalMatrix.MultiplyPoint(bounds.center);
                boundSpheresOS[i] = bounds;
            }

            instanceMesh.SetUVs(1, indexes);
            instanceMesh.SetUVs(2, weights);
            instanceMesh.boneWeights = null;
            instanceMesh.bindposes = null;
            return instanceMesh;
        }
    }
}
