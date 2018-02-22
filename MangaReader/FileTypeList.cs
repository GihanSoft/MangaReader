public static class FileTypeList
{
    public static System.Collections.Generic.IEnumerable<string> ImageTypes { get; }
    public static System.Collections.Generic.IEnumerable<string> CompressedType { get; }

    static FileTypeList()
    {
        ImageTypes = new string[] { "jpg", "jpeg", "png", "bmp", "gif" };
        CompressedType = new string[] { "zip", "rar", "cbr", "cbz", "kn" };
    }
}