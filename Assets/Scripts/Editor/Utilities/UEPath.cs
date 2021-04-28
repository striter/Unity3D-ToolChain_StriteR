namespace TEditor
{
    public static class UEPath
    {
        public static string FilePathToAssetPath(string path)
        {
            int assetIndex = path.IndexOf("/Assets") + 1;
            if (assetIndex != 0)
                path = path.Substring(assetIndex, path.Length - assetIndex);
            return path;
        }
        public static string RemoveExtension(string path)
        {
            int extensionIndex = path.LastIndexOf('.');
            if (extensionIndex >= 0)
                return path.Remove(extensionIndex);
            return path;
        }
        public static string GetPathName(string path)
        {
            path = RemoveExtension(path);
            int folderIndex = path.LastIndexOf('/');
            if (folderIndex >= 0)
                path = path.Substring(folderIndex + 1, path.Length - folderIndex - 1);
            return path;
        }
    }
}