using System.Collections.Generic;

namespace Gihan.Manga.Reader.Controllers
{

    public static class FileTypeList
    {
        public static IEnumerable<string> ImageTypes { get; }
        public static IEnumerable<string> CompressedType { get; }

        static FileTypeList()
        {
            ImageTypes = new[] { "jpg", "jpeg", "png", "bmp", "gif" };
            CompressedType = new[] { "zip", "rar", "cbr", "cbz", "kn" };
        }
    }
}