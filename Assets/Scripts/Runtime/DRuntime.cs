
using Unity.Mathematics;
using UnityEngine;

public static class DRuntime
{
    public static readonly string kDataPersistentPath = Application.persistentDataPath + "/Save/";
}

public static class KRect
{
    public static readonly Rect kRect01 = new Rect(Vector2.zero, Vector2.one);
}

public partial class KColor
{
    public static readonly Color kOrange = new Color(1f,0.6f,0,1f);
}