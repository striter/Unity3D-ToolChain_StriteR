using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UByte
{
    public static byte ForwardOne(byte _src)
    {
        _src += 1;
        return _src;
    }

    public static byte BackOne(byte _src)
    {
        _src -= 1;
        return _src;
    }

    public static byte[] s_Positions = new byte[] {1 << 0, 1 << 1, 1 << 2, 1 << 3, 1 << 4, 1 << 5, 1 << 6, 1 << 7};
    public static bool PosValid(byte _src, int _pos)
    {
        byte compare = s_Positions[_pos];
        return (compare & _src) == compare;
    }

    public static byte ToByte(bool _byte0, bool _byte1, bool _byte2, bool _byte3, bool _byte4, bool _byte5, bool _byte6,
        bool _byte7)
    {
        var bt = ((_byte0 ? 0 : 1) << 0) | ((_byte1 ? 0 : 1) << 1) | ((_byte2 ? 0 : 1) << 2) | ((_byte3 ? 0 : 1) << 3) |
                 ((_byte4 ? 0 : 1) << 4) | ((_byte5 ? 0 : 1) << 5) | ((_byte6 ? 0 : 1) << 6) | ((_byte7 ? 0 : 1) << 7);
        return (byte) bt;
    }
}