using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace PolyGrid.Module
{
    public enum EModuleType
    {
        Invalid=-1,
        Green,
        Red,
    }

    [Flags]
    public enum ECornerStatus
    {
        Invalid=-1,
        Bottom=1,
        Body=2,
        Rooftop=4,
    }
    public static class DModule
    {
        public static ECornerStatus GetModuleStatus(int _index,ECornerStatus _conrerStatus)
        {
            if (_conrerStatus == ECornerStatus.Body)
                return _conrerStatus;
            
            switch (_conrerStatus)
            {
                case ECornerStatus.Bottom:
                {
                    if (_index < 4)
                        _conrerStatus = ECornerStatus.Body;
                }
                    break;
                case ECornerStatus.Rooftop:
                {
                    if (_index >= 4)
                        _conrerStatus = ECornerStatus.Body;
                }
                break;
            }
            return _conrerStatus;
        }
        
        public static ECornerStatus GetCornerStatus(byte _cornerHeight, byte _cornerChainMax)
        {
            if (_cornerHeight == byte.MinValue)
                return ECornerStatus.Bottom;
            if (_cornerHeight == _cornerChainMax)
                return ECornerStatus.Rooftop;
            return ECornerStatus.Body;
        }

        public static Color ToColor(this EModuleType _type)
        {
            switch (_type)
            {
                default: return Color.magenta;
                case EModuleType.Red: return Color.red;
                case EModuleType.Green: return Color.green;
            }
        }

        public static Color ToColor(this ECornerStatus _status)
        {
            switch (_status)
            {
                default: throw new InvalidEnumArgumentException();
                case ECornerStatus.Rooftop: return Color.green;
                case ECornerStatus.Body: return Color.yellow;
                case ECornerStatus.Bottom: return Color.blue;
            }
        }
    }
}
