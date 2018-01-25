using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;

namespace MangaReader
{

    [SuppressUnmanagedCodeSecurity]
    internal static class SafeNativeMethods
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern int StrCmpLogicalW(string psz1, string psz2);
    }

    public sealed class NaturalStringComparer : IComparer<string>
    {
        static NaturalStringComparer _default;
        public static NaturalStringComparer Default
        {
            get
            {
                if (_default == null) _default = new NaturalStringComparer();
                return _default;
            }
        }

        public int Compare(string a, string b)
        {
            return SafeNativeMethods.StrCmpLogicalW(a, b);
        }

        public int MangaCompare(Models.MangaInfo x, Models.MangaInfo y)
        {
            return Compare(x.Name, y.Name);
        }
    }

    public sealed class NaturalFileInfoNameComparer : IComparer<FileInfo>
    {
        public int Compare(FileInfo a, FileInfo b)
        {
            return SafeNativeMethods.StrCmpLogicalW(a.Name, b.Name);
        }
    }
}
