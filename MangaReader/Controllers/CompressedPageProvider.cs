using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using MangaReader.PagesViewer;

using SharpCompress.Readers;

namespace MangaReader.Controllers
{
    public class CompressedPageProvider : PagesProvider
    {
        private readonly Dictionary<int, MemoryStream> loadedPages;
        private readonly List<string> pageNames;

        private readonly MemoryStream? compressedStream;
        private readonly string filePath;

        private CompressedPageProvider()
        {
            loadedPages = new Dictionary<int, MemoryStream>();
            pageNames = new List<string>();
            filePath = string.Empty;
        }

        public CompressedPageProvider(MemoryStream stream) : this()
        {
            compressedStream = stream;
            Initialize(compressedStream);
        }

        public CompressedPageProvider(string filePath) : this()
        {
            this.filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));

            using var file = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            Initialize(file);
        }

        public CompressedPageProvider(Uri fileUri)
            : this(
                  fileUri is null ?
                  throw new ArgumentNullException(nameof(fileUri)) :
                  fileUri.LocalPath ?? throw new ArgumentException("LocalPath not found", nameof(fileUri)))
        {
        }

        private void Initialize(Stream stream)
        {
            stream.Position = 0;
            using var reader = ReaderFactory.Open(stream);
            while (reader.MoveToNextEntry())
            {
                if (!reader.Entry.IsDirectory && FileTypeList.ImageTypes.Contains(Path.GetExtension(reader.Entry.Key), StringComparer.InvariantCultureIgnoreCase))
                    pageNames.Add(reader.Entry.Key);
            }
            pageNames.Sort(NaturalStringComparer.Default);
        }

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

        public override int Count => pageNames.Count;

        public override async Task LoadPageAsync(int page)
        {
            if (loadedPages.ContainsKey(page))
                return;
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

        public override Task UnLoadPageAsync(int page)
        {
            loadedPages.Remove(page, out var mem);
            mem?.Dispose();
            return Task.CompletedTask;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                var mems = loadedPages?.Values;
                if(mems is null)
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