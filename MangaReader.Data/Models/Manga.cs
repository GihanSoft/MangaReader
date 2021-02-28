// <copyright file="Manga.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MangaReader.Data.Models
{
    using System;

    using LiteDB;

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

        public override string ToString()
        {
            return Name ?? "Manga";
        }
    }
}
