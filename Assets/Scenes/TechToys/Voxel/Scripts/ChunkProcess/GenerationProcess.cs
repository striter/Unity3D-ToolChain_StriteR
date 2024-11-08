using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace TheVoxel.ChunkProcess
{
    
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct ImplicitJob : IJobFor
    {
        private TerrainData terrainData;
        private Int2 identity;
        private int startIndex;
        [ReadOnly] [NativeDisableContainerSafetyRestriction] private NativeHashMap<Int3, ChunkVoxel> indexes;
        public ImplicitJob(Int2 _identity, NativeHashMap<Int3, ChunkVoxel> _indexes)
        {
            terrainData = ChunkManager.Instance;
            startIndex = 0;
            identity = _identity;
            indexes = _indexes;
        }

        void Insert(Int3 _identity,EVoxelType _type)
        {
            indexes.Add(_identity,new ChunkVoxel(){identity = _identity ,type = _type});
        }

        void Remove(Int3 _identity)
        {
            indexes.Remove(_identity);
        }

        int Plane(int2 index,int surfaceHeight,float random,float2 p2D)
        {
            float planeRandom = Noise.Perlin.Unit1f2(p2D * terrainData.planeScale) *.5f + .5f;
            int dirtRandom = terrainData.dirtRandom.GetValueContains(random);
            surfaceHeight += terrainData.planeHeight.GetValueContains(planeRandom) - dirtRandom;
            int stoneHeight = surfaceHeight;
            surfaceHeight += dirtRandom;
            int dirtHeight = surfaceHeight;
            surfaceHeight += 1;

            for (int k = 1; k <= surfaceHeight; k++)
            {
                EVoxelType type;
                if(k <= stoneHeight)
                    type = EVoxelType.Stone;
                else if (k <= dirtHeight)
                    type = EVoxelType.Dirt;
                else
                    type = EVoxelType.Grass;
                Insert(new Int3(index.x,k,index.y),type);
            }

            return surfaceHeight;
        }
 
        void Cave(int2 index,int geometryHeight,float2 p2D)
        {
            if (geometryHeight == 1)
                return;
            
            var r = (float)DVoxel.kChunkSize;
            for (var k = 1; k <= geometryHeight; k++)
            {
                var p3D = new float3(p2D.x,k/r,p2D.y);
            
                var noise = Noise.Perlin.Unit1f3(p3D * terrainData.caveScale);
                if(noise < terrainData.caveValidation)
                    continue;
                Remove(new Int3(index.x,k,index.y));
            }
        }
        
        public void Execute(int _)
        {
            var ind = (int2)identity * DVoxel.kChunkSize;
            float r = DVoxel.kChunkSize;
            for (var i = 0; i < DVoxel.kChunkSize; i++)
            for (var j = 0; j < DVoxel.kChunkSize; j++)
            {
                var index = new int2(i, j);
                var indexWS =  index+ ind ;
                var noiseSample = indexWS / (float2)r + 500;

                var formSample = Noise.Voronoi.Evaluate(noiseSample / terrainData.formScale);
                var formRandom = Noise.Value.Unit1f2(formSample.yz * .5f + .5f);
                var biomeCount = (int)EBiome.Max;
                var terrainForm = (EBiome)((int)(formRandom * biomeCount) % biomeCount);

                Insert(new Int3(i, 0, j), EVoxelType.BedRock);
                var geometryHeight = 0;
                var noiseRandom = Noise.Value.Unit1f2(noiseSample-.5f);

                var genCave = true;
                
                switch (terrainForm)
                {
                    case EBiome.Ocean:
                    {
                        genCave = false;
                        for (int k = 1; k <= terrainData.baseHeight; k++)
                        {
                            Insert(new Int3(i, k, j), EVoxelType.Ocean);
                        }
                    }
                        break;                    
                    case EBiome.Mountains:
                    {
                        var mountainFormStrength = umath.invLerp(terrainData.mountainValidation,1f,1f - formSample.x);
                        if (mountainFormStrength <= 0 || Noise.Simplex.Unit1f2(formSample.yz * .5f + .5f) < .7f)
                        {
                            geometryHeight = Plane(index,terrainData.baseHeight,noiseRandom,noiseSample);
                            break;
                        }
                        
                        var terrainRandom = Noise.Perlin.Unit1f2(noiseSample * terrainData.planeScale);
                        var mountainRandom = Noise.Perlin.Unit1f2(noiseSample * terrainData.mountainScale) *.5f + .5f;

                        var mountainHeight = math.max(terrainData.mountainHeight.GetValueContains(mountainRandom * mountainFormStrength), terrainData.planeHeight.GetValueContains(terrainRandom));
                        geometryHeight = terrainData.baseHeight;
                        geometryHeight +=  mountainHeight;
                        geometryHeight += 1;
                        
                        bool snow = geometryHeight >=  terrainData.mountainForm;
                        
                        for (int k = 1; k <= geometryHeight; k++)
                        {
                            EVoxelType type = EVoxelType.Stone;
                            if (snow && k == geometryHeight)
                                type = EVoxelType.Snow;
                            Insert(new Int3(i,k,j),type);
                        }
                    }
                        break;
                    case EBiome.Plane:
                    {
                        geometryHeight = Plane(index,terrainData.baseHeight,noiseRandom,noiseSample);
                    }
                    break;
                }

                if (!genCave)
                    continue;
                
                Cave(index,geometryHeight,noiseSample);
            }
        }
    }
}