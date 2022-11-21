#if UNITY_EDITOR
namespace PCG.Baking
{
    using Module.Cluster;
    using Module.Prop;
    using System.ComponentModel;
    public static class DBaking
    {
        public static string GetPartPileName(EClusterStatus _type, byte _orientByte)
        {
            return GetPartBegin(_type) + _orientByte;
        }
        private static string GetPartBegin(EClusterStatus _type)
        {
            switch (_type)
            {
                default: throw new InvalidEnumArgumentException();
                case EClusterStatus.Common: return "C";
                case EClusterStatus.Spike: return "S";
                case EClusterStatus.Rooftop: return "T";
                case EClusterStatus.Base: return "B";
                case EClusterStatus.Surface: return "S";
                case EClusterStatus.Foundation: return "F";
            }
        }
        
        public static string GetPathName(byte _orientedByte)
        {
            return "P" + _orientedByte;
        }

        public static string GetDecorationName(byte _orientedByte)
        {
            return "D" + _orientedByte;
        }

        private static readonly string kLightingKeyword = "Light";
        private static readonly string kFlowerKeyword = "Flower";
        public static EModulePropType GetPropType(string _name)
        {
            if (_name.StartsWith(kLightingKeyword))
                return EModulePropType.Light;
            if (_name.StartsWith(kFlowerKeyword))
                return EModulePropType.Flower;
            return EModulePropType.Default;
        }
    }
}
#endif
