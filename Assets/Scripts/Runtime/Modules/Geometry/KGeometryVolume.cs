using System;
namespace Geometry.Voxel
{
    public partial class KQube
    {
        public static readonly Qube<int> Zero = new Qube<int>(0);
        public static readonly Qube<int> NegOne = new Qube<int>(-1);
        public static readonly Qube<int> One = new Qube<int>(1);
        public static readonly Qube<bool> True = new Qube<bool>(true);
        public static readonly Qube<bool> False = new Qube<bool>(false);
        public static readonly Qube<byte> MaxByte = new Qube<byte>(byte.MaxValue);
        public static readonly Qube<byte> MinByte = new Qube<byte>(byte.MinValue);
    }

    public static class KEnumQube<T> where T :struct, Enum
    {
        public static Qube<T> Invalid = new Qube<T>()
        {
            vDB = UEnum.GetInvalid<T>(),vDL = UEnum.GetInvalid<T>(),vDF = UEnum.GetInvalid<T>(),vDR = UEnum.GetInvalid<T>(),
            vTB = UEnum.GetInvalid<T>(),vTL = UEnum.GetInvalid<T>(),vTF = UEnum.GetInvalid<T>(),vTR = UEnum.GetInvalid<T>()
        };
    }
}