using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using MangaReader.Controllers;

namespace GihanSoft.MangaSources.Local
{
    public class LocalPagesProvider : PagesProvider
    {
        private readonly Dictionary<int, MemoryStream> loadedPages;
        private readonly List<string> pagePathes;

        public LocalPagesProvider(string path)
        {
            if (!File.GetAttributes(path).HasFlag(FileAttributes.Directory))
                throw new ArgumentException("is not directory path", nameof(path));

            loadedPages = new Dictionary<int, MemoryStream>();

            DirectoryInfo dir = new(path);
            pagePathes = dir.EnumerateFiles("*", SearchOption.AllDirectories)
                .Where(f => FileTypeList.ImageTypes.Contains(f.Extension))
                .Select(f => f.FullName).ToList();
            pagePathes.Sort(NaturalStringComparer.Default);
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

        public override int Count => pagePathes.Count;

        public override async Task LoadPageAsync(int page)
        {
            if (loadedPages.ContainsKey(page))
                return;
            using var file = File.Open(pagePathes[page], FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var mem = new MemoryStream();
            await file.CopyToAsync(mem).ConfigureAwait(false);
            mem.Position = 0;
            loadedPages[page] = mem;
        }

        public override Task UnLoadPageAsync(int page)
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            foreach (var loaded in loadedPages)
            {
                loaded.Value.Dispose();
            }
            loadedPages.Clear();
        }
    }
}