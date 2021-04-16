using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class USerialize
{
    public static void SetUVsResize(this Mesh _tar,int _index,List<Vector4> uvs)
    {
        if (uvs.Count <= 0)
            return;
        bool third = false;
        bool fourth = false;
        for (int j = 0; j < uvs.Count; j++)
        {
            Vector4 check = uvs[j];
            third |= check.z != 0;
            fourth |= check.w != 0;
        }

        if (fourth)
            _tar.SetUVs(_index, uvs);
        else if (third)
            _tar.SetUVs(_index, uvs.ToList(vec4 => new Vector3(vec4.x, vec4.y, vec4.z)));
        else
            _tar.SetUVs(_index, uvs.ToList(vec4 => new Vector2(vec4.x, vec4.y)));
    }

    public static void CopyMesh(Mesh _src, Mesh _tar)
    {
        _tar.Clear();
        Vector3[] verticies = _src.vertices;
        _tar.vertices =  verticies;
        _tar.normals = _src.normals;
        _tar.tangents = _src.tangents;
        _tar.name = _src.name;
        _tar.bounds = _src.bounds;
        _tar.bindposes = _src.bindposes;
        _tar.colors = _src.colors;
        _tar.boneWeights = _src.boneWeights;
        _tar.triangles = _src.triangles;
        List<Vector4> uvs = new List<Vector4>();
        for (int i = 0; i < 8; i++)
        {
            _src.GetUVs(i, uvs);
            SetUVsResize(_tar, i, uvs);
        }
        for (int i = 0; i < _src.subMeshCount; i++)
            _tar.SetIndices(_src.GetIndices(i), MeshTopology.Triangles, i);

        _tar.ClearBlendShapes();
        _src.TraversalBlendShapes(verticies.Length, (name, index, frame, weight, deltaVerticies, deltaNormals, deltaTangents) => _tar.AddBlendShapeFrame(name, weight, deltaVerticies, deltaNormals, deltaTangents));
    }
    public static void CopyAnimationClip(AnimationClip _src, AnimationClip _dstClip)
    {
        AnimationUtility.SetAnimationEvents(_dstClip, _src.events);
        _dstClip.frameRate = _src.frameRate;
        _dstClip.wrapMode = _src.wrapMode;
        _dstClip.legacy = _src.legacy;
        _dstClip.localBounds = _src.localBounds;
        _dstClip.ClearCurves();
        foreach (var curveBinding in AnimationUtility.GetCurveBindings(_src))
            _dstClip.SetCurve(curveBinding.path, curveBinding.type, curveBinding.propertyName, AnimationUtility.GetEditorCurve(_src, curveBinding));
    }

    public static Mesh Copy(this Mesh _srcMesh)
    {
        Mesh copy = new Mesh();
        CopyMesh(_srcMesh, copy);
        return copy;
    }
    public static AnimationClip Copy(this AnimationClip _srcClip)
    {
        AnimationClip copy = new AnimationClip();
        CopyAnimationClip(_srcClip, copy);
        return copy;
    }
}
