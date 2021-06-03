// -----------------------------------------------------------------------
// <copyright file="FileTypeList.cs" company="GihanSoft">
// Copyright (c) 2021 GihanSoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;

namespace MangaReader.Controllers
{
    public static class FileTypeList
    {
        public static IEnumerable<string> ImageTypes { get; }
        public static IEnumerable<string> CompressedType { get; }

        static FileTypeList()
        {
            ImageTypes = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".webp" };
            CompressedType = new[] { ".zip", ".rar", ".cbr", ".cbz", ".kn" };
        }
    }
}
