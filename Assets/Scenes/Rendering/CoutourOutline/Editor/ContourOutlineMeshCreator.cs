using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Extensions;
using UnityEditor.Extensions.EditorPath;
using UnityEngine;

namespace Examples.Rendering.ContourOutline.Editor
{
    public static class ContourOutlineMeshCreator
    {
        [MenuItem("Assets/Create/Optimize/ContourOutlineMesh", false, 10)]
        public static void CreateWithSelections()
        {
            var successful = false;
            foreach(var obj in Selection.objects)
            {
                if (obj is not Texture2D { isReadable: true } texture)
                    continue;

                var path = AssetDatabase.GetAssetPath(obj);
                var filePath = path.AssetToFilePath();
    
                var resolution = texture.GetResolution();
                var contourPixels = ContourTracingData.FromColor(texture.width, texture.GetPixels()).MooreNeighborTracing();
                var polygon = UGeometry.GetBoundingPolygon(contourPixels,0.1f);
                var contourPolygon = new G2Polygon(CartographicGeneralization.VisvalingamWhyatt(polygon.Remake(p=>p/resolution),math.min(contourPixels.Count ,10),true));

                var mesh = new Mesh() { name = $"{path.GetFileName().RemoveExtension()}_Contour"};
                mesh.SetVertices(contourPolygon.Select(p=>(Vector3)((p-.5f).to3xy() )).ToList());
                mesh.SetIndices(contourPolygon.GetIndexes().ToArray(),MeshTopology.Triangles,0);
                mesh.SetUVs(0,contourPolygon.Select(p=>(Vector2)p).ToArray());

                UEAsset.CreateOrReplaceMainAsset(mesh,$"{filePath.RemoveExtension()}_Contour.asset".FileToAssetPath());
                successful = true;
            }
        
            if(!successful)
                Debug.LogWarning("No Readable Texture Selected");
        }
    }
}