using MangaReader.Data.Models;

using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;

namespace MangaReader.Controllers
{

    [SuppressUnmanagedCodeSecurity]
    internal static class SafeNativeMethods
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern int StrCmpLogicalW(string psz1, string psz2);
    }

    public sealed class NaturalStringComparer : IComparer<string?>
    {
        public static NaturalStringComparer Default { get; } = new NaturalStringComparer();

        public int Compare(string? a, string? b)
        {
            if (a is null || b is null)
            {
                if (a is null && b is null)
                {
                    return 0;
                }
                if (a is null)
                {
                    return -1;
                }
                return 1;
            }
            return SafeNativeMethods.StrCmpLogicalW(a, b);
        }

        public int Compare(Manga a, Manga b)
        {
            return Compare(a.Name, b.Name);
        }
    }
}