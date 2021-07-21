using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public enum enum_Option_LanguageRegion
{
    CN=1,
    EN=2,
    EU,
}
public static class TLocalization 
{
    public static bool IsInit = false;
    public static event Action OnLocaleChanged;
    public static enum_Option_LanguageRegion e_CurLocation { get; private set; }
    static Dictionary<string, string> CurLocalization = new Dictionary<string, string>();
    public static void SetRegion(enum_Option_LanguageRegion location)
    {
        if (e_CurLocation == location)
            return;

        e_CurLocation = location;

        List<string[]> data = TExcel.Tools.ReadExcelFirstSheetData(Resources.Load<TextAsset>("Excel/SLocalization"));

        for (int i = 1; i < data[0].Length; i++)
        {
            if (data[0][i] != ((enum_Option_LanguageRegion)i).ToString())
                Debug.LogError("SLocalizataion Not Init Propertly:" + i.ToString());    
        }

        CurLocalization.Clear();
        int localizeIndex = (int)e_CurLocation;
        for (int i = 0; i < data.Count; i++)
            CurLocalization.Add(data[i][0], data[i][localizeIndex]);
        OnLocaleChanged?.Invoke();
        IsInit = true;
    }
    public static bool CheckKeyLocalizable(string key)=>key!=null&&CurLocalization.ContainsKey(key);
    public static bool CheckValueLocalized(string value) =>value!=null&&CurLocalization.ContainsValue(value);
    public static string GetLocalizeValue(this string key)
    {
        if(CheckKeyLocalizable(key))
            return CurLocalization[key.Replace("\\n", "\n")];

        Debug.LogWarning("Localization Key:(" + key + ") Not Found In SLocalization ");
        return key;
    }
    public static string FindLocalizeKey(this string value)
    {
        if (CheckValueLocalized(value))
            return CurLocalization.Keys.ToList().Find(p=>CurLocalization[p]==value);

        Debug.LogWarning("Localization Key:(" + value + ") Not Found In SLocalization ");
        return value;
    }
}
