using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using MangaReader.Models;

namespace Gihan.Manga.Reader.Controllers
{

    [SuppressUnmanagedCodeSecurity]
    internal static class SafeNativeMethods
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern int StrCmpLogicalW(string psz1, string psz2);
    }

    public sealed class NaturalStringComparer : IComparer<string>
    {
        private static NaturalStringComparer _default;
        public static NaturalStringComparer Default => _default ?? (_default = new NaturalStringComparer());

        public int Compare(string a, string b)
        {
            return SafeNativeMethods.StrCmpLogicalW(a, b);
        }

        public int Compare(MangaInfo a, MangaInfo b)
        {
            return Compare(a.Name, b.Name);
        }
    }
}