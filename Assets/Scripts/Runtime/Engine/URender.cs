using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Geometry;
using UnityEngine.Rendering;

public static class URender
{
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
                    _data.Add(color.toV4());
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
                _srcMesh.SetUVs(UEnum.GetIndex(_dataType), _data);
                break;
            case EVertexData.Color:
                _srcMesh.SetColors(_data.Select(p => new Color(p.x, p.y, p.z, p.w)).ToArray());
                break;
            case EVertexData.Tangent:
                _srcMesh.SetTangents(_data);
                break;
            case EVertexData.Normal:
                _srcMesh.SetNormals(_data.Select(vec4 => vec4.XYZ()).ToArray());
                break;
        }
    }
    public static void SetVertexData(this Mesh _srcMesh, EVertexData _dataType, Vector4[] _data)
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
                _srcMesh.SetNormals(_data.Select(vec4 => vec4.XYZ()).ToArray());
                break;
        }
    }
    public static void SetVertexData(this Mesh _srcMesh, EVertexData _dataType, Vector3[] _data)
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
                _srcMesh.SetUVs(UEnum.GetIndex(_dataType), _data);
                break;
            case EVertexData.Color:
                _srcMesh.SetColors(_data.Select(p => new Color(p.x, p.y, p.z, 1.0f)).ToArray());
                break;
            case EVertexData.Tangent:
                _srcMesh.SetTangents(_data.Select(vec3 => new Vector4(vec3.x, vec3.y, vec3.z, 1.0f)).ToArray());
                break;
            case EVertexData.Normal:
                _srcMesh.SetNormals(_data);
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
                    _data.Add(color.toV4());
                break;
            case EVertexData.Tangent:
                List<Vector4> tangents = new List<Vector4>();
                _srcMesh.GetTangents(tangents);
                foreach (var tangent in tangents)
                    _data.Add(tangent.XYZ());
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
        if (_material == null)
        {
            Debug.LogWarning("Mull Material Found.");
            return false;
        }
        
        if (_enable)
            _material.EnableKeyword(_keyword);
        else
            _material.DisableKeyword(_keyword);
        return _enable;
    }

    public static void EnableKeywords(this Material _material, string[] _keywords, int _target)
    {
        for (int i = 0; i < _keywords.Length; i++)
            _material.EnableKeyword(_keywords[i], i + 1 == _target);
    }

    public static bool EnableKeywords<T>(this Material _material, T _target) where T:Enum
    {
        int index = UEnum.GetIndex(_target);
        var keywords = UEnum.GetEnums<T>();
        for (int i = 0; i < keywords.Length; i++)
            _material.EnableKeyword(keywords[i].ToString(), i == index);

        return index != 0;
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
    public static bool EnableGlobalKeywords<T>(T _target) where T : Enum
    {
        int index = UEnum.GetIndex(_target);
        var keywords = UEnum.GetEnums<T>();
        for (int i = 0; i < keywords.Length; i++)
            EnableGlobalKeyword(keywords[i].ToString(), i==index);
        return index != 0;
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

    public static LocalKeyword[] GetLocalKeywords<T>(this ComputeShader _compute) where T:Enum
    {
        var keywords = UEnum.GetEnums<T>();
        LocalKeyword[] localKeywords = new LocalKeyword[keywords.Length];
        for (int i = 0; i < keywords.Length; i++)
            localKeywords[i] = new LocalKeyword(_compute,keywords[i].ToString());
        return localKeywords;
    }

    public static void EnableLocalKeywords<T>(this CommandBuffer _buffer,ComputeShader _shader,LocalKeyword[] _keywords,T _value) where T:Enum
    {
        int index = UEnum.GetIndex(_value);
        var keywords = UEnum.GetEnums<T>();
        LocalKeyword[] localKeywords = new LocalKeyword[keywords.Length];
        for (int i = 0; i < keywords.Length; i++)
        {
            if(i==index)
                _buffer.EnableKeyword(_shader,_keywords[i]);
            else
                _buffer.DisableKeyword(_shader,_keywords[i]);
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
    
    public static Vector4 GetTexelSizeParameters(this Texture _texture)=>new Vector4(1f/_texture.width,1f/_texture.height,_texture.width,_texture.height);
}
