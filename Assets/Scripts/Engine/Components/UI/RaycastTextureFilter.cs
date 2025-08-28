using System;
using System.Linq;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine.Serialization;

namespace UnityEngine.UI
{
    public class RaycastTextureFilter : MonoBehaviour , ICanvasRaycastFilter , IMaterialModifier
    {
        public enum EMode
        {
            AutomaticConvex,
            AutomaticVoxelize,
            ManualConvex,
        }

        public EMode m_Mode = EMode.AutomaticConvex;

        [Foldout(nameof(m_Mode),EMode.AutomaticVoxelize),IntEnum(8,16,32,64)] public int m_VoxelResolution = 16;
        [Foldout(nameof(m_Mode),EMode.AutomaticConvex),Min(0f)] public float m_Expand = 0f;
        [Foldout(nameof(m_Mode), EMode.ManualConvex)] public G2Polygon m_PolygonNS = G2Polygon.kDefaultUV;
        private G2Voxels m_VoxelNS = G2Voxels.kEmpty;
        private void OnValidate() {
            switch (m_Mode) {
                case EMode.AutomaticConvex: ConstructConvex(m_Expand); break;
                case EMode.AutomaticVoxelize: ConstructVoxels(); break;
            }
        }

        public Material GetModifiedMaterial(Material baseMaterial)
        {
            OnValidate();
            return baseMaterial;
        }

        Texture2D CollectTexture()
        {
            var texture = GetComponent<RawImage>()?.texture;
            if (texture == null)
            {
                var sprite = GetComponent<Image>()?.sprite;
                if (sprite != null)
                    texture = sprite.texture;
            }
            return texture as Texture2D;
        }

        private const float kAlphaClip = .1f;
        private const EDownSample kConvexDownSample = EDownSample.Quarter;
        [InspectorButtonFoldout(nameof(m_Mode), EMode.ManualConvex)]
        void ConstructConvex(float _expand = 0f)
        {
            var texture = CollectTexture();
            if (texture == null)
                return;

            var downSample = (int)kConvexDownSample;
            var contourTracingData = ContourTracingData.FromColor(texture.width / downSample, texture.ReadPixels(downSample),p => p.a > kAlphaClip);
            if(!contourTracingData.ContourAble(int2.zero, out var startPixel))
                return;

            var positions = contourTracingData.TheoPavlidis(startPixel);
            m_PolygonNS = G2Polygon.ConvexHull(positions);
            
            var bounds = G2Box.Minmax(0,new float2(texture.width / downSample, texture.height  / downSample));
            var center = bounds.center;
            m_PolygonNS = new G2Polygon(m_PolygonNS.positions.Select(p => bounds.GetUV((p + .5f) + _expand * (p - center).normalize()).saturate()));
        }

        [InspectorButtonFoldout(nameof(m_Mode),EMode.ManualConvex)]
        void ConstructConcave(float _threshold = .5f,int _desireCount = 32)
        {
            var texture = CollectTexture();
            if (texture == null)
                return;

            var downSample = (int)kConvexDownSample;
            var contourTracingData = ContourTracingData.FromColor(texture.width / downSample, texture.ReadPixels(downSample),p => p.a > kAlphaClip);
            if(!contourTracingData.ContourAble(int2.zero, out var startPixel))
                return;

            var positions = contourTracingData.TheoPavlidis(startPixel);
            if(positions.Count > _desireCount)
                positions = UCartographicGeneralization.VisvalingamWhyatt(positions, _desireCount);
            
            m_PolygonNS = G2Polygon.AlphaShape(positions,_threshold);
            
            var bounds = G2Box.Minmax(0,new float2(texture.width / downSample, texture.height  / downSample));
            m_PolygonNS = new G2Polygon(m_PolygonNS.positions.Select(p => bounds.GetUV((p + .5f) ).saturate()));
        }

        void ConstructVoxels()
        {
            var texture = CollectTexture();
            if (texture == null)
                return;

            var pixels = texture.ReadPixels(m_VoxelResolution,false);
            m_VoxelNS = G2Voxels.FromPixelsNormalized(m_VoxelResolution,pixels,p=>p.a > kAlphaClip);
        }

        private Vector2 m_RaycastPositionLS = Vector2.zero;
        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            var rectTransform = transform as RectTransform;
            var boundsLS = (G2Box)rectTransform.rect;
            Gizmos.DrawWireSphere(m_RaycastPositionLS,1f);
            switch (m_Mode)
            {
                case EMode.ManualConvex:
                case EMode.AutomaticConvex:
                {
                    var polygonLS = new G2Polygon(m_PolygonNS.positions.Select(p => boundsLS.GetPoint(p)));
                    Gizmos.color = polygonLS.Contains(m_RaycastPositionLS)?Color.yellow.SetA(.5f) : Color.white.SetA(.5f);
                    polygonLS.DrawGizmosXY();

                    Gizmos.color = Color.green;
                    foreach (var edge in polygonLS.GetEdges())
                    {
                        if( edge.Intersect(new G2Ray(m_RaycastPositionLS,kfloat2.up),out var distance))
                            edge.DrawGizmosXY();
                    }
                }
                    break;
                case EMode.AutomaticVoxelize:
                {
                    var voxelLS = m_VoxelNS;
                    voxelLS.bounding = G2Box.Minmax(boundsLS.GetPoint(m_VoxelNS.bounding.min),boundsLS.GetPoint(m_VoxelNS.bounding.max));
                    voxelLS.DrawGizmosXY();

                    Gizmos.color = Color.green;
                    foreach (var voxel in voxelLS.GetVoxels())
                    {
                        if (!voxel.Contains(m_RaycastPositionLS)) 
                            continue;
                        voxel.DrawGizmosXY();
                        Gizmos.DrawSphere(voxel.center.to3xy(),5f);
                    }
                }
                    break;
            }
        }
        
        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            var rectTransform = transform as RectTransform;
            var boundsLS = (G2Box)rectTransform.rect;
            if(!rectTransform.TransformScreenToLocal(sp, eventCamera, out var positionLS))
                return false;

            m_RaycastPositionLS = positionLS;
            switch (m_Mode)
            {
                case EMode.AutomaticConvex:
                case EMode.ManualConvex:
                {
                    var polygonLS = new G2Polygon(m_PolygonNS.positions.Select(p => boundsLS.GetPoint(p)));
                    return polygonLS.Contains(positionLS);
                }
                case EMode.AutomaticVoxelize:
                {
                    var voxelLS = m_VoxelNS;
                    voxelLS.bounding = G2Box.Minmax(boundsLS.GetPoint(m_VoxelNS.bounding.min),boundsLS.GetPoint(m_VoxelNS.bounding.max));
                    return voxelLS.Contains(positionLS);
                }
            }

            return false;
        }

    }
}