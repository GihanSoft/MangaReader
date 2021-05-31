// -----------------------------------------------------------------------
// <copyright file="Manga.cs" company="GihanSoft">
// Copyright (c) 2021 GihanSoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;

using LiteDB;

namespace MangaReader.Data.Models
{
    /// <summary>
    /// Manga Model.
    /// </summary>
    public class Manga
    {
        [BsonId(true)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets title of Manga.
        /// </summary>
        public string? Name { get; set; }

        public string? Path { get; set; }

        public string? Cover { get; set; }

        public int CurrentChapter { get; set; }

        public int CurrentPage { get; set; }

        public double Offset { get; set; }

        public double Zoom { get; set; } = 1;

        /// <inheritdoc/>
        public override string ToString() =>
            Name ??
            (Path is not null ? System.IO.Path.GetFileName(Path) : null) ??
            "Manga";
    }
}
