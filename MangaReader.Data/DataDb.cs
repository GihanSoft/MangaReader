using System;

using LiteDB;

using MangaReader.Data.Models;

namespace MangaReader.Data
{
    public class DataDb : IDisposable
    {
        private bool disposedValue;

        public DataDb(ConnectionString connectionString)
        {
            Database = new LiteDatabase(connectionString);
            Init();
        }

        public LiteDatabase Database { get; }

        public ILiteCollection<Setting> Settings =>
            disposedValue ? throw new ObjectDisposedException(nameof(Settings)) :
            Database.GetCollection<Setting>();

        public ILiteCollection<Manga> Mangas =>
            disposedValue ? throw new ObjectDisposedException(nameof(Settings)) :
            Database.GetCollection<Manga>();

        private void Init()
        {
            Database.GetCollection<Setting>().EnsureIndex(o => o.Key, true);
            Database.GetCollection<Manga>().EnsureIndex(m => m.Name);
        }

        #region Dispose

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Database.Checkpoint();
                    Database.Dispose();
                    // dispose managed state (managed objects)
                }

                // free unmanaged resources (unmanaged objects) and override finalizer
                // set large fields to null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion Dispose
    }
}