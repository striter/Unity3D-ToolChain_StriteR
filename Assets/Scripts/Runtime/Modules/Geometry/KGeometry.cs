using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry
{
    public partial class KQuad
    {
        public static readonly Quad<bool> False = new Quad<bool>(false, false, false, false);
        public static readonly Quad<bool> True = new Quad<bool>(true, true, true, true);
    }
}
