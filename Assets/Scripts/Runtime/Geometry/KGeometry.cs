using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Geometry
{
    public partial class KQuad
    {
        public static readonly Quad<bool> kFalse = new Quad<bool>(false, false, false, false);
        public static readonly Quad<bool> kTrue = new Quad<bool>(true, true, true, true);
    }
    
    public partial class KQube
    {
        public static readonly Qube<int> kZero = new Qube<int>(0);
        public static readonly Qube<int> kNegOne = new Qube<int>(-1);
        public static readonly Qube<int> kOne = new Qube<int>(1);
        public static readonly Qube<bool> kTrue = new Qube<bool>(true);
        public static readonly Qube<bool> kFalse = new Qube<bool>(false);
        public static readonly Qube<byte> kMaxByte = new Qube<byte>(byte.MaxValue);
        public static readonly Qube<byte> kMinByte = new Qube<byte>(byte.MinValue);
    }

    public static class KEnumQube<T> where T :struct, Enum
    {
        public static Qube<T> kInvalid = new Qube<T>()
        {
            vDB = UEnum.GetInvalid<T>(),vDL = UEnum.GetInvalid<T>(),vDF = UEnum.GetInvalid<T>(),vDR = UEnum.GetInvalid<T>(),
            vTB = UEnum.GetInvalid<T>(),vTL = UEnum.GetInvalid<T>(),vTF = UEnum.GetInvalid<T>(),vTR = UEnum.GetInvalid<T>()
        };
    }
}
