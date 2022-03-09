using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Procedural;
using UnityEditor;
using UnityEngine;
using Procedural.Hexagon;
using Procedural.Hexagon.Area;

namespace TEditor
{
    public class PlaneMeshGenerator : EditorWindow
    {
        public enum EPlaneGeometryType
        {
            Plane,
            CircledSquare,
            Hexagon,
        }
        EPlaneGeometryType m_GeometryType= EPlaneGeometryType.CircledSquare;
        int m_TileRadius=50;
        float m_TileSize = 2f;

        private int m_Width = 10;
        private int m_Height = 10;
        private Vector2 m_Pivot = Vector2.one * .5f;
        private void OnGUI()
        {
            GUILayout.BeginVertical();
            m_GeometryType = (EPlaneGeometryType)EditorGUILayout.EnumPopup("Plane Geometry:",m_GeometryType);

            m_TileSize = EditorGUILayout.FloatField("Tile Size", m_TileSize);
            switch (m_GeometryType)
            {
                case EPlaneGeometryType.Hexagon:
                    m_TileRadius = EditorGUILayout.IntField("Grid Radius",m_TileRadius);
                    break;
                case EPlaneGeometryType.Plane:
                    m_Width = EditorGUILayout.IntField("Plane Width",m_Width);
                    m_Height = EditorGUILayout.IntField("Plane Height",m_Height);
                    m_Pivot = EditorGUILayout.Vector2Field("Pivot", m_Pivot);
                    break;
                case EPlaneGeometryType.CircledSquare:
                    m_TileRadius = EditorGUILayout.IntField("Grid Radius",m_TileRadius);
                    break;
            }
            if (GUILayout.Button("Generate Plane"))
            {
                switch(m_GeometryType)
                {
                    case EPlaneGeometryType.Hexagon:CreateHexagon(m_TileSize, m_TileRadius); break;
                    case EPlaneGeometryType.Plane: CreatePlane(m_TileSize, m_Width,m_Height,m_Pivot); break;
                    case EPlaneGeometryType.CircledSquare:CreateCircleSquare(m_TileSize, m_TileRadius); break;
                }
            }
            GUILayout.EndVertical();
        }

        void CreateHexagon(float _tileSize, int _radius)
        {
            if (!UEAsset.SaveFilePath( out string path, "asset", "CustomPlane"))
                return;
            
            List<Vector3> vertices = new List<Vector3>();
            // List<Vector2> uvs = new List<Vector2>();
            List<int> indices = new List<int>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector4> tangents = new List<Vector4>();

            int index = 0;
            foreach (var coord in UHexagon.GetCoordsInRadius( HexCoord.zero,_radius))
            {
                var center = coord.ToCoord();
                vertices.AddRange(UHexagon.GetHexagonPoints().Select(p =>
                {
                    var hexCorner = p + center;
                    return new Vector3(hexCorner.x,0f,hexCorner.y)*_tileSize;
                }));

                for (int i = 0; i < 6; i++)
                {
                    normals.Add(Vector3.up);
                    tangents.Add(Vector3.right.ToVector4(1f));
                }
                
                indices.Add(index+0);
                indices.Add(index+1);
                indices.Add(index+2);
                indices.Add(index+0);
                indices.Add(index+2);
                indices.Add(index+3);
                indices.Add(index+0);
                indices.Add(index+3);
                indices.Add(index+4);
                indices.Add(index+0);
                indices.Add(index+4);
                indices.Add(index+5);
                index += 6;
            }
            
            Mesh mesh = new Mesh();
            mesh.name = "CustomPlane";
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetTangents(tangents);
            mesh.SetIndices(indices, MeshTopology.Triangles, 0);

            UEAsset.CreateOrReplaceMainAsset(mesh, UEPath.FileToAssetPath( path));
        }

