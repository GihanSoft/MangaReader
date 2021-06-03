// -----------------------------------------------------------------------
// <copyright file="MainOptions.cs" company="GihanSoft">
// Copyright (c) 2021 GihanSoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace MangaReader.Options
{
    public partial class Appearance
    {
        public WindowPosition WindowPosition { get; set; } = new WindowPosition();

        public string? Theme { get; set; }
    }
}
