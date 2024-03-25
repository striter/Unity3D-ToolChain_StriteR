using System;
using Runtime.Geometry;
using Runtime.Geometry.Explicit;
using Runtime.Geometry.Explicit.Mesh;
using System.Linq.Extensions;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Gizmos = UnityEngine.Gizmos;

namespace TechToys.ThePlanet.Grid
{
    using static UCubeExplicit;
    using static USphereExplicit;
    using static USphereExplicit.Cube;
    [Serializable]
    public class SphericalGridGenerator : IGridGenerator
    {
        public Transform transform { get; set; }
        public int resolution;
        private GridChunkData m_ChunkData;
        public void Setup()
        {
        }

        public void Tick(float _deltaTime)
        {
        }

        public void Clear()
        {
        }

        public void OnSceneGUI(SceneView _sceneView)
        {
        }

        public void OnGizmos()
        {
            float r = resolution;

            for (int k = 0; k < kCubeFacingAxisCount; k++)
            {
                var _axis = GetFacingAxis(k);
                for (int j = 0; j < resolution; j++)
                for (int i = 0; i < resolution; i++)
                {
                    var vTR =  CubeToSpherePosition(_axis.origin + (i + 1)/r * _axis.uDir + (j+1)/r * _axis.vDir);
                    var vTL =  CubeToSpherePosition(_axis.origin + i/r * _axis.uDir + (j+1)/r * _axis.vDir);
                    var vBR =  CubeToSpherePosition(_axis.origin + (i + 1)/r * _axis.uDir + j/r * _axis.vDir);
                    var vBL =  CubeToSpherePosition(_axis.origin + i/r * _axis.uDir + j/r * _axis.vDir);
                    
                    Gizmos.DrawWireSphere(vTR,.1f);
                    Gizmos.DrawWireSphere(vTL,.1f);
                    Gizmos.DrawWireSphere(vBR,.1f);
                    Gizmos.DrawWireSphere(vBL,.1f);
                }
            }
            
        }

        public void Output(GridCollection _collection)
        {         
            float r = resolution;
            _collection.vertices = new GridVertexData[Cube.GetVertexCount(resolution)];
            _collection.chunks = new GridChunkData[] {new GridChunkData(){ quads =new GridQuadData[USphereExplicit.Cube.GetQuadCount(resolution)]}};
            int quadIndex = 0;
            for (int k = 0; k < kCubeFacingAxisCount; k++)
            {
                var _axis = GetFacingAxis(k);
                for (int j = 0; j < resolution; j++)
                for (int i = 0; i < resolution; i++)
                {
                    var iTR = GetVertexIndex(i + 1, j + 1, resolution, _axis.index);
                    var iTL = GetVertexIndex(i, j + 1, resolution, _axis.index);
                    var iBR = GetVertexIndex(i + 1, j, resolution, _axis.index);
                    var iBL = GetVertexIndex(i, j, resolution, _axis.index);
                    
                    var vTR =  CubeToSpherePosition(_axis.origin + (i + 1)/r * _axis.uDir + (j+1)/r * _axis.vDir);
                    var vTL =  CubeToSpherePosition(_axis.origin + i/r * _axis.uDir + (j+1)/r * _axis.vDir);
                    var vBR =  CubeToSpherePosition(_axis.origin + (i + 1)/r * _axis.uDir + j/r * _axis.vDir);
                    var vBL =  CubeToSpherePosition(_axis.origin + i/r * _axis.uDir + j/r * _axis.vDir);

                    _collection.vertices[iTR] = new GridVertexData() {position = vTR,normal = math.normalize(vTR) , invalid = true};
                    _collection.vertices[iTL] = new GridVertexData() {position = vTL,normal = math.normalize(vTL) , invalid = true};
                    _collection.vertices[iBR] = new GridVertexData() {position = vBR,normal = math.normalize(vBR) , invalid = true};
                    _collection.vertices[iBL] = new GridVertexData() {position = vBL,normal = math.normalize(vBL) , invalid = true};
                    _collection.chunks[0].quads[quadIndex++] = new GridQuadData() {vertices = new PQuad(iBL,iTL,iTR,iBR)};
                }
            }
        }

    }
    
}