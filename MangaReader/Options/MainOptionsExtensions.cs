// -----------------------------------------------------------------------
// <copyright file="MainOptions.cs" company="GihanSoft">
// Copyright (c) 2021 GihanSoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;

using MangaReader.Data;

namespace MangaReader.Options
{
    public static class MainOptionsExtensions
    {
        public static MainOptions GetMainOptions(this SettingsManager src)
            => src?.Get<MainOptions>(MainOptions.Key) ?? throw new ArgumentNullException(nameof(src));

        public static void SaveMainOptions(this SettingsManager src, MainOptions mainOptions)
        {
            if (src is null)
            {
                throw new ArgumentNullException(nameof(src));
            }

            src.Save(MainOptions.Key, mainOptions);
        }
    }
}
