using System;
using System.Collections.Generic;
using Geometry;
using UnityEngine;
using UnityEngine.Serialization;

namespace Examples.Rendering.Shadows
{
    [ExecuteInEditMode]
    public class SDFRenderer : MonoBehaviour
    {
        public static List<SDFRenderer> sRenderers = new List<SDFRenderer>();
        
        public bool m_ReconstructFromCollider;
        public Capsule[] m_ShapesOS;
        private void OnValidate()
        {
            if (!m_ReconstructFromCollider)
                return;

            var colliders = GetComponentsInChildren<CapsuleCollider>(false);

            m_ShapesOS = new Capsule[colliders.Length];
            int index = 0;
            var worldToLocal = transform.worldToLocalMatrix;
            foreach (var collider in colliders)
                m_ShapesOS[index++] = worldToLocal * new Capsule(collider);
        }

        private void OnEnable()
        {
            sRenderers.Add(this);
        }

        private void OnDisable()
        {
            sRenderers.Remove(this);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            foreach (var shapesOS in m_ShapesOS)
            {
                shapesOS.DrawGizmos();
                shapesOS.GetBoundingSphere().DrawGizmos();
            }
        }
    }
}