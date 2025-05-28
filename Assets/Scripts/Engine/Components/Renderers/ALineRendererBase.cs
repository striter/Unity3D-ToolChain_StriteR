using System.Collections.Generic;
using Runtime.Geometry.Curves;
using Runtime.Pool;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime
{
    public abstract class ALineRendererBase : ARendererBase , IRendererViewSpace
    {
        [Header("Shape")]
        public float m_Width = .1f;
        public bool m_Billboard = false;
        public bool m_JointSubdivision = false;
        [Foldout(nameof(m_JointSubdivision), true)] [Range(1,180)] public float m_TolerangeInAngles = 10f;
        [Foldout(nameof(m_JointSubdivision), true)] [Range(0f,3f)] public float m_DistanceDamper = 0.1f;
        [Foldout(nameof(m_JointSubdivision), true)] public bool m_ExtraDivision;

        protected abstract void PopulatePositions(List<Vector3> _vertices, List<Vector3> _tangents);

        private void PopulatePositions_Internal(List<Vector3> _positions, List<Vector3> _tangents,Transform _viewTransform)
        {
            PopulatePositions(_positions,_tangents);
            var count = _positions.Count;
            if (m_JointSubdivision)
            {
                for (int i = 1; i < count - 1; i++)
                {
                    var position = _positions[i];
                    var tangent = _tangents[i];
                    var preTangent = _tangents[i - 1];
                    var preDirection = (position - _positions[i - 1]).normalized;
                    var direction = (_positions[i + 1] - position).normalized;
                    var up = math.cross( direction,tangent);
                    var degree = umath.closestAngle(direction, preDirection, up) ;
                    var sign = math.sign( degree);
                    var degreeValue = degree * sign;
                    if( degreeValue < m_TolerangeInAngles)
                        continue;

                    var ankleSide = preTangent * m_Width;
                    var anklePosition = position - sign * tangent * m_Width + sign * ankleSide;
                    var ankleDistance =  degreeValue * m_DistanceDamper / 360;

                    var start = anklePosition - preDirection * ankleDistance;
                    var end = position + direction * ankleDistance;
                    
                    _positions[i] = end;
                    if (m_ExtraDivision)
                    {
                        var distance = math.distance(start, end);
                        var bezier = new GBezierCurveQuadratic(start,end,position   );

                        var tessellationAmount = (int)math.ceil(degreeValue / m_TolerangeInAngles);
                        var tessellationFragment = 1f / (tessellationAmount + 1);
                        for (int j = tessellationAmount; j >= 0; j--)
                        {
                            var interpolation = tessellationFragment * j;
                            _positions.Insert(i,bezier.Evaluate(interpolation));
                            _tangents.Insert(i, math.lerp(preTangent,tangent,interpolation));//math.cross(up, bezier.EvaluateTangent(interpolation)));
                        }

                        i += tessellationAmount + 1;
                        count += tessellationAmount + 1;
                    }
                    else
                    {
                        _positions.Insert(i, start);
                        _tangents.Insert(i, preTangent);
                        i++;
                        count++;
                    }
                }
            }

            if (m_Billboard)
            {
                for (int i = 0; i < count; i++)
                {
                    if (i == count - 1)
                    {
                        _tangents[i] = _tangents[i - 1];
                        continue;
                    }
                    
                    var position = _positions[i];
                    var C = _viewTransform.position;
                    var Z = (C - position).normalized;
                    var T = ( _positions[i + 1] - position).normalized;
                    _tangents[i] = Vector3.Cross(Z,T);
                }
            }
        }
        
        protected sealed override void PopulateMesh(Mesh _mesh,Transform _viewTransform)
        {
            Matrix4x4 worldToLocal = transform.worldToLocalMatrix;
            PoolList<Vector3>.ISpawn(out var positions);
            PoolList<Vector3>.ISpawn(out var tangents);
            PopulatePositions_Internal(positions, tangents,_viewTransform);
            var count = positions.Count;
            if (count > 1)
            {
                //Fill positions
                PoolList<Vector3>.ISpawn(out var vertices);
                PoolList<Vector4>.ISpawn(out var uvs);
                PoolList<int>.ISpawn(out var indexes);

                var totalLength = 0f;
                var curIndex = 0;
                
                for (int i = 0; i < count; i++)
                {
                    var position = positions[i];
                    var tangent = tangents[i];
                    // Debug.DrawRay(position,tangent,Color.red);
                    
                    vertices.Add( worldToLocal.MultiplyPoint(position + tangent * m_Width));
                    uvs.Add(new Vector4(totalLength, 0));
                    vertices.Add( worldToLocal.MultiplyPoint(position - tangent * m_Width));
                    uvs.Add(new Vector4(totalLength, 1));

                    if (i != count - 1)
                    {
                        var delta = (positions[i + 1] - position);
                        var sqrLength = delta.sqrMagnitude;
                        if (sqrLength > 0.00001f)
                        {
                            totalLength += math.sqrt(sqrLength);
                        }

                        indexes.Add(curIndex);
                        indexes.Add(curIndex + 1);
                        indexes.Add(curIndex + 2);

                        indexes.Add(curIndex + 2);
                        indexes.Add(curIndex + 1);
                        indexes.Add(curIndex + 3);
                        curIndex += 2;
                    }
                }

                var lastPoint = positions[^1];
                var lastTangent = tangents[^1].normalized;
                vertices.Add(worldToLocal.MultiplyPoint(lastPoint + lastTangent * m_Width));
                uvs.Add( new Vector4(totalLength, 0));
                vertices.Add(worldToLocal.MultiplyPoint(lastPoint - lastTangent * m_Width));
                uvs.Add(new Vector4(totalLength, 1));
                
                _mesh.SetVertices(vertices);
                _mesh.SetUVs(0,uvs);
                _mesh.SetTriangles(indexes,0,false);
                _mesh.RecalculateBounds();
                _mesh.RecalculateNormals();
                
                PoolList<Vector3>.IDespawn(vertices);
                PoolList<Vector4>.IDespawn(uvs);
                PoolList<int>.IDespawn(indexes);
            }
            PoolList<Vector3>.IDespawn(positions);
            PoolList<Vector3>.IDespawn(tangents);
        }

        public bool ViewSpaceRequired => m_Billboard;
    }

    
    
}
