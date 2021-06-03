// -----------------------------------------------------------------------
// <copyright file="SettingsManager.cs" company="GihanSoft">
// Copyright (c) 2021 GihanSoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace MangaReader.Data
{
    public class SettingsManager
    {
        private readonly DataDb dataDb;

        [CLSCompliant(false)]
        public SettingsManager(DataDb dataDb)
        {
            this.dataDb = dataDb;
        }

        public SettingsManager(string connectionString)
            : this(new DataDb(new LiteDB.LiteDatabase(connectionString)))
        {
        }

        public TOptions? Get<TOptions>(string key) where TOptions : class
            => dataDb.Settings.FindOne(s => s.Key == key)?.Options as TOptions;
        public void Save<TOptions>(string key, TOptions options)
            where TOptions : class
        {
            Setting settings = new(key, options);
            var found = dataDb.Settings.Update(settings);
            if (!found)
            {
                dataDb.Settings.Insert(settings);
            }
        }
    }
}
