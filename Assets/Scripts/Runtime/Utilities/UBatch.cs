using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector4> tangents = new List<Vector4>();
            List<Color> colors = new List<Color>();
            List<int> triangles = new List<int>();
            Dictionary<int, List<Vector4>> uvs = new Dictionary<int, List<Vector4>>();
            for (int i = 0; i < 8; i++)
                uvs.Add(i, new List<Vector4>());
            List<Transform> bones = new List<Transform>();
            List<BoneWeight> boneWeightes = new List<BoneWeight>();

            List<Vector4> tempUVfiller=new List<Vector4>();
            foreach (var renderer in group)
            {
                Mesh concatMesh = renderer.sharedMesh;
                meshName += concatMesh.name + " ";
                int triangleOffset = vertices.Count;
                int boneOffset = bones.Count;

                var concatVerticies = concatMesh.vertices;
                int verticiesCount = concatVerticies.Length;
                vertices.AddRange(concatVerticies);
                var concatNormals = concatMesh.normals;
                normals.AddRange(concatNormals.Length > 0 ? concatNormals : new Vector3[verticiesCount]);
                var concatTangents = concatMesh.tangents;
                tangents.AddRange(concatTangents.Length > 0 ? concatTangents : new Vector4[verticiesCount]);
                var concatColors = concatMesh.colors;
                colors.AddRange(concatColors.Length > 0 ? concatColors : new Color[verticiesCount]);

                foreach (var triangle in concatMesh.triangles)
                    triangles.Add(triangleOffset + triangle);

                for (int i = 0; i < 8; i++)
                {
                    concatMesh.GetUVs(i, tempUVfiller);
                    uvs[i].AddRange(tempUVfiller);
                }

                bones.AddRange(renderer.bones);
                boneWeightes.AddRange(concatMesh.boneWeights.Select(boneWeight => new BoneWeight()
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
            batchMesh.boneWeights = boneWeightes.ToArray();
            for (int i = 0; i < 8; i++)
                batchMesh.SetUVsResize(i, uvs[i]);
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
