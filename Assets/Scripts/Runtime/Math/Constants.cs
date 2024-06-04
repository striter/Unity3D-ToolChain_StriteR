
public static class kmath
{
    public static readonly float kGoldenRatio = (1f + kSQRT5) / 2f;
    public const float kOneMinusEpsilon = 1f - float.Epsilon;
    public static readonly float kInv2 = 1f / 2;
    public static readonly float kInv3 = 1f / 3;
    public static readonly float kInv6 = 1f / 6;
    public static readonly float kInv9 = 1f / 9;
    
    public const float kSQRT2 = 1.4142135623731f;
    public const float kSQRT3 = 1.7320508075689f;
    public const float kSQRT5 = 2.2360679774998f;
    public const float kSQRT7 = 2.6457513110646f;
    public const float kSQRT15 = 3.8729833462074f;
    public const float kSQRT21 = 4.5825756949558f;
    public const float kSQRT35 = 5.3851648071345f;
    public const float kSQRT105 = 7.0710678118655f;
    
    public static readonly float kSQRT3Half = kSQRT3 / 2f;
    public static readonly float kInvSQRT3 = 1f / kSQRT3;

    public const float kSin0d = 0, kSin30d = 0.5f,     kSin45d=kSQRT2/2f, kSin60d = kSQRT3/2f, kSin90d = 1f,             kSin120d = kSQRT3/2;
    public const float kCos0d = 1, kCos30d = kSQRT3/2, kCos45d=kSQRT2/2f, kCos60d = 0.5f,      kCos90d = 0f,             kCos120d = -1/2f;
    public const float kTan0d = 0, kTan30d = kSQRT3/3, kTan45d = 1,       kTan60d = kSQRT3,    kTan90d = float.MaxValue, kTan120d =-kSQRT3;
    
    public const float kDeg2Rad = 0.017453292519943f;//PI / 180
    public const float kRad2Deg = 57.295779513082f ;//180f / PI;
    
    //PI
    public const float kPI = 3.14159265359f;
    public const float kPIHalf = 1.57079632679f;
    public const float kPI2 = 6.28318530718f;
    public const float kPI4 = 12.5663706144f;
    //Division
    public const float kPIDiv2 = 1.5707963267948966f;
    public const float kPIDiv4 = 0.7853981633974483f;
    public const float kPIDiv8 = 0.3926990817f;
    public const float kPIDiv16 = 0.19634954085f;
    public const float kPiDiv128 = 0.0245436926f;
    
    //Invert
    public const float kInvPI = 0.31830988618f;
    public const float k128InvPi = 40.74366543152f;
    
    public const float kSQRTPi = 1.7724538509055f;

    public static readonly ushort[] kPrimes128 = new ushort[] {
        2,3   ,5   ,7  ,11 ,13 ,17 ,19 ,23 ,29 ,
        31,37  ,41  ,43 ,47 ,53 ,59 ,61 ,67 ,71 ,
        73,79  ,83  ,89 ,97 ,101,103,107,109,113,
        127,131 ,137,139,149,151,157,163,167,173,
        179,181 ,191,193,197,199,211,223,227,229,
        233,239 ,241,251,257,263,269,271,277,281,
        283,293 ,307,311,313,317,331,337,347,349,
        353,359 ,367,373,379,383,389,397,401,409,
        419,421 ,431,433,439,443,449,457,461,463,
        467,479 ,487,491,499,503,509,521,523,541,
        547,557 ,563,569,571,577,587,593,599,601,
        607,613 ,617,619,631,641,643,647,653,659,
        661,673 ,677,683,691,701,709,719
    };

    public static readonly ushort[] kPolys128 = new ushort[]
    {
        1,    3,    7,   11,   13,   19,   25,   37,   59,   47,
        61,   55,   41,   67,   97,   91,  109,  103,  115,  131,
        193,  137,  145,  143,  241,  157,  185,  167,  229,  171,
        213,  191,  253,  203,  211,  239,  247,  285,  369,  299,
        301,  333,  351,  355,  357,  361,  391,  397,  425,  451,
        463,  487,  501,  529,  539,  545,  557,  563,  601,  607,
        617,  623,  631,  637,  647,  661,  675,  677,  687,  695, 
        701,  719,  721,  731,  757,  761,  787,  789,  799,  803,
        817,  827,  847,  859,  865,  875,  877,  883,  895,  901,
        911,  949,  953,  967,  971,  973,  981,  985,  995, 1001,
        1019, 1033, 1051, 1063, 1069, 1125, 1135, 1153, 1163, 1221,
        1239, 1255, 1267, 1279, 1293, 1305, 1315, 1329, 1341, 1347,
        1367, 1387, 1413, 1423, 1431, 1441, 1479, 1509,
    };
}
