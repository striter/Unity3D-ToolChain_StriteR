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

    public static bool PosValid(byte _src, int _pos)
    {
        var compare = 1 << _pos;
        return (compare & _src) == compare;
    }

    public static int PosValidCount(byte _src)
    {
        int count = 0;
        for(int i=0;i<8;i++)
            if (PosValid(_src, i))
                count++;
        return count;
    }
    
    public static byte ToByte(bool _byte0, bool _byte1, bool _byte2, bool _byte3, bool _byte4, bool _byte5, bool _byte6,
        bool _byte7)
    {
        var bt = ((_byte0 ? 1 : 0) << 0) | ((_byte1 ? 1 : 0) << 1) | ((_byte2 ? 1 : 0) << 2) | ((_byte3 ? 1 : 0) << 3) |
                 ((_byte4 ? 1 : 0) << 4) | ((_byte5 ? 1 : 0) << 5) | ((_byte6 ? 1 : 0) << 6) | ((_byte7 ? 1 : 0) << 7);
        return (byte) bt;
    }
}