using System.Collections;
using System.Collections.Generic;
using Runtime.Geometry;
using UnityEngine;

namespace Examples.Rendering.Imposter
{
    public class ImposterData : ScriptableObject
    {
        public ImposterInput m_Input;
        public GSphere m_BoundingSphere;
    }
}