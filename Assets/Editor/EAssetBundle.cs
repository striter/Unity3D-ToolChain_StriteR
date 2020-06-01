using System.Collections;
using UnityEditor;
using UnityEngine;
public class CreateAssetBundles
{
    [MenuItem("Work Flow/AssetBundles/Build For Android(Must Build Before APK Packing)", false,110)]
    static void BuildAllAssetBundlesAndroid()
    {
        BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath, BuildAssetBundleOptions.None, BuildTarget.Android);
    }
}