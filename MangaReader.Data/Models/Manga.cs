using LiteDB;

using System;

namespace MangaReader.Data.Models
{
    public class Manga
    {
        [BsonId(true)]
        public int Id { get; set; }

        public string? Name { get; set; }
        public string? path { get; set; }
        public string? CoverUri { get; set; }
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
