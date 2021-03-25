using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class URender
{
    public static MeshPolygon[] GetPolygons(int[] _indices)
    {
        MeshPolygon[] polygons = new MeshPolygon[_indices.Length / 3];
        for (int i = 0; i < polygons.Length; i++)
        {
            int startIndex = i * 3;
            int triangle0 = _indices[startIndex];
            int triangle1 = _indices[startIndex + 1];
            int triangle2 = _indices[startIndex + 2];
            polygons[i] = new MeshPolygon(triangle0, triangle1, triangle2);
        }
        return polygons;
    }
    public static MeshPolygon[] GetPolygons(this Mesh _srcMesh, out int[] _indices)
    {
        _indices = _srcMesh.triangles;
        return GetPolygons(_indices);
    }
    public static void TraversalBlendShapes(this Mesh _srcMesh, Action<string, int, int, float, Vector3[], Vector3[], Vector3[]> _OnEachFrame)
    {
        Vector3[] deltaVerticies = null;
        Vector3[] deltaNormals = null;
        Vector3[] deltaTangents = null;
        int totalBlendshapes = _srcMesh.blendShapeCount;
        for (int i = 0; i < _srcMesh.blendShapeCount; i++)
        {
            int frameCount = _srcMesh.GetBlendShapeFrameCount(i);
            string name = _srcMesh.GetBlendShapeName(i);
            for (int j = 0; j < frameCount; j++)
            {
                float weight = _srcMesh.GetBlendShapeFrameWeight(i, j);
                _srcMesh.GetBlendShapeFrameVertices(i, j, deltaVerticies, deltaNormals, deltaTangents);
                _OnEachFrame(name, i, j, weight, deltaVerticies, deltaNormals, deltaTangents);
            }
        }
    }
    public static bool GetVertexData(this Mesh _srcMesh, enum_VertexData _dataType, List<Vector4> vertexData)
    {
        vertexData.Clear();
        switch (_dataType)
        {
            default: throw new Exception("Invalid Vertex Data Type" + _dataType);
            case enum_VertexData.UV0:
            case enum_VertexData.UV1:
            case enum_VertexData.UV2:
            case enum_VertexData.UV3:
            case enum_VertexData.UV4:
            case enum_VertexData.UV5:
            case enum_VertexData.UV6:
            case enum_VertexData.UV7:
                _srcMesh.GetUVs((int)_dataType, vertexData);
                break;
            case enum_VertexData.Tangent:
                _srcMesh.GetTangents(vertexData);
                break;
            case enum_VertexData.Normal:
                {
                    List<Vector3> normalList = new List<Vector3>();
                    _srcMesh.GetNormals(normalList);
                    foreach (var normal in normalList)
                        vertexData.Add(normal);
                }
                break;
        }
        return vertexData != null;
    }
    public static void SetVertexData(this Mesh _srcMesh, enum_VertexData _dataType, List<Vector4> _data)
    {
        switch (_dataType)
        {
            default: throw new Exception("Invalid Vertex Data Type" + _dataType);
            case enum_VertexData.UV0:
            case enum_VertexData.UV1:
            case enum_VertexData.UV2:
            case enum_VertexData.UV3:
            case enum_VertexData.UV4:
            case enum_VertexData.UV5:
            case enum_VertexData.UV6:
            case enum_VertexData.UV7:
                _srcMesh.SetUVs((int)_dataType, _data);
                break;
            case enum_VertexData.Tangent:
                _srcMesh.SetTangents(_data);
                break;
            case enum_VertexData.Normal:
                _srcMesh.SetNormals(_data.ToArray(vec4 => vec4.ToVector3()));
                break;
        }
    }
    public static bool GetVertexData(this Mesh _srcMesh, enum_VertexData _dataType, List<Vector3> vertexData)
    {
        vertexData.Clear();
        switch (_dataType)
        {
            default: throw new Exception("Invalid Vertex Data Type" + _dataType);
            case enum_VertexData.UV0:
            case enum_VertexData.UV1:
            case enum_VertexData.UV2:
            case enum_VertexData.UV3:
            case enum_VertexData.UV4:
            case enum_VertexData.UV5:
            case enum_VertexData.UV6:
            case enum_VertexData.UV7:
                _srcMesh.GetUVs((int)_dataType, vertexData);
                break;
            case enum_VertexData.Tangent:
                List<Vector4> tangents = new List<Vector4>();
                _srcMesh.GetTangents(tangents);
                foreach (var tangent in tangents)
                    vertexData.Add(tangent.ToVector3());
                break;
            case enum_VertexData.Normal:
                {
                    List<Vector3> normalList = new List<Vector3>();
                    _srcMesh.GetNormals(normalList);
                    foreach (var normal in normalList)
                        vertexData.Add(normal);
                }
                break;
        }
        return vertexData != null;
    }
    public static void SetVertexData(this Mesh _srcMesh, enum_VertexData _dataType, List<Vector3> _data)
    {
        switch (_dataType)
        {
            default: throw new Exception("Invalid Vertex Data Type" + _dataType);
            case enum_VertexData.UV0:
            case enum_VertexData.UV1:
            case enum_VertexData.UV2:
            case enum_VertexData.UV3:
            case enum_VertexData.UV4:
            case enum_VertexData.UV5:
            case enum_VertexData.UV6:
            case enum_VertexData.UV7:
                _srcMesh.SetUVs((int)_dataType, _data);
                break;
            case enum_VertexData.Tangent:
                _srcMesh.SetTangents(_data.ToArray(p => p.ToVector4(1f)));
                break;
            case enum_VertexData.Normal:
                _srcMesh.SetNormals(_data);
                break;
        }
    }

    public static void EnableKeyword(this Material _material, string _keyword, bool _enable)
    {
        if (_enable)
            _material.EnableKeyword(_keyword);
        else
            _material.DisableKeyword(_keyword);
    }
    public static void EnableKeywords(this Material _material, string[] _keywords, int _target)
    {
        for (int i = 0; i < _keywords.Length; i++)
            _material.EnableKeyword(_keywords[i], i + 1 == _target);
    }
    public static void EnableGlobalKeyword(string[] _keywords, int _target)
    {
        for (int i = 0; i < _keywords.Length; i++)
            EnableGlobalKeyword(_keywords[i], (i + 1) == _target);
    }

    public static void EnableGlobalKeyword(string _keyword, bool _enable)
    {
        if (_enable)
            Shader.EnableKeyword(_keyword);
        else
            Shader.DisableKeyword(_keyword);
    }

}