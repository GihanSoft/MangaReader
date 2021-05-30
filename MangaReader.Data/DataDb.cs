using System;

using LiteDB;

using MangaReader.Data.Models;

namespace MangaReader.Data
{
    [CLSCompliant(false)]
    public class DataDb
    {
        public DataDb(LiteDatabase database)
        {
            Database = database;
            Init();
        }

        public LiteDatabase Database { get; }

        public ILiteCollection<Setting> Settings => Database.GetCollection<Setting>();

        public ILiteCollection<Manga> Mangas => Database.GetCollection<Manga>();

        private void Init()
        {
            Database.GetCollection<Setting>().EnsureIndex(o => o.Key, true);
            Database.GetCollection<Manga>().EnsureIndex(m => m.Name);
        }
    }
}