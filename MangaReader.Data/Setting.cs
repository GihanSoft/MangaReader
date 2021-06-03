// -----------------------------------------------------------------------
// <copyright file="Setting.cs" company="GihanSoft">
// Copyright (c) 2021 GihanSoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace MangaReader.Data
{
    public class Setting
    {
        [LiteDB.BsonId]
        public string Key { get; set; }
        public object Options { get; set; }

        public Setting(string key, object options)
        {
            Key = key;
            Options = options;
        }

        private Setting()
        {
            Key = string.Empty;
            Options = Key;
        }
    }
}