        void CreatePlane(float _tileSize,int _width,int _height,Vector2 _pivot)
        {
            if (!UEAsset.SaveFilePath( out string path, "asset", "CustomPlane"))
                return;

            Vector3 tileSize = new Vector3(_tileSize, 0, _tileSize);
            Vector3 size = new Vector3(_width,0f,_height).mul(tileSize);
            Vector3 offset = new Vector3(size.x*_pivot.x,0f,size.z* _pivot.y);
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> indices = new List<int>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector4> tangents = new List<Vector4>();

            Vector2 uv0 = new Vector2(0, 0);
            Vector2 uv1 = new Vector2(1, 0);
            Vector2 uv2 = new Vector2(0, 1);
            Vector2 uv3 = new Vector2(1, 1);

            int tileIndex = 0;
            for (int i=0;i< _width; i++)
            {
                for (int j = 0; j < _height; j++)
                {
                    int startIndex = tileIndex * 4;
                    Vector3 v0 = new Vector3(i, 0, j).mul(tileSize)-offset;
                    Vector3 v1 = new Vector3(i + 1, 0, j).mul(tileSize)-offset;
                    Vector3 v2 = new Vector3(i, 0, j + 1).mul(tileSize)-offset;
                    Vector3 v3 = new Vector3(i + 1, 0, j + 1).mul(tileSize)-offset;

                    vertices.Add(v0);
                    vertices.Add(v1);
                    vertices.Add(v2);
                    vertices.Add(v3);

                    normals.Add(Vector3.up);
                    normals.Add(Vector3.up);
                    normals.Add(Vector3.up);
                    normals.Add(Vector3.up);

                    tangents.Add(new Vector4(1,0,0,1));
                    tangents.Add(new Vector4(1, 0, 0, 1));
                    tangents.Add(new Vector4(1, 0, 0, 1));
                    tangents.Add(new Vector4(1, 0, 0, 1));

                    uvs.Add(uv0);
                    uvs.Add(uv1);
                    uvs.Add(uv2);
                    uvs.Add(uv3);

                    int indice0 = startIndex + 0;
                    int indice1 = startIndex + 1;
                    int indice2 = startIndex + 2;
                    int indice3 = startIndex + 3;
                    indices.Add(indice0);
                    indices.Add(indice2);
                    indices.Add(indice3);
                    indices.Add(indice0);
                    indices.Add(indice3);
                    indices.Add(indice1);
                    tileIndex++;
                }
            }

            Mesh mesh = new Mesh();
            mesh.name = "CustomPlane";
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetNormals(normals);
            mesh.SetTangents(tangents);
            mesh.SetIndices(indices, MeshTopology.Triangles, 0);

            UEAsset.CreateOrReplaceMainAsset(mesh, UEPath.FileToAssetPath( path));
        }
        void CreateCircleSquare(float _tileSize,int _radius)
        {
            if (!UEAsset.SaveFilePath( out string path, "asset", "CustomPlane"))
                return;

            Vector3 tileSize = new Vector3(_tileSize/2f, 0, _tileSize/2f);
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> indices = new List<int>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector4> tangents = new List<Vector4>();

            Vector2 uv0 = new Vector2(0, 0);
            Vector2 uv1 = new Vector2(1, 0);
            Vector2 uv2 = new Vector2(0, 1);
            Vector2 uv3 = new Vector2(1, 1);

            int tileIndex = 0;
            Vector3 halfSize = tileSize / 2;
            for (int i=-_radius+1;i< _radius; i++)
            {
                for (int j = -_radius+1; j < _radius; j++)
                {
                    if ((i * i + j * j > _radius * _radius))
                        continue;

                    int startIndex = tileIndex * 4;
                    Vector3 v0 = -halfSize + new Vector3(i, 0, j).mul(tileSize);
                    Vector3 v1 = -halfSize + new Vector3(i + 1, 0, j).mul(tileSize);
                    Vector3 v2 = -halfSize + new Vector3(i, 0, j + 1).mul(tileSize);
                    Vector3 v3 = -halfSize + new Vector3(i + 1, 0, j + 1).mul(tileSize);

                    vertices.Add(v0);
                    vertices.Add(v1);
                    vertices.Add(v2);
                    vertices.Add(v3);

                    normals.Add(Vector3.up);
                    normals.Add(Vector3.up);
                    normals.Add(Vector3.up);
                    normals.Add(Vector3.up);

                    tangents.Add(new Vector4(1,0,0,1));
                    tangents.Add(new Vector4(1, 0, 0, 1));
                    tangents.Add(new Vector4(1, 0, 0, 1));
                    tangents.Add(new Vector4(1, 0, 0, 1));

                    uvs.Add(uv0);
                    uvs.Add(uv1);
                    uvs.Add(uv2);
                    uvs.Add(uv3);

                    int indice0 = startIndex + 0;
                    int indice1 = startIndex + 1;
                    int indice2 = startIndex + 2;
                    int indice3 = startIndex + 3;
                    indices.Add(indice0);
                    indices.Add(indice2);
                    indices.Add(indice3);
                    indices.Add(indice0);
                    indices.Add(indice3);
                    indices.Add(indice1);
                    tileIndex++;
                }
            }

            Mesh mesh = new Mesh();
            mesh.name = "CustomPlane";
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetNormals(normals);
            mesh.SetTangents(tangents);
            mesh.SetIndices(indices, MeshTopology.Triangles, 0);

            UEAsset.CreateOrReplaceMainAsset(mesh, UEPath.FileToAssetPath( path));
        }
    }
}
