using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class UMesh
{
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
    public static bool GetVertexData(this Mesh _srcMesh, EVertexAttribute _dataType, List<Vector4> _data)
    {
        _data.Clear();
        switch (_dataType)
        {
            default: throw new Exception("Invalid Vertex Data Type" + _dataType);
            case EVertexAttribute.UV0:
            case EVertexAttribute.UV1:
            case EVertexAttribute.UV2:
            case EVertexAttribute.UV3:
            case EVertexAttribute.UV4:
            case EVertexAttribute.UV5:
            case EVertexAttribute.UV6:
            case EVertexAttribute.UV7:
                _srcMesh.GetUVs((int)_dataType, _data);
                break;
            case EVertexAttribute.Color:
                List<Color> colors = new List<Color>();
                _srcMesh.GetColors(colors);
                foreach (var color in colors)
                    _data.Add(color.toV4());
                break;
            case EVertexAttribute.Tangent:
                _srcMesh.GetTangents(_data);
                break;
            case EVertexAttribute.Normal:
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
    public static void SetVertexData(this Mesh _srcMesh, EVertexAttribute _dataType, List<Vector4> _data)
    {
        switch (_dataType)
        {
            default: throw new Exception("Invalid Vertex Data Type" + _dataType);
            case EVertexAttribute.UV0:
            case EVertexAttribute.UV1:
            case EVertexAttribute.UV2:
            case EVertexAttribute.UV3:
            case EVertexAttribute.UV4:
            case EVertexAttribute.UV5:
            case EVertexAttribute.UV6:
            case EVertexAttribute.UV7:
                _srcMesh.SetUVs(UEnum.GetIndex(_dataType), _data);
                break;
            case EVertexAttribute.Color:
                _srcMesh.SetColors(_data.Select(p => p.toColor()).ToArray());
                break;
            case EVertexAttribute.Tangent:
                _srcMesh.SetTangents(_data);
                break;
            case EVertexAttribute.Normal:
                _srcMesh.SetNormals(_data.Select(vec4 => vec4.XYZ()).ToArray());
                break;
        }
    }
    public static void SetVertexData(this Mesh _srcMesh, EVertexAttribute _dataType, Vector4[] _data)
    {
        switch (_dataType)
        {
            default: throw new Exception("Invalid Vertex Data Type" + _dataType);
            case EVertexAttribute.UV0:
            case EVertexAttribute.UV1:
            case EVertexAttribute.UV2:
            case EVertexAttribute.UV3:
            case EVertexAttribute.UV4:
            case EVertexAttribute.UV5:
            case EVertexAttribute.UV6:
            case EVertexAttribute.UV7:
                _srcMesh.SetUVs(UEnum.GetIndex(_dataType), _data);
                break;
            case EVertexAttribute.Color:
                _srcMesh.SetColors(_data.Select(p => new Color(p.x, p.y, p.z, p.w)).ToArray());
                break;
            case EVertexAttribute.Tangent:
                _srcMesh.SetTangents(_data);
                break;
            case EVertexAttribute.Normal:
                _srcMesh.SetNormals(_data.Select(vec4 => vec4.XYZ()).ToArray());
                break;
        }
    }
    public static void SetVertexData(this Mesh _srcMesh, EVertexAttribute _dataType, Vector3[] _data)
    {
        switch (_dataType)
        {
            default: throw new Exception("Invalid Vertex Data Type" + _dataType);
            case EVertexAttribute.UV0:
            case EVertexAttribute.UV1:
            case EVertexAttribute.UV2:
            case EVertexAttribute.UV3:
            case EVertexAttribute.UV4:
            case EVertexAttribute.UV5:
            case EVertexAttribute.UV6:
            case EVertexAttribute.UV7:
                _srcMesh.SetUVs(UEnum.GetIndex(_dataType), _data);
                break;
            case EVertexAttribute.Color:
                _srcMesh.SetColors(_data.Select(p => new Color(p.x, p.y, p.z, 1.0f)).ToArray());
                break;
            case EVertexAttribute.Tangent:
                _srcMesh.SetTangents(_data.Select(vec3 => new Vector4(vec3.x, vec3.y, vec3.z, 1.0f)).ToArray());
                break;
            case EVertexAttribute.Normal:
                _srcMesh.SetNormals(_data);
                break;
        }
    }
    public static bool GetVertexData(this Mesh _srcMesh, EVertexAttribute _dataType, List<Vector3> _data)
    {
        _data.Clear();
        switch (_dataType)
        {
            default: throw new Exception("Invalid Vertex Data Type" + _dataType);
            case EVertexAttribute.UV0:
            case EVertexAttribute.UV1:
            case EVertexAttribute.UV2:
            case EVertexAttribute.UV3:
            case EVertexAttribute.UV4:
            case EVertexAttribute.UV5:
            case EVertexAttribute.UV6:
            case EVertexAttribute.UV7:
                _srcMesh.GetUVs((int)_dataType, _data);
                break;
            case EVertexAttribute.Color:
                List<Color> colors = new List<Color>();
                _srcMesh.GetColors(colors);
                foreach (var color in colors)
                    _data.Add(color.toV4());
                break;
            case EVertexAttribute.Tangent:
                List<Vector4> tangents = new List<Vector4>();
                _srcMesh.GetTangents(tangents);
                foreach (var tangent in tangents)
                    _data.Add(tangent.XYZ());
                break;
            case EVertexAttribute.Normal:
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
    public static void SetVertexData(this Mesh _srcMesh, EVertexAttribute _dataType, List<Vector3> _data)
    {
        switch (_dataType)
        {
            default: throw new Exception("Invalid Vertex Data Type" + _dataType);
            case EVertexAttribute.UV0:
            case EVertexAttribute.UV1:
            case EVertexAttribute.UV2:
            case EVertexAttribute.UV3:
            case EVertexAttribute.UV4:
            case EVertexAttribute.UV5:
            case EVertexAttribute.UV6:
            case EVertexAttribute.UV7:
                _srcMesh.SetUVs((int)_dataType, _data);
                break;
            case EVertexAttribute.Color:
                _srcMesh.SetColors(_data.Select(p => new Color(p.x, p.y, p.z,0)).ToArray());
                break;
            case EVertexAttribute.Tangent:
                _srcMesh.SetTangents(_data.Select(p => p.ToVector4()).ToArray());
                break;
            case EVertexAttribute.Normal:
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
}
