using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TPoolStatic;
using UnityEngine;
using Object = UnityEngine.Object;

public static class UBatch 
{
    public static Mesh[] BatchSkinnedMeshRenderer(GameObject _srcObject) => BatchSkinnedMeshRenderer(_srcObject.GetComponentsInChildren<SkinnedMeshRenderer>());
    public static Mesh[] BatchSkinnedMeshRenderer( SkinnedMeshRenderer[] _renderers)
    {
        foreach (var renderer in _renderers)
        {
            if (renderer.sharedMesh.blendShapeCount > 0)
                throw new Exception("Batch Failed! Blend Shape Found!"+renderer.sharedMesh.name);
            if (renderer.sharedMesh.subMeshCount > 1)
                throw new Exception("Batch Failed! Sub Meshes!"+renderer.sharedMesh.name);
            if (!renderer.sharedMesh.isReadable)
                throw new Exception("Batch Failed! Unreadable Mesh!"+renderer.sharedMesh.name);
        }
        var groups = _renderers.GroupBy(p=>p.sharedMaterial);
        Mesh[] batchedMeshes = new Mesh[groups.Count()];
        int batchMeshIndex = -1;
        foreach (var group in groups)
        {
            string meshName = "Batched Mesh:";
            TSPoolList<Vector3>.Spawn(out var vertices);
            TSPoolList<Vector3>.Spawn(out var normals);
            TSPoolList<Vector4>.Spawn(out var tangents);
            TSPoolList<Color>.Spawn(out var colors);
            TSPoolList<int>.Spawn(out var triangles);
            List<Vector4>[] uvs = new List<Vector4>[8];
            for (int i = 0; i < 8; i++)
                uvs[i]= TSPoolList<Vector4>.Spawn();
            TSPoolList<Transform>.Spawn(out var bones);
            TSPoolList<BoneWeight>.Spawn(out var boneWeights);
            TSPoolList<Vector4>.Spawn(out var tempUV);
            foreach (var renderer in group)
            {
                Mesh concatMesh = renderer.sharedMesh;
                meshName += concatMesh.name + " ";
                int triangleOffset = vertices.Count;
                int boneOffset = bones.Count;

                var concatVertices = concatMesh.vertices;
                int verticesCount = concatVertices.Length;
                vertices.AddRange(concatVertices);
                var concatNormals = concatMesh.normals;
                normals.AddRange(concatNormals.Length > 0 ? concatNormals : new Vector3[verticesCount]);
                var concatTangents = concatMesh.tangents;
                tangents.AddRange(concatTangents.Length > 0 ? concatTangents : new Vector4[verticesCount]);
                var concatColors = concatMesh.colors;
                colors.AddRange(concatColors.Length > 0 ? concatColors : new Color[verticesCount]);

                foreach (var triangle in concatMesh.triangles)
                    triangles.Add(triangleOffset + triangle);

                for (int i = 0; i < 8; i++)
                {
                    concatMesh.GetUVs(i, tempUV);
                    uvs[i].AddRange(tempUV);
                }

                bones.AddRange(renderer.bones);
                boneWeights.AddRange(concatMesh.boneWeights.Select(boneWeight => new BoneWeight()
                {
                    boneIndex0 = boneWeight.boneIndex0 + boneOffset,
                    boneIndex1 = boneWeight.boneIndex1 + boneOffset,
                    boneIndex2 = boneWeight.boneIndex2 + boneOffset,
                    boneIndex3 = boneWeight.boneIndex3 + boneOffset,
                    weight0 = boneWeight.weight0,
                    weight1 = boneWeight.weight1,
                    weight2 = boneWeight.weight2,
                    weight3 = boneWeight.weight3,
                }));
            }

            batchMeshIndex++;
            Mesh batchMesh= new Mesh() { name=meshName};
            batchMesh.SetVertices(vertices);
            batchMesh.SetNormals(normals);
            batchMesh.SetTangents(tangents);
            batchMesh.SetTriangles(triangles, 0);
            batchMesh.SetColors(colors);
            batchMesh.boneWeights = boneWeights.ToArray();
            for (int i = 0; i < 8; i++)
                batchMesh.SetUVsResize(i, uvs[i]);
                
            TSPoolList<Vector3>.Recycle(vertices);
            TSPoolList<Vector3>.Recycle(normals);
            TSPoolList<Vector4>.Recycle(tangents);
            TSPoolList<Color>.Recycle(colors);
            TSPoolList<int>.Recycle(triangles);
            for (int i = 0; i < 8; i++)
                TSPoolList<Vector4>.Recycle(uvs[i]);
            TSPoolList<Transform>.Recycle(bones);
            TSPoolList<BoneWeight>.Recycle(boneWeights);
            TSPoolList<Vector4>.Recycle(tempUV);
            
            int rendererIndex=-1;
            foreach (var renderer in group)
            {
                rendererIndex++;
                if (rendererIndex != 0)
                {
                    Object.Destroy(renderer.gameObject);
                    continue;
                }
                Matrix4x4[] bindPoses = new Matrix4x4[bones.Count];
                for (int i = 0; i < bones.Count; i++)
                    bindPoses[i] = bones[i].worldToLocalMatrix * renderer.transform.localToWorldMatrix;
                batchMesh.bindposes = bindPoses;
                renderer.bones = bones.ToArray();
                renderer.sharedMesh = batchMesh;
            }
            batchedMeshes[batchMeshIndex] = batchMesh;
        }
        return batchedMeshes;
    }

}
