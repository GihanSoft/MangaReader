using System;
using System.IO;
using System.Threading.Tasks;

namespace MangaReader.Views.Components.PagesViewers
{
    public abstract class PagesProvider : IDisposable
    {
        private bool disposedValue;

        /// <summary>
        /// give loaded page as <see cref="MemoryStream"/> or <see cref="null"/> if not loaded.
        /// </summary>
        /// <param name="page">page number (start from 0).</param>
        public abstract MemoryStream? this[int page] { get; }

        /// <summary>
        /// Pages count
        /// </summary>
        public abstract int Count { get; }

        /// <summary>
        /// give loaded page as <see cref="MemoryStream"/> or <see cref="null"/> if not loaded.
        /// </summary>
        /// <param name="page">page number (start from 0).</param>
        public virtual MemoryStream? GetPage(int page) => this[page];

        /// <summary>
        /// load page from source (web, storage, etc) to <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="page">page number (start from 0).</param>
        public abstract Task LoadPageAsync(int page);

        /// <summary>
        /// unload page from ram to optimize ram usage. better to cache it on storage
        /// </summary>
        /// <param name="page">page number (start from 0).</param>
        public abstract Task UnLoadPageAsync(int page);

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TO DO: dispose managed state (managed objects)
                }

                // TO DO: free unmanaged resources (unmanaged objects) and override finalizer
                // TO DO: set large fields to null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
