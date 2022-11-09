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

    public class KCube
    {
        public const int kSideCount = 6;
        public static Axis GetCubeSide(int _index)
        {
            switch (_index)
            {
                default: throw new Exception("Invalid Index");
                case 0: return new Axis() {index = 0, origin = -Vector3.one, uDir = new Vector3(2f, 0, 0), vDir = new Vector3(0, 2f, 0)};
                case 1: return new Axis() {index = 1, origin = -Vector3.one, uDir = new Vector3(0, 2f, 0), vDir = new Vector3(0f, 0, 2f)};
                case 2: return new Axis()  {index = 2, origin = -Vector3.one, uDir = new Vector3(0, 0, 2f), vDir = new Vector3(2f, 0, 0)};
                case 3: return new Axis()  {  index = 3, origin = new Vector3(-1f, -1f, 1f), uDir = new Vector3(0, 2f, 0),  vDir = new Vector3(2f, 0, 0) };
                case 4: return new Axis()  {  index = 4, origin = new Vector3(1f, -1f, -1f), uDir = new Vector3(0, 0, 2f),  vDir = new Vector3(0, 2f, 0f) };
                case 5: return new Axis() { index = 5, origin = new Vector3(-1f, 1f, -1f), uDir = new Vector3(2f, 0f, 0),  vDir = new Vector3(0f, 0, 2f)  };
            }
        }
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
