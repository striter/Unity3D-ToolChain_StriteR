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
    
}