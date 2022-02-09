using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Geometry.Polygon;
using Geometry.Voxel;
using UnityEngine.Rendering;

public static class URender
{
    public static readonly int kIDColor = Shader.PropertyToID("_Color");
    public static readonly int kIDAlpha = Shader.PropertyToID("_Alpha");
    
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
    public static bool GetVertexData(this Mesh _srcMesh, EVertexData _dataType, List<Vector4> _data)
    {
        _data.Clear();
        switch (_dataType)
        {
            default: throw new Exception("Invalid Vertex Data Type" + _dataType);
            case EVertexData.UV0:
            case EVertexData.UV1:
            case EVertexData.UV2:
            case EVertexData.UV3:
            case EVertexData.UV4:
            case EVertexData.UV5:
            case EVertexData.UV6:
            case EVertexData.UV7:
                _srcMesh.GetUVs((int)_dataType, _data);
                break;
            case EVertexData.Color:
                List<Color> colors = new List<Color>();
                _srcMesh.GetColors(colors);
                foreach (var color in colors)
                    _data.Add(color.ToVector());
                break;
            case EVertexData.Tangent:
                _srcMesh.GetTangents(_data);
                break;
            case EVertexData.Normal:
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
    public static void SetVertexData(this Mesh _srcMesh, EVertexData _dataType, List<Vector4> _data)
    {
        switch (_dataType)
        {
            default: throw new Exception("Invalid Vertex Data Type" + _dataType);
            case EVertexData.UV0:
            case EVertexData.UV1:
            case EVertexData.UV2:
            case EVertexData.UV3:
            case EVertexData.UV4:
            case EVertexData.UV5:
            case EVertexData.UV6:
            case EVertexData.UV7:
                _srcMesh.SetUVs((int)_dataType, _data);
                break;
            case EVertexData.Color:
                _srcMesh.SetColors(_data.Select(p => new Color(p.x, p.y, p.z, p.w)).ToArray());
                break;
            case EVertexData.Tangent:
                _srcMesh.SetTangents(_data);
                break;
            case EVertexData.Normal:
                _srcMesh.SetNormals(_data.Select(vec4 => vec4.ToVector3()).ToArray());
                break;
        }
    }
    public static bool GetVertexData(this Mesh _srcMesh, EVertexData _dataType, List<Vector3> _data)
    {
        _data.Clear();
        switch (_dataType)
        {
            default: throw new Exception("Invalid Vertex Data Type" + _dataType);
            case EVertexData.UV0:
            case EVertexData.UV1:
            case EVertexData.UV2:
            case EVertexData.UV3:
            case EVertexData.UV4:
            case EVertexData.UV5:
            case EVertexData.UV6:
            case EVertexData.UV7:
                _srcMesh.GetUVs((int)_dataType, _data);
                break;
            case EVertexData.Color:
                List<Color> colors = new List<Color>();
                _srcMesh.GetColors(colors);
                foreach (var color in colors)
                    _data.Add(color.ToVector());
                break;
            case EVertexData.Tangent:
                List<Vector4> tangents = new List<Vector4>();
                _srcMesh.GetTangents(tangents);
                foreach (var tangent in tangents)
                    _data.Add(tangent.ToVector3());
                break;
            case EVertexData.Normal:
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
    public static void SetVertexData(this Mesh _srcMesh, EVertexData _dataType, List<Vector3> _data)
    {
        switch (_dataType)
        {
            default: throw new Exception("Invalid Vertex Data Type" + _dataType);
            case EVertexData.UV0:
            case EVertexData.UV1:
            case EVertexData.UV2:
            case EVertexData.UV3:
            case EVertexData.UV4:
            case EVertexData.UV5:
            case EVertexData.UV6:
            case EVertexData.UV7:
                _srcMesh.SetUVs((int)_dataType, _data);
                break;
            case EVertexData.Color:
                _srcMesh.SetColors(_data.Select(p => new Color(p.x, p.y, p.z,0)).ToArray());
                break;
            case EVertexData.Tangent:
                _srcMesh.SetTangents(_data.Select(p => p.ToVector4()).ToArray());
                break;
            case EVertexData.Normal:
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
            _tar.SetUVs(_index, uvs.Select(vec4 => new Vector3(vec4.x, vec4.y, vec4.z)).ToArray());
        else
            _tar.SetUVs(_index, uvs.Select(vec4 => new Vector2(vec4.x, vec4.y)).ToArray());
    }
    #endregion
    //Material
    public static bool EnableKeyword(this Material _material, string _keyword, bool _enable)
    {
        if (_enable)
            _material.EnableKeyword(_keyword);
        else
            _material.DisableKeyword(_keyword);
        return _enable;
    }

    public static bool EnableKeywords<T>(this Material _material, string[] _keywords, T _target) where T : Enum
    {
        int index = Convert.ToInt32(_target);
        EnableKeywords(_material, _keywords, index);
        return index != 0;
    }

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
    public static bool EnableGlobalKeywords<T>(string[] _keywords, T _target) where T : Enum => EnableGlobalKeywords(_keywords, Convert.ToInt32(_target));
    public static bool EnableGlobalKeywords(string[] _keywords, int _target)
    {
        for (int i = 0; i < _keywords.Length; i++)
            EnableGlobalKeyword(_keywords[i], (i + 1) == _target);
        return _target != 0;
    }
    public static bool EnableGlobalKeyword(string _keyword, bool _enable)
    {
        if (_enable)
            Shader.EnableKeyword(_keyword);
        else
            Shader.DisableKeyword(_keyword);
        return _enable;
    }
    public static void EnableKeyword(this CommandBuffer _buffer,string _keyword, bool _enable)
    {
        if (_enable)
            _buffer.EnableShaderKeyword(_keyword);
        else
            _buffer.DisableShaderKeyword(_keyword);
    }

    public static Vector2 IndexToQuadUV(int _index)
    {
        switch (_index)
        {
            case 0:return Vector2.zero;
            case 1: return Vector2.right;
            case 2: return Vector2.one;
            case 3: return Vector2.up;
            default: throw new Exception("Invalid Index:" + _index);
        }
    }

    // public static GFrustum CalculatePerspectiveFrustum(this Camera _camera)
    // {
    //     Quaternion rotation = _camera.transform.rotation;
    //     
    //     GFrustum frustum=new GFrustum(_camera.fieldOfView,_camera.aspect,_camera.nearClipPlane,_camera.farClipPlane);
    //
    //     
    //     return frustum;
    // }
    //
    public static void CalculateOrthographicPositions(this Camera camera, out Vector3 tl, out Vector3 tr,out Vector3 bl, out Vector3 br)
    {
        float aspect = camera.aspect;
        float halfHeight = camera.orthographicSize;
        Transform cameraTrans = camera.transform;
        Vector3 toRight = cameraTrans.right * halfHeight * aspect;
        Vector3 toTop = cameraTrans.up * halfHeight;
        Vector3 startPos = cameraTrans.position+cameraTrans.forward*camera.nearClipPlane;
        tl = startPos - toRight + toTop;
        tr = startPos + toRight + toTop;
        bl = startPos - toRight - toTop;
        br = startPos + toRight - toTop;
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

    public static Bounds GetBounds(IEnumerable<Vector3> _vertices)
    {
        Begin();
        foreach (var vertex in _vertices)
            CheckBounds(vertex);
        return CalculateBounds();
    }
}
