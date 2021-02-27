using System;
using System.IO;
using System.Threading.Tasks;

namespace OtakuLib.MangaBase
{
    public abstract class PagesProvider : IDisposable
    {
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
        public virtual MemoryStream GetPage(int page) => this[page];

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

        public abstract void Dispose();
    }
}