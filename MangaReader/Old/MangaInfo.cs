﻿using System;

namespace MangaReader.Models
{
    [Serializable]
    public class MangaInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string CoverAddress { get; set; }
        public int CurrentChapter { get; set; }
        public double CurrentPlace { get; set; }
        public string Zoom { get; set; } = "100";

        public override string ToString()
        {
            return Name;
        }
    }
}