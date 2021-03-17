using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TEditor
{
    public class EWPlaneMeshGenerator : EditorWindow
    {
        public enum enum_PlaneGeometryType
        {
            Square,
            CircledSquare,
            Hexagon,
        }
        enum_PlaneGeometryType m_GeometryType= enum_PlaneGeometryType.CircledSquare;
        int m_TileRadius=50;
        float m_TileSize = 2f;
        private void OnGUI()
        {
            GUILayout.BeginVertical();
            m_GeometryType = (enum_PlaneGeometryType)EditorGUILayout.EnumPopup("Plane Geometry:",m_GeometryType);
            m_TileRadius = EditorGUILayout.IntField("Grid Radius",m_TileRadius);
            m_TileSize = EditorGUILayout.FloatField("Tile Size", m_TileSize);
            if (GUILayout.Button("Generate Plane"))
            {
                switch(m_GeometryType)
                {
                    case enum_PlaneGeometryType.Hexagon:CreateHexagon(m_TileSize, m_TileRadius); break;
                    case enum_PlaneGeometryType.Square: CreateSquare(m_TileSize, m_TileRadius,false); break;
                    case enum_PlaneGeometryType.CircledSquare:CreateSquare(m_TileSize, m_TileRadius, true); break;
                }
            }
            GUILayout.EndVertical();
        }

        void CreateHexagon(float _tileSize, int _innerRadius)
        {
            //To Be Continued
        }

        void CreateSquare(float _tileSize,int _radius,bool circled)
        {
            Vector3 tileSize = new Vector3(_tileSize, 0, _tileSize);
            List<Vector3> verticies = new List<Vector3>();
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
                    if (circled && (i * i + j * j > _radius * _radius))
                        continue;

                    int verticiesStartIndex = tileIndex * 4;
                    Vector3 v0 = -halfSize + new Vector3(i, 0, j).Multiply(tileSize);
                    Vector3 v1 = -halfSize + new Vector3(i + 1, 0, j).Multiply(tileSize);
                    Vector3 v2 = -halfSize + new Vector3(i, 0, j + 1).Multiply(tileSize);
                    Vector3 v3 = -halfSize + new Vector3(i + 1, 0, j + 1).Multiply(tileSize);

                    verticies.Add(v0);
                    verticies.Add(v1);
                    verticies.Add(v2);
                    verticies.Add(v3);

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

                    int indice0 = verticiesStartIndex + 0;
                    int indice1 = verticiesStartIndex + 1;
                    int indice2 = verticiesStartIndex + 2;
                    int indice3 = verticiesStartIndex + 3;
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
            mesh.SetVertices(verticies);
            mesh.SetUVs(0, uvs);
            mesh.SetNormals(normals);
            mesh.SetTangents(tangents);
            mesh.SetIndices(indices, MeshTopology.Triangles, 0);

            if (!UECommon.SaveFilePath( out string path, "asset", "CustomPlane"))
                return;

            UECommon.CreateOrReplaceMainAsset(mesh, UEPath.FilePathToAssetPath( path));
        }
    }
}
