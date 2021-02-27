using OtakuLib.MangaBase;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Gihan.Manga.Views.Custom
{
    public abstract class PagesViewer : UserControl
    {
        protected MemoryStream[] _inMemoryStreams;
        protected Stream[] _sourceStreams;
        protected BitmapImage[] _bitmaps;
        protected Image[] _images;

        public virtual IEnumerable<Stream> SourceImageStreams
        {
            get => _sourceStreams != null ? Array.AsReadOnly(_sourceStreams) : null;
            //set => SetSourceStreams(value, Page);
        }
        public virtual IEnumerable<MemoryStream> InMemoryImageStreams
            => Array.AsReadOnly(_inMemoryStreams);
        public abstract double Zoom { get; set; }
        public abstract double Offset { get; set; }
        public abstract int Page { get; set; }

        protected void LoadBitmap(int page)
        {
            if (_bitmaps[page] != null) return;
            _bitmaps[page] = new BitmapImage();
            _bitmaps[page].BeginInit();
            _inMemoryStreams[page].Seek(0, SeekOrigin.Begin);
            _bitmaps[page].StreamSource = _inMemoryStreams[page];
            _bitmaps[page].EndInit();
        }
        protected void LoadPageStream(int page)
        {
            if (_inMemoryStreams[page] is null)
            {
                _inMemoryStreams[page] = new MemoryStream();
                if (_sourceStreams[page].CanSeek)
                    _sourceStreams[page].Seek(0, SeekOrigin.Begin);
                /*else
                    throw new Exception($"Source stream of page {page} is not seekable");*/
                _sourceStreams[page].CopyTo(_inMemoryStreams[page]);
            }
        }
        protected virtual void SetSourceStreams(IEnumerable<Stream> streams)
        {
            _sourceStreams = streams as Stream[] ?? streams.ToArray();
            var pagesCount = _sourceStreams.Length;
            if (_inMemoryStreams != null)
                foreach (var stream in _inMemoryStreams)
                    stream?.Dispose();
            _inMemoryStreams = new MemoryStream[pagesCount];
            for (int i = 0; i < pagesCount; i++)
            {
                if (_sourceStreams[i] is MemoryStream memoryStream)
                    _inMemoryStreams[i] = memoryStream;
            }
            _images = new Image[pagesCount];
            _bitmaps = new BitmapImage[pagesCount];
        }
        public async virtual void SetSourceStreams(PagesProvider pagesProvider, int page)
        {
            List<Stream> streams = new List<Stream>();
            for (int i = 0; i < pagesProvider.Count; i++)
            {
                await pagesProvider.LoadPageAsync(i);
                streams.Add(pagesProvider[i]);
            }
            SetSourceStreams(streams);
            Page = page > 0 ? page : 1;
        }
    }
}