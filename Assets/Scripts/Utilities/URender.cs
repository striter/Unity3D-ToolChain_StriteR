using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class URender
{
    public static GMeshPolygon[] GetPolygons(int[] _indices)
    {
        GMeshPolygon[] polygons = new GMeshPolygon[_indices.Length / 3];
        for (int i = 0; i < polygons.Length; i++)
        {
            int startIndex = i * 3;
            int triangle0 = _indices[startIndex];
            int triangle1 = _indices[startIndex + 1];
            int triangle2 = _indices[startIndex + 2];
            polygons[i] = new GMeshPolygon(triangle0, triangle1, triangle2);
        }
        return polygons;
    }
    public static GMeshPolygon[] GetPolygons(this Mesh _srcMesh, out int[] _indices)
    {
        _indices = _srcMesh.triangles;
        return GetPolygons(_indices);
    }
    #region Mesh Edit
    public static void TraversalBlendShapes(this Mesh _srcMesh, int _VertexCount, Action<string, int, int, float, Vector3[], Vector3[], Vector3[]> _OnEachFrame)
    {
        Vector3[] deltaVerticies = new Vector3[_VertexCount];
        Vector3[] deltaNormals = new Vector3[_VertexCount];
        Vector3[] deltaTangents = new Vector3[_VertexCount];
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
    public static bool GetVertexData(this Mesh _srcMesh, enum_VertexData _dataType, List<Vector4> _data)
    {
        _data.Clear();
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
                _srcMesh.GetUVs((int)_dataType, _data);
                break;
            case enum_VertexData.Color:
                List<Color> colors = new List<Color>();
                _srcMesh.GetColors(colors);
                foreach (var color in colors)
                    _data.Add(color.ToVector());
                break;
            case enum_VertexData.Tangent:
                _srcMesh.GetTangents(_data);
                break;
            case enum_VertexData.Normal:
                {
                    List<Vector3> normalList = new List<Vector3>();
                    _srcMesh.GetNormals(normalList);
                    foreach (var normal in normalList)
                        _data.Add(normal);
                }
                break;
        }
        return _data != null;
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
            case enum_VertexData.Color:
                _srcMesh.SetColors(_data.ToArray(p => new Color(p.x, p.y, p.z, p.w)));
                break;
            case enum_VertexData.Tangent:
                _srcMesh.SetTangents(_data);
                break;
            case enum_VertexData.Normal:
                _srcMesh.SetNormals(_data.ToArray(vec4 => vec4.ToVector3()));
                break;
        }
    }
    public static bool GetVertexData(this Mesh _srcMesh, enum_VertexData _dataType, List<Vector3> _data)
    {
        _data.Clear();
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
                _srcMesh.GetUVs((int)_dataType, _data);
                break;
            case enum_VertexData.Color:
                List<Color> colors = new List<Color>();
                _srcMesh.GetColors(colors);
                foreach (var color in colors)
                    _data.Add(color.ToVector());
                break;
            case enum_VertexData.Tangent:
                List<Vector4> tangents = new List<Vector4>();
                _srcMesh.GetTangents(tangents);
                foreach (var tangent in tangents)
                    _data.Add(tangent.ToVector3());
                break;
            case enum_VertexData.Normal:
                {
                    List<Vector3> normalList = new List<Vector3>();
                    _srcMesh.GetNormals(normalList);
                    foreach (var normal in normalList)
                        _data.Add(normal);
                }
                break;
        }
        return _data != null;
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
            case enum_VertexData.Color:
                _srcMesh.SetColors(_data.ToArray(p => new Color(p.x, p.y, p.z, 0)));
                break;
            case enum_VertexData.Tangent:
                _srcMesh.SetTangents(_data.ToArray(p => p.ToVector4(1f)));
                break;
            case enum_VertexData.Normal:
                _srcMesh.SetNormals(_data);
                break;
        }
    }
    public static void SetUVsResize(this Mesh _tar, int _index, List<Vector4> uvs)
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
    #endregion
    //Material
    public static void EnableKeyword(this Material _material, string _keyword, bool _enable)
    {
        if (_enable)
            _material.EnableKeyword(_keyword);
        else
            _material.DisableKeyword(_keyword);
    }
    public static void EnableKeywords<T>(this Material _material, string[] _keywords, T _target) where T : Enum => EnableKeywords(_material, _keywords, Convert.ToInt32(_target));
    public static void EnableKeywords(this Material _material, string[] _keywords, int _target)
    {
        for (int i = 0; i < _keywords.Length; i++)
            _material.EnableKeyword(_keywords[i], i + 1 == _target);
    }
    //Compute Shader
    public static void EnableKeyword(this ComputeShader _computeShader, string _keyword, bool _enable)
    {
        if (_enable)
            _computeShader.EnableKeyword(_keyword);
        else
            _computeShader.DisableKeyword(_keyword);
    }
    public static void EnableKeywords<T>(this ComputeShader _computeShader, string[] _keywords, T _target) where T : Enum => EnableKeywords(_computeShader, _keywords, Convert.ToInt32(_target));
    public static void EnableKeywords(this ComputeShader _computeShader, string[] _keywords, int _target)
    {
        for (int i = 0; i < _keywords.Length; i++)
            _computeShader.EnableKeyword(_keywords[i], i + 1 == _target);
    }
    //Global
    public static void EnableGlobalKeywords<T>(string[] _keywords, T _target) where T : Enum => EnableGlobalKeywords(_keywords, Convert.ToInt32(_target));
    public static void EnableGlobalKeywords(string[] _keywords, int _target)
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

public static class UBoundsChecker
{
    static Vector3 m_BoundsMin;
    static Vector3 m_BoundsMax;
    public static void Begin()
    {
        m_BoundsMin = Vector3.zero;
        m_BoundsMax = Vector3.zero;
    }
    public static void CheckBounds(Vector3 vertex)
    {
        m_BoundsMin = Vector3.Min(m_BoundsMin, vertex);
        m_BoundsMax = Vector3.Max(m_BoundsMax, vertex);
    }
    public static Bounds CalculateBounds() => new Bounds((m_BoundsMin + m_BoundsMax) / 2, m_BoundsMax - m_BoundsMin);

    public static Bounds GetBounds(Vector3[] _verticies)
    {
        Begin();
        foreach (var vertex in _verticies)
            CheckBounds(vertex);
        return CalculateBounds();
    }
}
