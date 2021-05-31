// -----------------------------------------------------------------------
// <copyright file="CompressedPageProvider.cs" company="GihanSoft">
// Copyright (c) 2021 GihanSoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using MangaReader.PagesViewer;

using SharpCompress.Readers;

namespace MangaReader.Controllers
{
    /// <summary>
    /// Provides pages within compressed file (.zip, .rar, etc).
    /// </summary>
    internal class CompressedPageProvider : PagesProvider
    {
        private readonly Dictionary<int, MemoryStream> loadedPages;
        private readonly List<string> pageNames;

        private readonly MemoryStream? compressedStream;
        private readonly string filePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompressedPageProvider"/> class.
        /// </summary>
        /// <param name="stream">Compressed file stream.</param>
        public CompressedPageProvider(MemoryStream stream)
            : this()
        {
            compressedStream = stream;
            Initialize(compressedStream);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompressedPageProvider"/> class.
        /// </summary>
        /// <param name="filePath">Compressed file path.</param>
        public CompressedPageProvider(string filePath)
            : this()
        {
            this.filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));

            using var file = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            Initialize(file);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompressedPageProvider"/> class.
        /// </summary>
        /// <param name="fileUri"><see cref="Uri"/> to compressed file.</param>
        public CompressedPageProvider(Uri fileUri)
            : this(
                  fileUri is null ?
                  throw new ArgumentNullException(nameof(fileUri)) :
                  fileUri.LocalPath ?? throw new ArgumentException("LocalPath not found", nameof(fileUri)))
        {
        }

        private CompressedPageProvider()
        {
            loadedPages = new Dictionary<int, MemoryStream>();
            pageNames = new List<string>();
            filePath = string.Empty;
        }

        /// <summary>
        /// Gets pages count.
        /// </summary>
        public override int Count => pageNames.Count;

        /// <summary>
        /// Gets loaded page. or null if page is not loaded.
        /// </summary>
        /// <param name="page">page number (0-base).</param>
        /// <returns>Page data loaded into a <see cref="MemoryStream"/>.</returns>
        public override MemoryStream? this[int page]
        {
            get
            {
                if (!loadedPages.ContainsKey(page))
                {
                    return null;
                }

                loadedPages[page].Position = 0;
                return loadedPages[page];
            }
        }

        private void Initialize(Stream stream)
        {
            stream.Position = 0;
            using var reader = ReaderFactory.Open(stream);
            while (reader.MoveToNextEntry())
            {
                if (!reader.Entry.IsDirectory && FileTypeList.ImageTypes.Contains(Path.GetExtension(reader.Entry.Key), StringComparer.InvariantCultureIgnoreCase))
                {
                    pageNames.Add(reader.Entry.Key);
                }
            }

            pageNames.Sort(NaturalStringComparer.Default);
        }

        /// <summary>
        /// Load given page into memory stream.
        /// </summary>
        /// <param name="page">page number (0-base).</param>
        /// <returns></returns>
        public override async Task LoadPageAsync(int page)
        {
            if (loadedPages.ContainsKey(page))
            {
                return;
            }

            var name = pageNames[page];

            using var stream =
                compressedStream as Stream ??
                File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            stream.Position = 0;

            using var reader = ReaderFactory.Open(stream);
            loadedPages[page] = await Task.Run(() =>
            {
                do
                {
                    reader.MoveToNextEntry();
                } while (reader.Entry.Key != name);

                var memStream = new MemoryStream();
                reader.WriteEntryTo(memStream);
                memStream.Position = 0;
                return memStream;
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Release memory of given page.
        /// </summary>
        /// <param name="page">Page number (0-base).</param>
        /// <returns></returns>
        public override Task UnLoadPageAsync(int page)
        {
            loadedPages.Remove(page, out var mem);
            mem?.Dispose();
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                var mems = loadedPages?.Values;
                if (mems is null)
                {
                    return;
                }

                foreach (var item in mems)
                {
                    item.Dispose();
                }

                loadedPages?.Clear();
            }

            base.Dispose(disposing);
        }
    }
}