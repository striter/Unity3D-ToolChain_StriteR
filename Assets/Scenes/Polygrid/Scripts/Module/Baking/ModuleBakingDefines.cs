using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace PolyGrid.Module.Baking
{
    public static class ModuleBakingDefines
    {
        public static string GetModuleName(ECornerStatus _type,byte _orientByte)
        {
            return GetStartName(_type) + _orientByte.ToString();
        }

        private static string GetStartName(ECornerStatus _type)
        {
            switch (_type)
            {
                default: throw new InvalidEnumArgumentException();
                case ECornerStatus.Bottom: return "B";
                case ECornerStatus.Rooftop: return "T";
                case ECornerStatus.Body: return "M";
            }
        }
    }
}